using QuickNV.HikvisionNetSDK.Api;
using System.Text;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Protocol.Driver.QpModels;
using Quick.Utils;

namespace QuickNV.Driver.HikvisionDeviceNetwork
{
    public class DeviceContext : IDisposable
    {
        private const string MANUFACTURER = "HIKVISION";
        private CancellationTokenSource cts;
        public HvSession HvSession { get; private set; }
        private Dictionary<int, HvChannel> hvChannelDict = new Dictionary<int, HvChannel>();
        public DriverDevice<DeviceConfig, ChannelConfig> Model { get; private set; }
        public bool IsOnline { get; private set; } = false;

        private void NoticeOnline()
        {
            IsOnline = true;
            Agent.Instance.SendDeviceOnlineNotice(Model);
        }

        private void NoticeOffline(string reason)
        {
            IsOnline = false;
            Agent.Instance.SendDeviceOfflineNotice(Model.Id, reason);
        }

        public DeviceContext(DriverDevice<DeviceConfig, ChannelConfig> model)
        {
            Model = model;
            cts = new CancellationTokenSource();
            beginConnect(cts.Token);
        }

        private void delayToConnect(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            Task.Delay(5000, cancellationToken).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                beginConnect(cancellationToken);
            });
        }

        private void beginConnect(CancellationToken cancellationToken)
        {
            Task.Run(() =>
            {
                try
                {
                    var config = Model.Config;
                    HvSession = HvSession.Login(
                        config.Host,
                        config.Port,
                        config.UserName,
                        config.Password,
                        Encoding.GetEncoding(config.Encoding));
                    HvSession.ChannelService.RefreshChannelsName();
                    hvChannelDict = HvSession.ChannelService.AllChannels.ToDictionary(t => t.Id, t => t);
                    var deviceConfig = HvSession.ConfigService.GetDeviceConfig();
                    Model.Manufacturer = MANUFACTURER;
                    Model.Model = deviceConfig.TypeName;
                    Model.SerialNumber = deviceConfig.Serial;
                    Model.FirmwareVersion = deviceConfig.Version;
                    checkConnection(HvSession, cancellationToken);
                    NoticeOnline();
                    AgentContext.LogInfo($"设备[Id:{Model.Id},Name:{Model.Name}]连接成功");
                }
                catch (Exception ex)
                {
                    var message = ExceptionUtils.GetExceptionMessage(ex);
                    AgentContext.LogTrace($"设备[Id:{Model.Id},Name:{Model.Name}]连接失败，原因：{ExceptionUtils.GetExceptionString(ex)}");
                    NoticeOffline(message);
                    delayToConnect(cancellationToken);
                }
            });
        }

        private void checkConnection(HvSession session, CancellationToken cancellationToken)
        {
            Task.Delay(5000, cancellationToken).ContinueWith(t =>
            {
                if (t.IsCanceled)
                    return;
                try
                {
                    session.ConfigService.GetTime();
                    checkConnection(session, cancellationToken);
                }
                catch
                {
                    NoticeOffline("连接已断开！");
                    AgentContext.LogWarn($"设备[Id:{Model.Id},Name:{Model.Name}]连接已断开");
                    HvSession?.Dispose();
                    delayToConnect(cancellationToken);
                }
            });
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts = null;

            HvSession?.Dispose();
        }

        private HvChannel GetHvChannel(int channelId)
        {
            if (hvChannelDict.TryGetValue(channelId, out var channel))
                return channel;
            return null;
        }

        public async Task<StreamInfo> CreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            var channelInfo = Model.GetChannel(mediaInfo.Channel.Id);
            var channel = GetHvChannel(channelInfo.Config.ChannelId);
            //得到流地址
            var streamUrl = HvSession.ChannelService.GetLiveRtspUrl(channel, channelInfo.Config.StreamType, Model.Config.RtspPathFormat);
            //如果配置了RTSP端口
            if (Model.Config.RtspPort > 0)
            {
                var uriBuilder = new UriBuilder(streamUrl);
                uriBuilder.Port = Model.Config.RtspPort;
                streamUrl = uriBuilder.ToString();
            }

            try
            {
                //媒体服务器添加媒体代理
                return await Agent.Instance.MediaServerAddStreamProxy(mediaInfo.MediaId,
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

        public async Task<StreamInfo> CreatePlaybackStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo, DateTime startTime, DateTime endTime)
        {
            var channelInfo = Model.GetChannel(mediaInfo.Channel.Id);
            var channel = GetHvChannel(channelInfo.Config.ChannelId);
            //得到流地址。录像回放都是主码流
            var streamUrl = HvSession.ChannelService.GetPlaybackRtspUrl(channel, HvStreamType.Main, startTime, endTime);
            //如果配置了RTSP端口
            if (Model.Config.RtspPort > 0)
            {
                var uriBuilder = new UriBuilder(streamUrl);
                uriBuilder.Port = Model.Config.RtspPort;
                streamUrl = uriBuilder.ToString();
            }
            try
            {
                //媒体服务器添加媒体代理
                return await Agent.Instance.MediaServerAddStreamProxy(mediaInfo.MediaId,
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

        private uint GetHvPtzSpeed(float protocolSpeed)
        {
            var MIN_VALUE = 1;
            var MAX_VALUE = 7;

            var speed = (MAX_VALUE - MIN_VALUE) * protocolSpeed + MIN_VALUE;
            return Convert.ToUInt32(speed);
        }

        private PTZCommandType lastCmd = PTZCommandType.Stop;

        private HvPTZCommand GetHvPTZCommand(PTZCommandType commandType)
        {
            switch (commandType)
            {
                case PTZCommandType.Up:
                    return HvPTZCommand.TILT_UP;
                case PTZCommandType.Down:
                    return HvPTZCommand.TILT_DOWN;
                case PTZCommandType.Left:
                    return HvPTZCommand.PAN_LEFT;
                case PTZCommandType.Right:
                    return HvPTZCommand.PAN_RIGHT;
                case PTZCommandType.ZoomIn:
                    return HvPTZCommand.ZOOM_IN;
                case PTZCommandType.ZoomOut:
                    return HvPTZCommand.ZOOM_OUT;
                case PTZCommandType.FocusFar:
                    return HvPTZCommand.FOCUS_FAR;
                case PTZCommandType.FocusNear:
                    return HvPTZCommand.FOCUS_NEAR;
                case PTZCommandType.IrisOpen:
                    return HvPTZCommand.IRIS_OPEN;
                case PTZCommandType.IrisClose:
                    return HvPTZCommand.IRIS_CLOSE;
            }
            return default;
        }

        public void PtzControl(int channelId, PTZCommandType commandType, float moveSpeed)
        {
            var channel = GetHvChannel(channelId);
            if (channel == null)
                return;
            if (commandType != PTZCommandType.Stop)
                lastCmd = commandType;
            switch (commandType)
            {
                case PTZCommandType.Up:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.TILT_UP, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.Down:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.TILT_DOWN, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.Left:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.PAN_LEFT, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.Right:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.PAN_RIGHT, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.ZoomIn:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.ZOOM_IN, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.ZoomOut:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.ZOOM_OUT, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.FocusFar:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.FOCUS_FAR, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.FocusNear:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.FOCUS_NEAR, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.IrisOpen:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.IRIS_OPEN, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.IrisClose:
                    HvSession.ChannelService.PTZControl(channel.Id, HvPTZCommand.IRIS_CLOSE, false, GetHvPtzSpeed(moveSpeed));
                    break;
                case PTZCommandType.Stop:
                    HvSession.ChannelService.PTZControl(channel.Id, GetHvPTZCommand(lastCmd), true, 0);
                    break;
            }
        }

        public byte[] Snapshot(DriverChannel<ChannelConfig> channelInfo)
        {
            return HvSession.PictureService.CaptureJPEGPicture_NEW(channelInfo.Config.ChannelId);
        }

        public VideoFileInfo[] FindPlaybackFiles(DriverChannel<ChannelConfig> channelInfo, DateTime startTime, DateTime endTime)
        {
            var files = HvSession.VideoFileService.FindFile(channelInfo.Config.ChannelId, startTime, endTime);
            return files.Select(t => new VideoFileInfo()
            {
                Id = t.Name,
                Name = t.Name,
                Size = t.Size,
                StartTime = t.StartTime,
                EndTime = t.StopTime
            }).ToArray();
        }
    }
}