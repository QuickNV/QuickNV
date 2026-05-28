using QuickNV.DahuaNetSDK;
using QuickNV.DahuaNetSDK.Api;
using System.Text;
using YiQiDong.Agent;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;
using Quick.Utils;

namespace QuickNV.Driver.DahuaDeviceNetwork
{
    public class DeviceContext : IDisposable
    {
        private const string MANUFACTURER = "DAHUA";
        private CancellationTokenSource cts;
        public DhSession DhSession { get; private set; }
        private Dictionary<int, DhChannel> hvChannelDict = new Dictionary<int, DhChannel>();
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
                    DhSession = DhSession.Login(
                        config.Host,
                        config.Port,
                        config.UserName,
                        config.Password,
                        config.LoginType);
                    DhSession.ChannelService.RefreshChannelsName();
                    hvChannelDict = DhSession.ChannelService.AllChannels.ToDictionary(t => t.Id, t => t);

                    Model.Manufacturer = MANUFACTURER;
                    Model.Model = DhSession.ConfigService.GetDeviceType();
                    Model.SerialNumber = DhSession.ConfigService.GetDeviceSerialNumber();
                    Model.FirmwareVersion = DhSession.ConfigService.GetSoftwareVersion();
                    checkConnection(DhSession, cancellationToken);
                    NoticeOnline();
                    AgentContext.LogInfo($"设备[Id:{Model.Id},Name:{Model.Name}]连接成功");
                }
                catch (Exception ex)
                {
                    var message = ExceptionUtils.GetExceptionMessage(ex);
                    NoticeOffline(message);
                    delayToConnect(cancellationToken);
                }
            });
        }

        private void checkConnection(DhSession session, CancellationToken cancellationToken)
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
                    DhSession?.Dispose();
                    delayToConnect(cancellationToken);
                }
            });
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts = null;

            DhSession?.Dispose();
        }

        private DhChannel GetDhChannel(int channelId)
        {
            if (hvChannelDict.TryGetValue(channelId, out var channel))
                return channel;
            return null;
        }

        public async Task<StreamInfo> CreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            var channelInfo = Model.GetChannel(mediaInfo.Channel.Id);
            var channel = GetDhChannel(channelInfo.Config.ChannelId);
            //得到流地址
            var streamUrl = DhSession.ChannelService.GetRtspUrl(channel, channelInfo.Config.StreamType);
            //如果配置了RTSP端口
            if (Model.Config.RtspPort > 0)
            {
                var uriBuilder = new UriBuilder(streamUrl);
                uriBuilder.Port = Model.Config.RtspPort;
                streamUrl = uriBuilder.ToString();
            }

            StreamInfo liveStream_StreamInfo = null;
            try
            {
                //媒体服务器添加媒体代理
                liveStream_StreamInfo = await Agent.Instance.MediaServerAddStreamProxy(
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
            catch
            {
                throw;
            }
            return liveStream_StreamInfo;
        }

        private int GetHvPtzSpeed(float protocolSpeed)
        {
            var MIN_VALUE = 1;
            var MAX_VALUE = 7;

            var speed = (MAX_VALUE - MIN_VALUE) * protocolSpeed + MIN_VALUE;
            return Convert.ToInt32(speed);
        }

        private PTZCommandType lastCmd = PTZCommandType.Stop;


        private EM_EXTPTZ_ControlType GetHvPTZCommand(PTZCommandType commandType)
        {
            switch (commandType)
            {
                case PTZCommandType.Up:
                    return EM_EXTPTZ_ControlType.UP_CONTROL;
                case PTZCommandType.Down:
                    return EM_EXTPTZ_ControlType.DOWN_CONTROL;
                case PTZCommandType.Left:
                    return EM_EXTPTZ_ControlType.LEFT_CONTROL;
                case PTZCommandType.Right:
                    return EM_EXTPTZ_ControlType.RIGHT_CONTROL;
                case PTZCommandType.ZoomIn:
                    return EM_EXTPTZ_ControlType.ZOOM_ADD_CONTROL;
                case PTZCommandType.ZoomOut:
                    return EM_EXTPTZ_ControlType.ZOOM_DEC_CONTROL;
                case PTZCommandType.FocusFar:
                    return EM_EXTPTZ_ControlType.FOCUS_ADD_CONTROL;
                case PTZCommandType.FocusNear:
                    return EM_EXTPTZ_ControlType.FOCUS_DEC_CONTROL;
                case PTZCommandType.IrisOpen:
                    return EM_EXTPTZ_ControlType.APERTURE_ADD_CONTROL;
                case PTZCommandType.IrisClose:
                    return EM_EXTPTZ_ControlType.APERTURE_DEC_CONTROL;
            }
            return default;
        }

        public void PtzControl(int channelId, PTZCommandType commandType, float moveSpeed)
        {
            var channel = GetDhChannel(channelId);
            if (channel == null)
                return;
            if (commandType != PTZCommandType.Stop)
                lastCmd = commandType;
            switch (commandType)
            {
                case PTZCommandType.Stop:
                    DhSession.ChannelService.PTZControl(channel.Id, GetHvPTZCommand(lastCmd), true, 0);
                    break;
                default:
                    DhSession.ChannelService.PTZControl(channel.Id, GetHvPTZCommand(commandType), false, GetHvPtzSpeed(moveSpeed));
                    break;
            }
        }

        public byte[] Snapshot(DriverChannel<ChannelConfig> channelInfo)
        {
            return DhSession.PictureService.ManualSnap(channelInfo.Config.ChannelId);
        }
    }
}