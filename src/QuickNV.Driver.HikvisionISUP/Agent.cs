using QuickNV.HikvisionISUPSDK.Api;
using QuickNV.HikvisionISUPSDK.Api.Rtp;
using Quick.Fields.AppSettings;
using Quick.Protocol;
using System.Text;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;
using QuickNV.Driver.Protocol.QpCommands.CreateChannelPlaybackStream;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.HikvisionISUP
{
    public class Agent : AbstractDriverAgent<ConfigModel, DeviceConfig, ChannelConfig>
    {
        public static Agent Instance { get; private set; }
        protected override JsonSerializerContext ConfigSerializerContext => ConfigModelSerializerContext.Default;
        protected override ConfigModel ReadConfig()=>Functions.Config.Instance.ReadConfig();
        public override bool HasChannelConfig => true;
        public override bool CanImportChannel => true;

        private Dictionary<string, DeviceContext> deviceContextDict = new Dictionary<string, DeviceContext>();
        private DeviceContext[] deviceContexts = new DeviceContext[0];
        
        private Dictionary<int, int> previewSessionIdLinkHandleDict = new Dictionary<int, int>();
        private Dictionary<int, StreamPushContext> linkHandleStreamPushContextDict = new Dictionary<int, StreamPushContext>();
        private string containerFolder = string.Empty;
        private SmsContext smsContext;
        public CmsContext CmsContext { get; private set; }

        public Agent()
        {
            Instance = this;
        }

        public DeviceContext GetDeviceContext(string deviceId)
        {
            lock (deviceContextDict)
            {
                if (deviceContextDict.TryGetValue(deviceId, out DeviceContext deviceContext))
                    return deviceContext;
                return null;
            }
        }

        public override void Init()
        {
            CmsContext.Init();
            SmsContext.Init();
            AgentContext.LogInfo($"海康ISUP SDK初始化完成");
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            base.Init();
            AddFunction(new Functions.Config());
            AddFunction(new YiQiDong.Core.Functions.QpChannelView(() => Client), true);
        }

        public override void Start()
        {
            base.Start();

            smsContext = new SmsContext(new SmsContextOptions()
            {
                ListenIPAddress = Config.SmsListenIPAddress,
                ListenPort = Config.SmsListenPort,
                LinkMode = Config.SmsLinkMode
            });
            smsContext.PreviewNewlink += SmsContext_PreviewNewlink;
            smsContext.PreviewData += SmsContext_PreviewPsData;
            AgentContext.LogInfo($"[ISUP媒体转发服务]启动中...");
            smsContext.Start();
            AgentContext.LogInfo($"[ISUP媒体转发服务]启动完成。监听端点：{Config.SmsListenIPAddress}:{Config.SmsListenPort}");
            CmsContext = new CmsContext(new CmsContextOptions()
            {
                ListenIPAddress = Config.CmsListenIPAddress,
                ListenPort = Config.CmsListenPort,
                Encoding = Encoding.GetEncoding(Config.CmsEncoding),
                AccessSecurity = Config.CmsAccessSecurity,
                ServerHost = Config.CmsPublicIPAddress,
                ServerPort = Config.CmsPublicPort,
                Password = Config.CmsPassword
            });
            CmsContext.DeviceOnline += CmsContext_DeviceOnline;
            CmsContext.DeviceOffline += CmsContext_DeviceOffline;
            AgentContext.LogInfo($"[ISUP中心服务]启动中...");
            CmsContext.Start();
            AgentContext.LogInfo($"[ISUP中心服务]启动完成。监听端点：{Config.CmsListenIPAddress}:{Config.CmsListenPort}");
        }

        private void CmsContext_DeviceOffline(object sender, QuickNV.HikvisionISUPSDK.Api.DeviceContext e)
        {
            AgentContext.LogInfo($"[ISUP中心服务]设备下线。Id:{e.Id},名称:{e.Name}");

            foreach (var device in deviceContexts)
            {
                if (device.Model.Config.SdkDeviceId == e.Id)
                {
                    device.ApiContext = null;
                    SendDeviceOfflineNotice(device.Model.Id, "设备下线");
                }
            }
        }

        private void CmsContext_DeviceOnline(object sender, QuickNV.HikvisionISUPSDK.Api.DeviceContext e)
        {
            AgentContext.LogInfo($"[ISUP中心服务]设备上线。Id:{e.Id},名称:{e.Name}");
            foreach (var device in deviceContexts)
            {
                if (device.Model.Config.SdkDeviceId == e.Id)
                {
                    device.ApiContext = e;
                    SendDeviceOnlineNotice(device.Model);
                }
            }
        }

        private void SmsContext_PreviewNewlink(object sender, SmsContextPreviewNewlinkEventArgs e)
        {
            //AgentContext.LogDebug($"[ISUP媒体转发服务]新预览连接。LinkHandle:{e.LinkHandle},SessionId:{e.SessionId},设备:{e.DeviceId},通道:{e.ChannelId},流格式:{e.StreamFormat},流类型:{e.StreamType}");
            lock (previewSessionIdLinkHandleDict)
                previewSessionIdLinkHandleDict[e.SessionId] = e.LinkHandle;
            e.Allowed = true;
        }

        private void SmsContext_PreviewPsData(object sender, SmsContextPreviewDataEventArgs e)
        {
            StreamPushContext streamPushContext = null;
            try
            {
                if (!linkHandleStreamPushContextDict.TryGetValue(e.LinkHandle, out streamPushContext))
                    return;
                if (streamPushContext == null)
                    return;
                var rtpSender = streamPushContext.RtpSender;
                if (rtpSender == null)
                    return;
                rtpSender.Write(e.GetDataSpan());
            }
            catch
            {
                if (streamPushContext != null)
                {
                    try
                    {
                        streamPushContext.Device.ApiContext.StopGetRealStream(streamPushContext.SessionId);
                    }
                    catch { }
                    lock (linkHandleStreamPushContextDict)
                        linkHandleStreamPushContextDict.Remove(e.LinkHandle);
                    streamPushContext.RtpSender.Dispose();
                }
            }
        }

        public override void Stop()
        {
            CmsContext.DeviceOnline -= CmsContext_DeviceOnline;
            CmsContext.DeviceOffline -= CmsContext_DeviceOffline;
            CmsContext.Stop();
            AgentContext.LogInfo($"[ISUP中心服务]已停止");

            smsContext.PreviewNewlink -= SmsContext_PreviewNewlink;
            smsContext.PreviewData -= SmsContext_PreviewPsData;
            smsContext.Stop();
            AgentContext.LogInfo($"[ISUP媒体转发服务]已停止");

            base.Stop();

            OnDriverDisconnected();
        }

        protected override Protocol.QpCommands.ImportDevices.Response ImportDevices(QpChannel channel, Protocol.QpCommands.ImportDevices.Request request)
            => Functions.ImportDevicesFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.GetDeviceConfig.Response GetDeviceConfig(QpChannel channel, Protocol.QpCommands.GetDeviceConfig.Request request)
            => Functions.GetDeviceConfigFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.ImportChannels.Response ImportChannels(
            QpChannel channel,
            Protocol.QpCommands.ImportChannels.Request request)
            => Functions.ImportChannelsFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.GetChannelConfig.Response GetChannelConfig(QpChannel channel, Protocol.QpCommands.GetChannelConfig.Request request)
            => Functions.GetChannelConfigFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.CreateChannelLiveStream.Response CreateChannelLiveStream(QpChannel channel, Protocol.QpCommands.CreateChannelLiveStream.Request request)
        {
            var deviceContext = GetDeviceContext(request.MediaInfo.Device.Id);
            if (!deviceContext.IsOnline)
                throw new IOException("设备不在线");

            var deviceApiContext = deviceContext.ApiContext;
            var channelInfo = deviceContext.Model.GetChannel(request.MediaInfo.Channel.Id);

            //AgentContext.LogDebug($"[ISUP媒体转发服务]获取流信息。设备:{deviceContext.ApiContext.Id},通道:{channelInfo.Config.ChannelId},连接模式:{Config.SmsLinkMode},流类型:{channelInfo.Config.StreamType}");
            //获取流信息
            var sessionId = deviceApiContext.StartGetRealStreamV11(
                Config.SmsPublicIPAddress, Config.SmsListenPort,
                channelInfo.Config.ChannelId,
                Config.SmsLinkMode, channelInfo.Config.StreamType);
            //开始推流
            deviceApiContext.StartPushRealStream(sessionId);
            //查找LinkHandle，最多等待5秒
            int linkHandle = -1;
            for (var i = 0; i < 5; i++)
            {
                //如果根据SessionId未找到对应的LinkHandle
                lock (previewSessionIdLinkHandleDict)
                {
                    //如果已经找到
                    if (previewSessionIdLinkHandleDict.TryGetValue(sessionId, out linkHandle))
                    {
                        previewSessionIdLinkHandleDict.Remove(sessionId);
                        break;
                    }
                    //如果未找到
                    else
                    {
                        linkHandle = -1;
                    }
                }
                Thread.Sleep(1000);
            }
            if (linkHandle < 0)
            {
                try { deviceApiContext.StopGetRealStream(sessionId); }
                catch { }
                throw new IOException("根据SessionId未找到对应的LinkHandle");
            }
            //AgentContext.LogDebug($"[ISUP媒体转发服务]根据SessionId[{sessionId}]找到LinkHandle[{linkHandle}]");
            //准备RTP推送器
            var rtpSenderOptions = new RtpSenderOptions()
            {
                Host = request.MediaServerInfo.PublicIpAddress,
                Port = request.MediaServerInfo.RtpProxyPort,
                SSRC = Convert.ToUInt32(request.MediaInfo.MediaId)
            };
            RtpSender rtpSender = null;
            switch (deviceContext.Model.Config.StreamTransferMode)
            {
                case SmsLinkMode.TCP:
                    rtpSender = new TcpRtpSender(rtpSenderOptions);
                    break;
                case SmsLinkMode.UDP:
                default:
                    rtpSender = new UdpRtpSender(rtpSenderOptions);
                    break;
            }
            rtpSender.Connect();
            var currentStreamPushContext = new StreamPushContext(deviceContext, channelInfo, rtpSender);
            lock(linkHandleStreamPushContextDict)
                linkHandleStreamPushContextDict[linkHandle] = currentStreamPushContext;

            //等待媒体服务器的on_publish回调
            StreamInfo liveStream_StreamInfo = null;
            try
            {
                //等待流注册
                liveStream_StreamInfo = GetMediaServerStreamInfo(request.MediaServerInfo.Id, request.MediaInfo.MediaId).Result;
            }
            catch (TimeoutException)
            {
                throw new TimeoutException("等待流注册超时");
            }
            return new Protocol.QpCommands.CreateChannelLiveStream.Response()
            {
                LiveStreamInfo = liveStream_StreamInfo
            };
        }

        protected override Response CreateChannelPlaybackStream(QpChannel channel, Request request)
        {
            throw new NotImplementedException();
        }

        protected override Protocol.QpCommands.DestoryChannelStream.Response DestoryChannelStream(QpChannel channel, Protocol.QpCommands.DestoryChannelStream.Request request)
        {
            lock (linkHandleStreamPushContextDict)
            {
                foreach (var item in linkHandleStreamPushContextDict.ToArray())
                {
                    var handle = item.Key;
                    var streamPushContext = item.Value;
                    var deviceContext = streamPushContext.Device;
                    if (deviceContext.Model.Id == request.DeviceId
                        && streamPushContext.Channel.Id == request.ChannelId)
                    {
                        deviceContext.ApiContext.StopGetRealStream(streamPushContext.SessionId);
                        streamPushContext.RtpSender.Dispose();
                    }
                }
            }
            return new Protocol.QpCommands.DestoryChannelStream.Response();
        }

        protected override void OnDriverConnected()
        {
            lock (deviceContextDict)
            {
                foreach (var device in GetDevices())
                {
                    var deviceContext = new DeviceContext(device);
                    deviceContext.ApiContext = CmsContext.GetDevice(device.Config.SdkDeviceId);
                    deviceContextDict[device.Id] = deviceContext;
                    if (deviceContext.IsOnline)
                        SendDeviceOnlineNotice(device);
                    else
                        SendDeviceOfflineNotice(device.Id);
                }
                deviceContexts = deviceContextDict.Values.ToArray();
            }
        }

        protected override void OnDriverDisconnected()
        {
            //销毁全部的流
            foreach (var deviceContext in deviceContexts)
            {
                deviceContext.Dispose();
            }
            lock (deviceContextDict)
            {
                deviceContextDict.Clear();
                deviceContexts = deviceContextDict.Values.ToArray();
            }
        }

        protected override void OnDeviceAdded(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            var deviceContext = new DeviceContext(device);
            deviceContext.ApiContext = CmsContext.GetDevice(deviceContext.Model.Config.SdkDeviceId);

            lock (deviceContextDict)
            {
                deviceContextDict[device.Id] = deviceContext;
                deviceContexts = deviceContextDict.Values.ToArray();
            }
            if (deviceContext.IsOnline)
                SendDeviceOnlineNotice(device);
        }

        protected override void OnDeviceDeleted(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            lock (deviceContextDict)
            {
                if (!deviceContextDict.TryGetValue(device.Id, out var deviceContext))
                    return;
                deviceContextDict.Remove(device.Id);
                deviceContext.Dispose();
                deviceContexts = deviceContextDict.Values.ToArray();
            }
        }

        protected override Protocol.QpCommands.PtzControl.Response PtzControl(QpChannel channel, Protocol.QpCommands.PtzControl.Request request)
        {
            var deviceContext = GetDeviceContext(request.DeviceId);
            if (deviceContext != null)
            {
                var channelConfig = deviceContext.Model.GetChannel(request.ChannelId)?.Config;
                if (channelConfig != null)
                    try
                    {
                        deviceContext.PtzControl(channelConfig.ChannelId, request.CommandType, request.MoveSpeed);
                    }
                    catch (Exception ex)
                    {
                        AgentContext.LogDebug($"PTZ Error." + ExceptionUtils.GetExceptionMessage(ex));
                    }
            }
            return new Protocol.QpCommands.PtzControl.Response();
        }
    }
}
