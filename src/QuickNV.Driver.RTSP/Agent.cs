using Quick.Protocol;
using System.Text;
using System.Text.Json.Serialization;
using YiQiDong.Agent;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.RTSP
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
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            base.Init();
            AddFunction(new Functions.Config());
            AddFunction(new YiQiDong.Core.Functions.QpChannelView(() => Client));
        }

        public override void Stop()
        {
            base.Stop();
        }

        protected override Protocol.QpCommands.GetDeviceConfig.Response GetDeviceConfig(QpChannel channel, Protocol.QpCommands.GetDeviceConfig.Request request)
        {
            return Functions.GetDeviceConfigFunction.Invoke(channel, request);
        }

        protected override Protocol.QpCommands.ImportDevices.Response ImportDevices(
            QpChannel channel,
            Protocol.QpCommands.ImportDevices.Request request)
            => Functions.ImportDevicesFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.ImportChannels.Response ImportChannels(
            QpChannel channel,
            Protocol.QpCommands.ImportChannels.Request request)
            => Functions.ImportChannelsFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.GetChannelConfig.Response GetChannelConfig(QpChannel channel, Protocol.QpCommands.GetChannelConfig.Request request)
            => Functions.GetChannelConfigFunction.Invoke(channel, request);

        protected override Protocol.QpCommands.CreateChannelLiveStream.Response CreateChannelLiveStream(QpChannel channel, Protocol.QpCommands.CreateChannelLiveStream.Request request)
        {
            var deviceContext = GetDeviceContext(request.MediaInfo.Device.Id);
            var streamInfo = deviceContext.CreateLiveStream(request.MediaServerInfo, request.MediaInfo).Result;
            return new Protocol.QpCommands.CreateChannelLiveStream.Response()
            {
                LiveStreamInfo = streamInfo
            };
        }

        protected override Protocol.QpCommands.DestoryChannelStream.Response DestoryChannelStream(QpChannel channel, Protocol.QpCommands.DestoryChannelStream.Request request)
        {
            return new Protocol.QpCommands.DestoryChannelStream.Response();
        }

        protected override void OnDriverConnected()
        {
            lock (deviceContextDict)
            {
                foreach (var device in GetDevices())
                {
                    var deviceContext = new DeviceContext(device);
                    deviceContextDict[device.Id] = deviceContext;
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
            lock (deviceContextDict)
            {
                deviceContextDict[device.Id] = deviceContext;
                deviceContexts = deviceContextDict.Values.ToArray();
            }
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

        protected override Protocol.QpCommands.CreateChannelPlaybackStream.Response CreateChannelPlaybackStream(QpChannel channel, Protocol.QpCommands.CreateChannelPlaybackStream.Request request)
        {
            throw new NotImplementedException();
        }

        protected override Protocol.QpCommands.PtzControl.Response PtzControl(QpChannel channel, Protocol.QpCommands.PtzControl.Request request)
        {
            throw new NotImplementedException();
        }
    }
}
