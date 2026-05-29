using Microsoft.VisualBasic;
using Quick.EntityFrameworkCore.Plus;
using Quick.Fields;
using Quick.Protocol;
using QuickNV.Protocol.Driver.QpModels;
using QuickNV.Model;

namespace QuickNV.Core
{
    public class DriverContext
    {
        public QpChannel Channel { get; private set; }
        public DriverInfo DriverInfo { get; private set; }

        public DriverContext(QpChannel channel, DriverInfo driverInfo)
        {
            Channel = channel;
            DriverInfo = driverInfo;
        }

        public async Task OnAddChannel(Channel channel)
        {
            await Channel.SendNoticePackage(new Protocol.Driver.QpNotices.ChannelAddedNotice
            {
                Channel = channel
            });
        }

        public async Task OnAddDevice(Device device)
        {
            await Channel.SendNoticePackage(new Protocol.Driver.QpNotices.DeviceAddedNotice
            {
                Device = device
            });
            var channels = ConfigDbContext.CacheContext.Query<Channel>(t => t.DeviceId == device.Id);
            foreach (var channel in channels)
                await OnAddChannel(channel);
        }

        public async Task OnDelChannel(Channel channel)
        {
            await Channel.SendNoticePackage(new Protocol.Driver.QpNotices.ChannelDeletedNotice
            {
                Channel = channel
            });
        }

        public async Task OnDelDevice(Device device)
        {
            await Channel.SendNoticePackage(new Protocol.Driver.QpNotices.DeviceDeletedNotice
            {
                Device = device
            });
            var channels = ConfigDbContext.CacheContext.Query<Channel>(t => t.DeviceId == device.Id);
            foreach (var channel in channels)
                await OnDelChannel(channel);
        }

        public async Task<Protocol.Driver.QpCommands.ImportDevices.Response> ImportDevices(
            string[] fieldIds,
            FieldForPost[] fields)
        {
            return await Channel.SendCommand(new Protocol.Driver.QpCommands.ImportDevices.Request()
            {
                FieldIds = fieldIds,
                Fields = fields
            });
        }

        public async Task<Protocol.Driver.QpCommands.ImportChannels.Response> ImportChannels(string deviceId, string[] fieldIds, FieldForPost[] fields)
        {
            return await Channel.SendCommand(new Protocol.Driver.QpCommands.ImportChannels.Request()
            {
                DeviceId = deviceId,
                FieldIds = fieldIds,
                Fields = fields
            });
        }

        public async Task<Protocol.Driver.QpCommands.GetDeviceConfig.Response> GetDeviceConfig(
            string config,
            string[] fieldIds,
            FieldForPost[] fields)
        {
            return await Channel.SendCommand(new Protocol.Driver.QpCommands.GetDeviceConfig.Request()
            {
                Config = config,
                FieldIds = fieldIds,
                Fields = fields
            });
        }

        public async Task<Protocol.Driver.QpCommands.GetChannelConfig.Response> GetChannelConfig(
            string deviceId,
            string config,
            string[] fieldIds,
            FieldForPost[] fields)
        {
            return await Channel.SendCommand(new Protocol.Driver.QpCommands.GetChannelConfig.Request()
            {
                DeviceId = deviceId,
                Config = config,
                FieldIds = fieldIds,
                Fields = fields
            });
        }

        public async Task<StreamInfo> CreateChannelLiveStream(MediaServerContext mediaServer, MediaInfo mediaInfo)
        {
            var rep = await Channel.SendCommand(new Protocol.Driver.QpCommands.CreateChannelLiveStream.Request()
            {
                MediaServerInfo = new MediaServerInfo()
                {
                    Id = mediaServer.Model.Id,
                    Name = mediaServer.Model.Name,
                    PublicIpAddress = mediaServer.Model.PublicIpAddress,
                    RtpProxyPort = mediaServer.Config.RtpProxy.Port
                },
                MediaInfo = mediaInfo
            });
            return rep.LiveStreamInfo;
        }

        public async Task<StreamInfo> CreateChannelPlaybackStream(MediaServerContext mediaServer, MediaInfo mediaInfo, DateTime startTime, DateTime endTime)
        {
            var rep = await Channel.SendCommand(new Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Request()
            {
                MediaServerInfo = new MediaServerInfo()
                {
                    Id = mediaServer.Model.Id,
                    Name = mediaServer.Model.Name,
                    PublicIpAddress = mediaServer.Model.PublicIpAddress,
                    RtpProxyPort = mediaServer.Config.RtpProxy.Port
                },
                MediaInfo = mediaInfo,
                StartTime = startTime,
                EndTime = endTime
            });
            return rep.PlaybackStreamInfo;
        }

        public async Task DestoryStream(string deviceId, string channelId, int mediaId)
        {
            await Channel.SendCommand(new Protocol.Driver.QpCommands.DestoryChannelStream.Request()
            {
                DeviceId = deviceId,
                ChannelId = channelId,
                MediaId = mediaId
            });
        }

        public async Task PtzControl(string deviceId, string channelId, PTZCommandType commandType, float moveSpeed)
        {
            await Channel.SendCommand(new Protocol.Driver.QpCommands.PtzControl.Request()
            {
                DeviceId = deviceId,
                ChannelId = channelId,
                CommandType = commandType,
                MoveSpeed = moveSpeed
            });
        }

        public void Unregister()
        {
            var devices = ConfigDbContext.CacheContext.Query<Model.Device>(t => t.DriverId != null && t.DriverId == DriverInfo.Id);
            foreach (var device in devices)
                Interfaces.Driver.Manager.Instance.NoticeDeviceOffline(device, "驱动取消注册");
        }
        private Task<Protocol.Driver.QpCommands.Snapshot.Response> snapshotTask;
        public async Task<Protocol.Driver.QpCommands.Snapshot.Response> Snapshot(
            string deviceId,
            string channelId,
            ImageParameter parameter = null)
        {
            Task<Protocol.Driver.QpCommands.Snapshot.Response> currentTask = null;
            lock (this)
            {
                if (snapshotTask == null)
                {
                    snapshotTask = Channel.SendCommand(new Protocol.Driver.QpCommands.Snapshot.Request()
                    {
                        DeviceId = deviceId,
                        ChannelId = channelId,
                        Parameter = parameter
                    });
                }
                else
                {
                    snapshotTask = snapshotTask.ContinueWith(t =>
                    {
                        return Channel.SendCommand(new Protocol.Driver.QpCommands.Snapshot.Request()
                        {
                            DeviceId = deviceId,
                            ChannelId = channelId,
                            Parameter = parameter
                        }).Result;
                    });
                }
                currentTask = snapshotTask;
            }
            return await currentTask;
        }

        public async Task<Protocol.Driver.QpCommands.FindPlaybackFiles.Response> FindPlaybackFiles(
            string deviceId,
            string channelId,
            DateTime startTime,
            DateTime endTime)
        {
            return await Channel.SendCommand(new Protocol.Driver.QpCommands.FindPlaybackFiles.Request()
            {
                DeviceId = deviceId,
                ChannelId = channelId,
                StartTime = startTime,
                EndTime = endTime
            });
        }

        //获取驱动关联的设备通道
        public void GetRelateDevicesAndChannels(out Device[] devices, out Channel[] channels)
        {
            devices = ConfigDbContext.CacheContext
                    .Query<Device>(
                        t => t.DriverId != null
                        && t.DriverId == DriverInfo.Id
                        && t.Enable)
                    .ToArray();
            var channelList = new List<Channel>();
            foreach (var device in devices)
            {
                var deviceChannels = device.GetChannels();
                channelList.AddRange(deviceChannels);
                if (device.ChannelsCount != deviceChannels.Length)
                {
                    device.ChannelsCount = deviceChannels.Length;
                    ConfigDbContext.CacheContext.Update(device);
                }
            }
            channels = channelList.ToArray();
        }

        public async Task LoadDevicesAndChannelsAsync()
        {
            GetRelateDevicesAndChannels(out var devices, out var channels);
            foreach (var device in devices)
                await OnAddDevice(device).ConfigureAwait(false);
            foreach (var channel in channels)
                await OnAddChannel(channel).ConfigureAwait(false);
        }

        public async Task UnloadDevicesAndChannelsAsync()
        {
            GetRelateDevicesAndChannels(out var devices, out var channels);
            foreach (var channel in channels)
                await OnDelChannel(channel).ConfigureAwait(false);
            foreach (var device in devices)
                await OnDelDevice(device).ConfigureAwait(false);
        }
    }
}
