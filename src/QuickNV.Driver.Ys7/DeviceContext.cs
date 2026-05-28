using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpCommands.DestoryChannelStream;
using QuickNV.Driver.Protocol.QpModels;
using QuickNV.YS7.Model;

namespace QuickNV.Driver.Ys7
{
    public class DeviceContext : IDisposable
    {
        private Dictionary<int, string> previewMediaIdYs7LiveIdDict = new Dictionary<int, string>();
        public DriverDevice<DeviceConfig, ChannelConfig> Model { get; private set; }
        public bool IsOnline { get; set; }

        public DeviceContext(DriverDevice<DeviceConfig, ChannelConfig> device)
        {
            Model = device;
            var ys7Device = Agent.Instance.Ys7Context.GetDevice(device.Config.Ys7DeviceSerial);
            IsOnline = ys7Device != null && ys7Device.status == DeviceStatus.Online;
        }

        public async Task<StreamInfo> CreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            var channelInfo = Model.GetChannel(mediaInfo.Channel.Id);
            //得到流地址
            var getLiveAddressRet = await Agent.Instance.Ys7Context.Ys7Client.GetLiveAddress(
                deviceSerial: Model.Config.Ys7DeviceSerial,
                channelNo: channelInfo.Config.ChannelId,
                protocol: channelInfo.Config.StreamProtocol,
                expireTime: 24 * 60 * 60,
                quality: channelInfo.Config.StreamType
                );

            if (!getLiveAddressRet.IsSuccess)
                throw new IOException($"萤石开放平台返回错误。错误码：[{getLiveAddressRet.code}]，消息：[{getLiveAddressRet.msg}]");

            lock (previewMediaIdYs7LiveIdDict)
                previewMediaIdYs7LiveIdDict[mediaInfo.MediaId] = getLiveAddressRet.data.id;

            var streamUrl = getLiveAddressRet.data.url;
            try
            {
                //媒体服务器添加媒体代理
                return await Agent.Instance.MediaServerAddStreamProxy(
                    mediaInfo.MediaId,
                    new StreamInfo()
                    {
                        MediaServerId = mediaServerInfo.Id,
                        App = "rtp",
                        Stream = mediaInfo.StreamId
                    }, streamUrl);
            }
            catch (TimeoutException)
            {
                throw new TimeoutException("添加媒体代理超时");
            }
        }

        public void Dispose()
        {

        }

        private PTZCommand lastCommand = PTZCommand.Right;
        public void PtzControl(int channelId, PTZCommandType commandType, float moveSpeed)
        {
            var start = true;
            switch (commandType)
            {
                case PTZCommandType.Stop:
                    start = false;
                    break;
                case PTZCommandType.Up:
                    lastCommand = PTZCommand.Up;
                    break;
                case PTZCommandType.Down:
                    lastCommand = PTZCommand.Down;
                    break;
                case PTZCommandType.Left:
                    lastCommand = PTZCommand.Left;
                    break;
                case PTZCommandType.Right:
                    lastCommand = PTZCommand.Right;
                    break;
                case PTZCommandType.ZoomIn:
                    lastCommand = PTZCommand.ZoomIn;
                    break;
                case PTZCommandType.ZoomOut:
                    lastCommand = PTZCommand.ZoomOut;
                    break;
                case PTZCommandType.FocusFar:
                    lastCommand = PTZCommand.FocusFar;
                    break;
                case PTZCommandType.FocusNear:
                    lastCommand = PTZCommand.FocusNear;
                    break;
                case PTZCommandType.IrisOpen:
                case PTZCommandType.IrisClose:
                    return;
            }
            if (start)
            {
                var speed = Convert.ToInt32(moveSpeed * 2);
                _ = Agent.Instance.Ys7Context.Ys7Client.StartPtz(Model.Config.Ys7DeviceSerial, channelId, lastCommand, speed);
            }
            else
            {
                _ = Agent.Instance.Ys7Context.Ys7Client.StopPtz(Model.Config.Ys7DeviceSerial, channelId, lastCommand);
            }
        }

        public byte[] Snapshot(DriverChannel<ChannelConfig> channelInfo)
        {
            var ret = Agent.Instance.Ys7Context.Ys7Client.Capture(Model.Config.Ys7DeviceSerial, channelInfo.Config.ChannelId).Result;
            if (!ret.IsSuccess)
                throw new IOException(ret.msg);
            var uri = ret.data.picUrl;
            using (HttpClient httpClient = new HttpClient())
                return httpClient.GetByteArrayAsync(uri).Result;
        }

        public void DestoryChannelStream(Request request)
        {
            var channel = Model.GetChannel(request.ChannelId);
            string liveId;
            lock (previewMediaIdYs7LiveIdDict)
            {
                if (!previewMediaIdYs7LiveIdDict.TryGetValue(request.MediaId, out liveId))
                    return;
                previewMediaIdYs7LiveIdDict.Remove(request.MediaId);
            }
            //失效播放地址
            _ = Agent.Instance.Ys7Context.Ys7Client.DisableLiveAddress(Model.Config.Ys7DeviceSerial, channel.Config.ChannelId, liveId);
        }
    }
}
