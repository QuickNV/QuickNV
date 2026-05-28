using Quick.Protocol;
using System.Text;
using System.Text.Json.Serialization;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.GB28181
{
    public class Agent : AbstractDriverAgent<ConfigModel, DeviceConfig, ChannelConfig>
    {
        public static Agent Instance { get; private set; }
        protected override JsonSerializerContext ConfigSerializerContext => ConfigModelSerializerContext.Default;
        protected override ConfigModel ReadConfig()=>Functions.Config.Instance.ReadConfig();
        public override bool CanImportChannel => true;

        public SipServer SipServer { get; private set; }

        public Agent()
        {
            Instance = this;
        }

        public override void Init()
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            base.Init();
            AddFunction(new Functions.Config());
            AddFunction(new YiQiDong.Core.Functions.QpChannelView(() => Client), true);
        }

        public override void Start()
        {
            base.Start();

            //启动SIP服务器
            try
            {
                SipServer = new SipServer(Config);
                SipServer.DeviceOnlineStateChanged += SipServer_DeviceOnlineStateChanged;
                SipServer.Start();
            }
            catch (Exception ex)
            {
                AgentContext.LogError($"启动SIP服务器时出错，原因：{ExceptionUtils.GetExceptionString(ex)}");
                throw;
            }
        }

        private void SipServer_DeviceOnlineStateChanged(object sender, DeviceOnlineStateChangedEventArgs e)
        {
            var deviceContext = e.DeviceContext;
            var deviceModel = deviceContext.Model;
            if (e.IsOnline)
            {
                SendDeviceOnlineNotice(deviceModel);
            }
            else
            {
                SendDeviceOfflineNotice(deviceModel.Id, e.Reason);
            }
        }

        public override void Stop()
        {
            foreach (var deviceContext in SipServer.GetDevices())
                deviceContext.Unregister("驱动停止");
            if (SipServer != null)
            {
                SipServer.Stop();
                SipServer.DeviceOnlineStateChanged -= SipServer_DeviceOnlineStateChanged;
                SipServer = null;
            }
            base.Stop();
        }

        protected override void OnDeviceAdded(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            var deviceContext = SipServer.GetDevice(device.Id);
            if (deviceContext != null)
                SipServer_DeviceOnlineStateChanged(SipServer, new DeviceOnlineStateChangedEventArgs()
                {
                    DeviceContext = deviceContext,
                    IsOnline = deviceContext.IsOnline
                });
        }

        protected override void OnDeviceDeleted(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            var deviceContext = SipServer.GetDevice(device.Id);
            if (deviceContext != null)
            {
                foreach (var channel in deviceContext.GetChannels())
                    channel.DestoryLiveStream();
            }
        }

        protected override Protocol.QpCommands.GetDeviceConfig.Response GetDeviceConfig(
            QpChannel channel,
            Protocol.QpCommands.GetDeviceConfig.Request request)
            => Functions.GetDeviceConfigFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.GetChannelConfig.Response GetChannelConfig(
            QpChannel channel,
            Protocol.QpCommands.GetChannelConfig.Request request)
            => Functions.GetChannelConfigFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.ImportDevices.Response ImportDevices(
            QpChannel channel,
            Protocol.QpCommands.ImportDevices.Request request)
            => Functions.ImportDevicesFunction.Invoke(channel, request);
        protected override Protocol.QpCommands.ImportChannels.Response ImportChannels(
            QpChannel channel,
            Protocol.QpCommands.ImportChannels.Request request)
            => Functions.ImportChannelsFunction.Invoke(channel, request);

        protected override void OnDriverConnected()
        {
            //发送一次全部设备的在线状态
            foreach (var device in GetDevices())
            {
                var deviceContext = SipServer.GetDevice(device.Id);
                var isOnline = deviceContext != null && deviceContext.IsOnline;
                if (isOnline)
                {
                    SendDeviceOnlineNotice(device);
                }
                else
                {
                    SendDeviceOfflineNotice(device.Id);
                }
            }
        }

        protected override void OnDriverDisconnected()
        {
            //销毁全部的流
            foreach (var device in GetDevices())
            {
                var deviceContext = SipServer.GetDevice(device.Id);
                if (deviceContext == null)
                    continue;
                foreach (var channel in deviceContext.GetChannels())
                    channel.DestoryLiveStream();
            }
        }

        protected override Protocol.QpCommands.CreateChannelLiveStream.Response CreateChannelLiveStream(QpChannel channel, Protocol.QpCommands.CreateChannelLiveStream.Request request)
        {
            var channelContext = GetChannelContext(request.MediaInfo.Device.Id, request.MediaInfo.Channel.Id);
            var streamInfo = channelContext.CreateLiveStream(request.MediaServerInfo, request.MediaInfo).Result;
            return new Protocol.QpCommands.CreateChannelLiveStream.Response()
            {
                LiveStreamInfo = streamInfo
            };
        }

        protected override Protocol.QpCommands.CreateChannelPlaybackStream.Response CreateChannelPlaybackStream(QpChannel channel, Protocol.QpCommands.CreateChannelPlaybackStream.Request request)
        {
            var channelContext = GetChannelContext(request.MediaInfo.Device.Id, request.MediaInfo.Channel.Id);
            var streamInfo = channelContext.CreatePlaybackStream(request.MediaServerInfo, request.MediaInfo, request.StartTime, request.EndTime).Result;
            return new Protocol.QpCommands.CreateChannelPlaybackStream.Response()
            {
                PlaybackStreamInfo = streamInfo
            };
        }

        private ChannelContext GetChannelContext(string deviceId, string channelId)
        {
            var deviceContext = SipServer.GetDevice(deviceId);
            if (deviceContext == null)
                throw new ApplicationException($"未找到编号为[{deviceId}]的设备");
            var channelContext = deviceContext.GetChannel(channelId);
            if (channelContext == null)
                throw new ApplicationException($"设备[{deviceId}]中未找到编号为[{channelId}]的通道");
            return channelContext;
        }

        protected override Protocol.QpCommands.DestoryChannelStream.Response DestoryChannelStream(QpChannel channel, Protocol.QpCommands.DestoryChannelStream.Request request)
        {
            var channelContext = GetChannelContext(request.DeviceId, request.ChannelId);
            channelContext.DestoryLiveStream();
            return new Protocol.QpCommands.DestoryChannelStream.Response();
        }

        protected override VideoFileInfo[] FindPlaybackFiles(string deviceId, string channelId, DateTime startTime, DateTime endTime)
        {
            var deviceContext = SipServer.GetDevice(deviceId);
            if (deviceContext == null)
                throw new IOException($"[SIP服务端]编号为[{deviceId}]的设备当前没有注册");
            var channelContext = deviceContext.GetChannel(channelId);
            if (channelContext == null)
                throw new IOException($"[SIP服务端]在设备[{deviceId}]的注册信息中未找到编号为[{channelId}]的通道。");
            return channelContext.FindPlaybackFiles(startTime, endTime).Result;
        }

        protected override Protocol.QpCommands.PtzControl.Response PtzControl(QpChannel channel, Protocol.QpCommands.PtzControl.Request request)
        {
            var deviceContext = SipServer.GetDevice(request.DeviceId);
            if (deviceContext == null)
                throw new ApplicationException($"未找到编号为[{request.DeviceId}]的设备");
            var channelContext = deviceContext.GetChannel(request.ChannelId);
            if (channelContext == null)
                throw new ApplicationException($"设备[{request.DeviceId}]中未找到编号为[{request.ChannelId}]的通道");
            var speed = Convert.ToByte(request.MoveSpeed * byte.MaxValue);
            channelContext.SendPtzCommandAsync(request.CommandType, speed).Wait(5000);
            return new Protocol.QpCommands.PtzControl.Response();
        }
    }
}
