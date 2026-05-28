using System.Text.Json;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Driver.Protocol.QpModels;
using Quick.Utils;

namespace QuickNV.Driver.Onvif
{
    public class DeviceContext : IDisposable
    {
        private CancellationTokenSource cts;
        private QuickNV.Onvif.OnvifClient client;
        private QuickNV.Onvif.Media.MediaClient mediaClient;
        private QuickNV.Onvif.Imaging.ImagingPortClient imagingPortClient;
        private QuickNV.Onvif.RecordingSearch.SearchPortClient searchPortClient;
        private QuickNV.Onvif.ReplayControl.ReplayPortClient replayPortClient;
        private QuickNV.Onvif.PTZ.PTZClient ptzClient;
        private bool isPtzContinuousMoving = false;
        private bool isPtzFocusMoving = false;

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
            client = new QuickNV.Onvif.OnvifClient(new QuickNV.Onvif.OnvifClientOptions()
            {
                Host = Model.Config.Host,
                Port = Model.Config.Port,
                ClientCredentialType = Model.Config.ClientCredentialType,
                UserName = Model.Config.UserName,
                Password = Model.Config.Password,
                Scheme = Model.Config.Scheme,
                RtspPort = Model.Config.RtspPort,
                SnapshotPort = Model.Config.SnapshotPort
            });
            beginCheckIsOnline(cts.Token);
        }

        private void beginCheckIsOnline(CancellationToken cancellationToken)
        {
            if (cancellationToken.IsCancellationRequested)
                return;
            Task.Run(async () =>
            {
                try
                {
                    QuickNV.Onvif.Device.GetDeviceInformationResponse deviceInformation = null;
                    if (IsOnline)
                    {
                        deviceInformation = await client.DeviceClient.GetDeviceInformationAsync(new QuickNV.Onvif.Device.GetDeviceInformationRequest());
                        NoticeOnline();
                    }
                    else
                    {
                        await client.ConnectAsync();
                        mediaClient = new QuickNV.Onvif.Media.MediaClient(client);
                        imagingPortClient = new QuickNV.Onvif.Imaging.ImagingPortClient(client);
                        if (client.Capabilities.PTZ != null)
                            ptzClient = new QuickNV.Onvif.PTZ.PTZClient(client);
                        if (client.Capabilities.Extension.Search != null)
                            searchPortClient = new QuickNV.Onvif.RecordingSearch.SearchPortClient(client);
                        if (client.Capabilities.Extension.Replay != null)
                            replayPortClient = new QuickNV.Onvif.ReplayControl.ReplayPortClient(client);
                        deviceInformation = client.DeviceInformation;
                    }
                    Model.Manufacturer = deviceInformation.Manufacturer;
                    Model.Model = deviceInformation.Model;
                    Model.SerialNumber = deviceInformation.SerialNumber;
                    Model.FirmwareVersion = deviceInformation.FirmwareVersion;
                    NoticeOnline();
                }
                catch (Exception ex)
                {
                    NoticeOffline(ExceptionUtils.GetExceptionMessage(ex));
                }
                try
                {
                    await Task.Delay(60 * 1000, cancellationToken);
                }
                catch
                {
                    return;
                }
                beginCheckIsOnline(cancellationToken);
            });
        }

        public void Dispose()
        {
            cts?.Cancel();
            cts = null;
        }


        public async Task<QuickNV.Onvif.Media.Profile[]> GetMediaProfilesAsync()
        {
            var rep = await mediaClient.GetProfilesAsync();
            return rep.Profiles;
        }

        public async Task<StreamInfo> CreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            //得到通道配置
            var channelConfig = JsonSerializer.Deserialize<ChannelConfig>(mediaInfo.Channel.DriverConfig);
            //得到流地址
            var streamUrl = await mediaClient.QuickOnvif_GetStreamUriAsync(channelConfig.ProfileToken, true);
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
            catch
            {
                throw;
            }
        }

        public async Task<QuickNV.Onvif.Media.OSDConfiguration[]> GetOSDs(string configurationToken)
        {
            var rep = await mediaClient.GetOSDsAsync(configurationToken);
            return rep.OSDs;
        }

        public async Task PtzContinuousMove(string profileToken, float xSpeed, float ySpeed, float zoomSpeed)
        {
            if (string.IsNullOrEmpty(profileToken))
                return;
            var isMoving = xSpeed != 0 || ySpeed != 0 || zoomSpeed != 0;
            //如果移动状态没有改变
            if (isMoving == isPtzContinuousMoving)
                return;
            isPtzContinuousMoving = isMoving;
            if (ptzClient == null)
                throw new ApplicationException("设备不支持云台控制");
            await ptzClient.ContinuousMoveAsync(profileToken, new QuickNV.Onvif.PTZ.PTZSpeed()
            {
                PanTilt = new QuickNV.Onvif.PTZ.Vector2D()
                {
                    x = xSpeed,
                    y = ySpeed
                },
                Zoom = new QuickNV.Onvif.PTZ.Vector1D()
                {
                    x = zoomSpeed
                }
            }, null);
        }

        public async Task PtzFocusMove(string videoSourceToken, float focusSpeed)
        {
            if (string.IsNullOrEmpty(videoSourceToken))
                return;
            var isMoving = focusSpeed != 0;
            //如果移动状态没有改变
            if (isMoving == isPtzFocusMoving)
                return;
            isPtzFocusMoving = isMoving;
            await imagingPortClient.MoveAsync(videoSourceToken, new QuickNV.Onvif.Imaging.FocusMove()
            {
                Continuous = new QuickNV.Onvif.Imaging.ContinuousFocus()
                {
                    Speed = focusSpeed
                }
            });
        }

        public byte[] Snapshot(DriverChannel<ChannelConfig> channelInfo)
        {
            if (mediaClient == null)
                throw new IOException("设备不在线");
            return mediaClient?.QuickOnvif_SnapshotAsync(channelInfo.Config.ProfileToken).Result;
        }

        public VideoFileInfo[] FindPlaybackFiles(DriverChannel<ChannelConfig> channelInfo, DateTime startTime, DateTime endTime)
        {
            var searchScope = new QuickNV.Onvif.RecordingSearch.SearchScope();
            var eventFilter = new QuickNV.Onvif.RecordingSearch.EventFilter();
            var searchToken = searchPortClient.FindEventsAsync(
                startTime.ToUniversalTime(),
                endTime.ToUniversalTime(),
                searchScope,
                eventFilter,
                true,
                100, "P5S").Result.SearchToken;
            var eventSearchResults = searchPortClient.GetEventSearchResultsAsync(searchToken, 0, 100, "P5S").Result;
            searchPortClient.EndSearchAsync(searchToken).Wait();
            return eventSearchResults.ResultList.Result.Select(t => new VideoFileInfo()
            {
                Id = t.RecordingToken,
                Name = t.TrackToken,
                Size = 0,
                StartTime = t.Time,
                EndTime = t.Time
            }).ToArray();
        }

        public async Task<StreamInfo> CreatePlaybackStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo, DateTime startTime, DateTime endTime)
        {
            var channelInfo = Model.GetChannel(mediaInfo.Channel.Id);

            //得到流地址
            var streamUrl = (await replayPortClient.GetReplayUriAsync(new QuickNV.Onvif.ReplayControl.StreamSetup()
            {
                Stream = QuickNV.Onvif.ReplayControl.StreamType.RTPUnicast,
                Transport = new QuickNV.Onvif.ReplayControl.Transport()
                {
                    Protocol = QuickNV.Onvif.ReplayControl.TransportProtocol.RTSP
                }
            }, null)).Uri;

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
    }
}