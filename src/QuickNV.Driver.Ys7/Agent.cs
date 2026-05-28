using Quick.Fields.AppSettings;
using Quick.Protocol;
using QuickNV.YS7;
using System.Text;
using System.Text.Json.Serialization;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpCommands.CreateChannelPlaybackStream;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Ys7
{
    public class Agent : AbstractDriverAgent<ConfigModel, DeviceConfig, ChannelConfig>
    {
        public static Agent Instance { get; private set; }
        protected override JsonSerializerContext ConfigSerializerContext => ConfigModelSerializerContext.Default;
        protected override ConfigModel ReadConfig() => Functions.Config.Instance.ReadConfig();
        public override bool HasChannelConfig => true;
        public override bool CanImportChannel => true;

        private Dictionary<string, DeviceContext> deviceContextDict = new Dictionary<string, DeviceContext>();
        private DeviceContext[] deviceContexts = new DeviceContext[0];

        public Ys7Context Ys7Context { get; private set; }

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
            base.Init();
            AddFunction(new Functions.Config());
            AddFunction(new YiQiDong.Core.Functions.QpChannelView(() => Client), true);
        }

        private void Ys7Context_DeviceOffline(object sender, QuickNV.YS7.Model.DeviceInfo e)
        {
            AgentContext.LogInfo($"[萤石上下文]设备下线。序列号:{e.deviceSerial},名称:{e.deviceName}");

            foreach (var device in deviceContexts)
            {
                if (device.Model.Config.Ys7DeviceSerial == e.deviceSerial)
                {
                    device.IsOnline = false;
                    SendDeviceOfflineNotice(device.Model.Id, "设备下线");
                }
            }
        }

        private void Ys7Context_DeviceOnline(object sender, QuickNV.YS7.Model.DeviceInfo e)
        {
            AgentContext.LogInfo($"[萤石上下文]设备上线。序列号:{e.deviceSerial},名称:{e.deviceName}");
            foreach (var device in deviceContexts)
            {
                if (device.Model.Config.Ys7DeviceSerial == e.deviceSerial)
                {
                    device.IsOnline = true;
                    SendDeviceOnlineNotice(device.Model);
                }
            }
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

        protected override byte[] Snapshot(string deviceId, string channelId, ImageParameter parameter)
        {
            var deviceContext = GetDeviceContext(deviceId);
            var channelInfo = deviceContext.Model.GetChannel(channelId);
            return deviceContext.Snapshot(channelInfo);
        }

        protected override Protocol.QpCommands.CreateChannelLiveStream.Response CreateChannelLiveStream(QpChannel channel, Protocol.QpCommands.CreateChannelLiveStream.Request request)
        {
            var deviceContext = GetDeviceContext(request.MediaInfo.Device.Id);
            if (!deviceContext.IsOnline)
                throw new IOException("设备不在线");
            var liveStream_StreamInfo = deviceContext.CreateLiveStream(request.MediaServerInfo, request.MediaInfo).Result;
            return new Protocol.QpCommands.CreateChannelLiveStream.Response()
            {
                LiveStreamInfo = liveStream_StreamInfo
            };
        }

        protected override Response CreateChannelPlaybackStream(QpChannel channel, Request request)
        {
            throw new NotImplementedException();
        }

        protected override Protocol.QpCommands.DestoryChannelStream.Response DestoryChannelStream(QpChannel qpChannel, Protocol.QpCommands.DestoryChannelStream.Request request)
        {
            var deviceContext = GetDeviceContext(request.DeviceId);
            deviceContext.DestoryChannelStream(request);
            return new Protocol.QpCommands.DestoryChannelStream.Response();
        }

        protected override void OnDriverConnected()
        {
            if (string.IsNullOrEmpty(Config.Ys7AppKey))
                throw new ArgumentNullException(nameof(Config.Ys7AppKey));
            if (string.IsNullOrEmpty(Config.Ys7Secret))
                throw new ArgumentNullException(nameof(Config.Ys7Secret));
            Ys7Context = new Ys7Context(new Ys7ClientOptions()
            {
                ServerUrl = Config.Ys7ServerUrl,
                AppKey = Config.Ys7AppKey,
                Secret = Config.Ys7Secret
            });
            Ys7Context.DeviceOffline += Ys7Context_DeviceOffline;
            Ys7Context.DeviceOnline += Ys7Context_DeviceOnline;
            Ys7Context.Start();

            lock (deviceContextDict)
            {
                foreach (var device in GetDevices())
                {
                    var deviceContext = new DeviceContext(device);
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
            Ys7Context.Stop();
        }

        protected override void OnDeviceAdded(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            var deviceContext = new DeviceContext(device);
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
