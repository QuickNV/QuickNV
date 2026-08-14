using Quick.Protocol;
using QuickNV.Interfaces.Core;
using YiQiDong.Agent;
using QuickNV.Core;
using Quick.EntityFrameworkCore.Plus;
using QuickNV.Protocol.Driver.QpModels;
using QuickNV.Model;
using QuickNV.Protocol.Driver.QpNotices;
using Quick.Utils;

namespace QuickNV.Interfaces.Driver
{
    public class Manager
    {
        public static Manager Instance { get; } = new Manager();

        public event EventHandler<Device> DeviceOnline;
        public event EventHandler<Device> DeviceOffline;

        private CommandExecuterManager commandExecuterManager;
        private NoticeHandlerManager noticeHandlerManager;

        private Manager()
        {
            commandExecuterManager = new CommandExecuterManager();
            commandExecuterManager.Register(new QuickNV.Protocol.Driver.QpCommands.Register.Request(), ExecuteRegister);
            commandExecuterManager.Register(new QuickNV.Protocol.Driver.QpCommands.ChangeLiveStreamSSRC.Request(), ChangeLiveStreamSSRC);
            commandExecuterManager.Register(new QuickNV.Protocol.Driver.QpCommands.GetMediaServerStreamInfo.Request(), GetMediaServerStreamInfo);
            commandExecuterManager.Register(new QuickNV.Protocol.Driver.QpCommands.MediaServerAddStreamProxy.Request(), MediaServerAddStreamProxy);

            noticeHandlerManager = new NoticeHandlerManager();
            noticeHandlerManager.Register<DeviceOnlineNotice>(OnDeviceOnlineNotice);
            noticeHandlerManager.Register<DeviceOfflineNotice>(OnDeviceOfflineNotice);
            noticeHandlerManager.Register<DeviceLogNotice>(OnDeviceLogNotice);
        }

        private bool isDeviceInfoChanged(string a, string b)
        {
            if (b == null)
                return false;
            return b != a;
        }

        public void NoticeDeviceOnline(Model.Device device, DeviceInfo deviceInfo)
        {
            if (device == null)
                return;
            if (!device.IsOnline)
            {
                device.IsOnline = true;
                device.PushLog("设备上线");
                AgentContext.LogTrace($"[驱动接口]{device}已上线");
                DeviceOnline?.Invoke(this, device);
            }
            //检查设备硬件信息是否有更新
            bool isChanged = false;
            if (isDeviceInfoChanged(device.Manufacturer, deviceInfo.Manufacturer))
            {
                device.Manufacturer = deviceInfo.Manufacturer;
                isChanged = true;
            }
            if (isDeviceInfoChanged(device.Model, deviceInfo.Model))
            {
                device.Model = deviceInfo.Model;
                isChanged = true;
            }
            if (isDeviceInfoChanged(device.SerialNumber, deviceInfo.SerialNumber))
            {
                device.SerialNumber = deviceInfo.SerialNumber;
                isChanged = true;
            }
            if (isDeviceInfoChanged(device.FirmwareVersion, deviceInfo.FirmwareVersion))
            {
                device.FirmwareVersion = deviceInfo.FirmwareVersion;
                isChanged = true;
            }
            if (isChanged)
            {
                ConfigDbContext.CacheContext.Update(device);
                device.PushLog($"设备硬件信息已更新。厂商：{device.Manufacturer}，型号：{device.Model}，序列号：{device.SerialNumber}，固件版本：{device.FirmwareVersion}");
            }
        }

        public void NoticeDeviceOffline(Model.Device device, string reason)
        {
            if (device == null)
                return;
            if (device.IsOnline)
            {
                device.IsOnline = false;
                var reasonString = string.Empty;
                if (!string.IsNullOrEmpty(reason))
                {
                    reasonString = $"，原因：{reason}";
                }
                AgentContext.LogTrace($"[驱动接口]{device}已离线{reasonString}");
                device.PushLog($"设备离线{reasonString}");
                DeviceOffline?.Invoke(this, device);
            }
        }

        private void OnDeviceOnlineNotice(QpChannel channel, DeviceOnlineNotice package)
        {
            var device = ConfigDbContext.CacheContext.Find(new Device(package.Device.Id));
            if (device == null)
                return;
            NoticeDeviceOnline(device, package.Device);
        }

        private void OnDeviceOfflineNotice(QpChannel channel, DeviceOfflineNotice package)
        {
            var device = ConfigDbContext.CacheContext.Find(new Device(package.DeviceId));
            if (device == null)
                return;
            NoticeDeviceOffline(device, package.Reason);
        }

        private void OnDeviceLogNotice(QpChannel channel, DeviceLogNotice package)
        {
            var device = ConfigDbContext.CacheContext.Find(new Device(package.DeviceId));
            device?.LogContext.PushLog(package.Message);
        }


        private QuickNV.Protocol.Driver.QpCommands.ChangeLiveStreamSSRC.Response ChangeLiveStreamSSRC(QpChannel channel, QuickNV.Protocol.Driver.QpCommands.ChangeLiveStreamSSRC.Request request)
        {
            var mediaServer = MediaServerManager.Instance.GetMediaServer(request.MediaServerId);
            if (mediaServer == null)
                throw new ApplicationException($"未找到编号为[{request.MediaServerId}]的媒体服务器");
            var mediaInfo = mediaServer.GetMediaInfo(request.MediaId);
            if (mediaInfo == null)
                throw new ApplicationException($"媒体服务器[{mediaServer.Model.Name}]中未找到编号为[{request.MediaId}]的媒体信息");
            mediaServer.DestoryMediaId(request.MediaId);
            mediaInfo = mediaServer.GenerateMediaInfo(mediaInfo.Device, mediaInfo.Channel, request.SSRC);
            return new QuickNV.Protocol.Driver.QpCommands.ChangeLiveStreamSSRC.Response()
            {
                MediaInfo = mediaInfo
            };
        }

        private QuickNV.Protocol.Driver.QpCommands.GetMediaServerStreamInfo.Response GetMediaServerStreamInfo(QpChannel channel, QuickNV.Protocol.Driver.QpCommands.GetMediaServerStreamInfo.Request request)
        {
            var mediaServer = MediaServerManager.Instance.GetMediaServer(request.MediaServerId);
            if (mediaServer == null)
                throw new ApplicationException($"未找到编号为[{request.MediaServerId}]的媒体服务器");

            var task = mediaServer.GetStreamRegisteredTask(request.MediaId);
            if (!task.Wait(15 * 1000))
                throw new TimeoutException("等待流注册超时");
            var liveStreamInfo = task.Result;
            return new QuickNV.Protocol.Driver.QpCommands.GetMediaServerStreamInfo.Response()
            {
                StreamInfo = new StreamInfo()
                {
                    MediaServerId = mediaServer.Model.Id,
                    App = liveStreamInfo.App,
                    Stream = liveStreamInfo.Stream
                }
            };
        }

        private QuickNV.Protocol.Driver.QpCommands.MediaServerAddStreamProxy.Response MediaServerAddStreamProxy(QpChannel channel, QuickNV.Protocol.Driver.QpCommands.MediaServerAddStreamProxy.Request request)
        {
            var mediaServer = MediaServerManager.Instance.GetMediaServer(request.StreamInfo.MediaServerId);
            if (mediaServer == null)
                throw new ApplicationException($"未找到编号为[{request.StreamInfo.MediaServerId}]的媒体服务器");

            var task = mediaServer.GetStreamRegisteredTask(request.MediaId);
            var streamProxyKey = mediaServer.AddStreamProxy(request.StreamInfo, request.StreamUrl).Result;
            if (!task.Wait(15 * 1000))
                throw new TimeoutException("等待流注册超时");
            var liveStreamInfo = task.Result;
            return new QuickNV.Protocol.Driver.QpCommands.MediaServerAddStreamProxy.Response()
            {
                StreamInfo = new StreamInfo()
                {
                    MediaServerId = request.StreamInfo.MediaServerId,
                    App = liveStreamInfo.App,
                    Stream = liveStreamInfo.Stream,
                    StreamProxyKey = streamProxyKey
                }
            };
        }

        internal QuickNV.Protocol.Driver.QpCommands.Register.Response ExecuteRegister(QpChannel channel, QuickNV.Protocol.Driver.QpCommands.Register.Request request)
        {
            var driverInfo = request.CurrentDriver;
            EventHandler channel_Disconnected_Hanlder = null;
            channel_Disconnected_Hanlder = (sender, e) =>
             {
                 channel.Disconnected -= channel_Disconnected_Hanlder;
                 DriverManager.Instance.UnregisterDriver(channel, driverInfo);
                 AgentContext.LogInfo($"[驱动接口][{channel.ChannelName}]驱动[{driverInfo.Name}_{driverInfo.Version}]已经取消注册。原因：{ExceptionUtils.GetExceptionMessage(channel.LastException)}");
             };
            channel.Disconnected += channel_Disconnected_Hanlder;
            DriverManager.Instance.RegisterDriver(channel, driverInfo);
            channel.RegisterCommandExecuterManagers([commandExecuterManager]);
            channel.RegisterNoticeHandlerManagers([noticeHandlerManager]);
            AgentContext.LogInfo($"[驱动接口][{channel.ChannelName}]驱动[{driverInfo.Name}_{driverInfo.Version}]已经注册。");

            
            var driverContext = DriverManager.Instance.GetDriverContext(driverInfo.Id);
            driverContext.GetRelateDevicesAndChannels(out var devices,out var channels);
            return new QuickNV.Protocol.Driver.QpCommands.Register.Response()
            {
                Devices = devices,
                Channels = channels
            };
        }
    }
}
