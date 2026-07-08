using Quick.Protocol;
using System.Text;
using System.Text.Json.Serialization;
using YiQiDong.Agent;
using QuickNV.Driver.Agent;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Driver.Onvif
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

        protected override Protocol.Driver.QpCommands.GetDeviceConfig.Response GetDeviceConfig(QpChannel channel, Protocol.Driver.QpCommands.GetDeviceConfig.Request request)
        {
            return Functions.GetDeviceConfigFunction.Invoke(channel, request);
        }

        protected override Protocol.Driver.QpCommands.ImportDevices.Response ImportDevices(
            QpChannel channel,
            Protocol.Driver.QpCommands.ImportDevices.Request request)
            => Functions.ImportDevicesFunction.Invoke(channel, request);

        protected override Protocol.Driver.QpCommands.ImportChannels.Response ImportChannels(
            QpChannel channel,
            Protocol.Driver.QpCommands.ImportChannels.Request request)
            => Functions.ImportChannelsFunction.Invoke(channel, request);

        protected override Protocol.Driver.QpCommands.GetChannelConfig.Response GetChannelConfig(QpChannel channel, Protocol.Driver.QpCommands.GetChannelConfig.Request request)
            => Functions.GetChannelConfigFunction.Invoke(channel, request);

        protected override Protocol.Driver.QpCommands.CreateChannelLiveStream.Response CreateChannelLiveStream(QpChannel channel, Protocol.Driver.QpCommands.CreateChannelLiveStream.Request request)
        {
            var deviceContext = GetDeviceContext(request.MediaInfo.Device.Id);
            var streamInfo = deviceContext.CreateLiveStream(request.MediaServerInfo, request.MediaInfo).Result;
            return new Protocol.Driver.QpCommands.CreateChannelLiveStream.Response()
            {
                LiveStreamInfo = streamInfo
            };
        }

        protected override Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Response CreateChannelPlaybackStream(QpChannel channel, Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Request request)
        {
            var deviceContext = GetDeviceContext(request.MediaInfo.Device.Id);
            var streamInfo = deviceContext.CreatePlaybackStream(request.MediaServerInfo, request.MediaInfo, request.StartTime, request.EndTime).Result;
            return new Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Response()
            {
                PlaybackStreamInfo = streamInfo
            };
        }

        protected override Protocol.Driver.QpCommands.DestoryChannelStream.Response DestoryChannelStream(QpChannel channel, Protocol.Driver.QpCommands.DestoryChannelStream.Request request)
        {
            return new Protocol.Driver.QpCommands.DestoryChannelStream.Response();
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

        protected override Protocol.Driver.QpCommands.PtzControl.Response PtzControl(QpChannel channel, Protocol.Driver.QpCommands.PtzControl.Request request)
        {
            var deviceContext = GetDeviceContext(request.DeviceId);
            if (deviceContext != null)
            {
                var channelConfig = deviceContext.Model.GetChannel(request.ChannelId)?.Config;
                if (channelConfig != null)
                {
                    switch (request.CommandType)
                    {
                        case PTZCommandType.Up:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, 0, request.MoveSpeed, 0);
                            break;
                        case PTZCommandType.Left:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, 0 - request.MoveSpeed, 0, 0);
                            break;
                        case PTZCommandType.Down:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, 0, 0 - request.MoveSpeed, 0);
                            break;
                        case PTZCommandType.Right:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, request.MoveSpeed, 0, 0);
                            break;
                        case PTZCommandType.ZoomIn:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, 0, 0, request.MoveSpeed);
                            break;
                        case PTZCommandType.ZoomOut:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, 0, 0, 0 - request.MoveSpeed);
                            break;
                        case PTZCommandType.IrisOpen:
                            _ = deviceContext.PtzFocusMove(channelConfig.VideoSourceToken, request.MoveSpeed);
                            break;
                        case PTZCommandType.IrisClose:
                            _ = deviceContext.PtzFocusMove(channelConfig.VideoSourceToken, 0 - request.MoveSpeed);
                            break;
                        case PTZCommandType.Stop:
                            _ = deviceContext.PtzContinuousMove(channelConfig.ProfileToken, 0, 0, 0);
                            _ = deviceContext.PtzFocusMove(channelConfig.VideoSourceToken, 0);
                            break;
                    }
                }
            }
            return new Protocol.Driver.QpCommands.PtzControl.Response();
        }

        protected override byte[] Snapshot(string deviceId, string channelId, ImageParameter parameter)
        {
            var deviceContext = GetDeviceContext(deviceId);
            var channelInfo = deviceContext.Model.GetChannel(channelId);
            return deviceContext.Snapshot(channelInfo);
        }

        protected override VideoFileInfo[] FindPlaybackFiles(string deviceId, string channelId, DateTime startTime, DateTime endTime)
        {
            var deviceContext = GetDeviceContext(deviceId);
            var channelInfo = deviceContext.Model.GetChannel(channelId);
            return deviceContext.FindPlaybackFiles(channelInfo, startTime, endTime);
        }
    }
}
