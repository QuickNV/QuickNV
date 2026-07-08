using System.Text.Json;
using YiQiDong.Core.Utils;
using QuickNV.Driver.Agent;
using QuickNV.Protocol.Driver.QpModels;
using Quick.Utils;

namespace QuickNV.Driver.RTSP
{
    public class DeviceContext : IDisposable
    {
        private CancellationTokenSource cts;

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
                    //QuickNV.Onvif.Device.GetDeviceInformationResponse deviceInformation = null;
                    if (IsOnline)
                    {
                        //deviceInformation = await client.DeviceClient.GetDeviceInformationAsync(new QuickNV.Onvif.Device.GetDeviceInformationRequest());
                        NoticeOnline();
                    }
                    else
                    {
                        //await client.ConnectAsync();
                        //mediaClient = new QuickNV.Onvif.Media.MediaClient(client);
                        //imagingPortClient = new QuickNV.Onvif.Imaging.ImagingPortClient(client);
                        //if (client.Capabilities.PTZ != null)
                        //    ptzClient = new QuickNV.Onvif.PTZ.PTZClient(client);
                        //if (client.Capabilities.Extension.Search != null)
                        //    searchPortClient = new QuickNV.Onvif.RecordingSearch.SearchPortClient(client);
                        //if (client.Capabilities.Extension.Replay != null)
                        //    replayPortClient = new QuickNV.Onvif.ReplayControl.ReplayPortClient(client);
                        //deviceInformation = client.DeviceInformation;
                    }
                    //Model.Manufacturer = deviceInformation.Manufacturer;
                    //Model.Model = deviceInformation.Model;
                    //Model.SerialNumber = deviceInformation.SerialNumber;
                    //Model.FirmwareVersion = deviceInformation.FirmwareVersion;
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


        public async Task<StreamInfo> CreateLiveStream(MediaServerInfo mediaServerInfo, MediaInfo mediaInfo)
        {
            //得到通道配置
            var channelConfig = JsonSerializer.Deserialize<ChannelConfig>(mediaInfo.Channel.DriverConfig);
            //得到流地址
            var streamUrl = channelConfig.RtspUrl;
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
    }
}