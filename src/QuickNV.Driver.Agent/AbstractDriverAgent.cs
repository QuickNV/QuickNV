using Quick.Protocol;
using System.Threading.Tasks;
using System.Threading;
using System;
using YiQiDong.Agent;
using YiQiDong.Core;
using YiQiDong.Core.Utils;
using System.Linq;
using System.Collections.Generic;
using Quick.Fields;
using QuickNV.Protocol.Driver.QpModels;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json.Serialization;
using Quick.Utils;

namespace QuickNV.Driver.Agent
{
    public abstract class AbstractDriverAgent<TConfigModel, TDeviceConfig, TChannelConfig> : AbstractAgent
        where TConfigModel : AbstractDriverConfigModel
        where TDeviceConfig : new()
        where TChannelConfig : new()
    {
        public TConfigModel Config { get; private set; }
        protected abstract JsonSerializerContext ConfigSerializerContext { get; }
        protected abstract TConfigModel ReadConfig();

        private CancellationTokenSource cts;
        private CommandExecuterManager commandExecuterManager;
        private NoticeHandlerManager noticeHandlerManager;

        protected bool IsConnected { get; private set; } = false;
        private QpClientOptions clientOptions = null;
        protected QpClient Client { get; private set; }
        private Dictionary<string, DriverDevice<TDeviceConfig, TChannelConfig>> deviceDict;

        public DriverDevice<TDeviceConfig, TChannelConfig>[] GetDevices()
        {
            lock (deviceDict)
                return deviceDict.Values.ToArray();
        }

        public DriverDevice<TDeviceConfig, TChannelConfig> GetDevice(string deviceId)
        {
            lock (deviceDict)
            {
                if (deviceDict.TryGetValue(deviceId, out var device))
                    return device;
                return null;
            }
        }

        public AbstractDriverAgent()
        {
            Quick.Protocol.Pipeline.QpPipelineClientOptions.RegisterUriSchema();
            Quick.Protocol.Tcp.QpTcpClientOptions.RegisterUriSchema();
            Quick.Protocol.WebSocket.Client.QpWebSocketClientOptions.RegisterUriSchema();

            commandExecuterManager = new CommandExecuterManager();
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.ImportDevices.Request(), ImportDevices);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.GetDeviceConfig.Request(), GetDeviceConfig);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.ImportChannels.Request(), ImportChannels);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.GetChannelConfig.Request(), GetChannelConfig);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.CreateChannelLiveStream.Request(), CreateChannelLiveStream);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Request(), CreateChannelPlaybackStream);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.DestoryChannelStream.Request(), DestoryChannelStream);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.PtzControl.Request(), PtzControl);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.Snapshot.Request(), Snapshot);
            commandExecuterManager.Register(new Protocol.Driver.QpCommands.FindPlaybackFiles.Request(), FindPlaybackFiles);

            noticeHandlerManager = new NoticeHandlerManager();
            noticeHandlerManager.Register<Protocol.Driver.QpNotices.DeviceAddedNotice>(DeviceAdded);
            noticeHandlerManager.Register<Protocol.Driver.QpNotices.DeviceDeletedNotice>(DeviceDeleted);
            noticeHandlerManager.Register<Protocol.Driver.QpNotices.ChannelAddedNotice>(ChannelAdded);
            noticeHandlerManager.Register<Protocol.Driver.QpNotices.ChannelDeletedNotice>(ChannelDeleted);
        }

        protected virtual void OnDeviceAdded(DriverDevice<TDeviceConfig, TChannelConfig> device) { }
        protected virtual void OnDeviceDeleted(DriverDevice<TDeviceConfig, TChannelConfig> device) { }
        protected virtual void OnChannelAdded(DriverDevice<TDeviceConfig, TChannelConfig> device, DriverChannel<TChannelConfig> channel) { }
        protected virtual void OnChannelDeleted(DriverDevice<TDeviceConfig, TChannelConfig> device, DriverChannel<TChannelConfig> channel) { }

        private void ChannelAdded(QpChannel channel, Protocol.Driver.QpNotices.ChannelAddedNotice package)
        {
            var channelInfo = package.Channel;
            var device = GetDevice(channelInfo.DeviceId);
            if (device == null)
                return;
            var channelModel = new DriverChannel<TChannelConfig>(package.Channel);
            device.AddChannel(channelModel);
            OnChannelAdded(device, channelModel);
        }

        private void ChannelDeleted(QpChannel channel, Protocol.Driver.QpNotices.ChannelDeletedNotice package)
        {
            var channelInfo = package.Channel;
            var device = GetDevice(channelInfo.DeviceId);
            if (device == null)
                return;
            var channelModel = device.GetChannel(channelInfo.Id);
            if (channelModel == null)
                return;
            device.DeleteChannel(channelModel);
            OnChannelDeleted(device, channelModel);
        }

        private void DeviceAdded(QpChannel channel, Protocol.Driver.QpNotices.DeviceAddedNotice package)
        {
            var deviceInfo = package.Device;
            var device = new DriverDevice<TDeviceConfig, TChannelConfig>(deviceInfo);
            lock (deviceDict)
            {
                deviceDict.Add(device.Id, device);
            }
            OnDeviceAdded(device);
        }

        private void DeviceDeleted(QpChannel channel, Protocol.Driver.QpNotices.DeviceDeletedNotice package)
        {
            var deviceInfo = package.Device;
            DriverDevice<TDeviceConfig, TChannelConfig> device = null;
            lock (deviceDict)
            {
                if (deviceDict.ContainsKey(deviceInfo.Id))
                    deviceDict.Remove(deviceInfo.Id, out device);
            }
            if (device != null)
                OnDeviceDeleted(device);
        }

        protected virtual Protocol.Driver.QpCommands.ImportDevices.Response ImportDevices(QpChannel channel, Protocol.Driver.QpCommands.ImportDevices.Request request)
        {
            return new Protocol.Driver.QpCommands.ImportDevices.Response()
            {
                Fields = new[] {
                        new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "警告",
                            Description="当前驱动不支持导入功能"
                        }
                }
            };
        }

        protected virtual Protocol.Driver.QpCommands.ImportChannels.Response ImportChannels(QpChannel channel, Protocol.Driver.QpCommands.ImportChannels.Request request)
        {
            return new Protocol.Driver.QpCommands.ImportChannels.Response()
            {
                Fields = new[] {
                        new FieldForGet()
                        {
                            Type = FieldType.Alert,
                            Name = "警告",
                            Description="当前驱动不支持导入功能"
                        }
                }
            };
        }

        protected abstract Protocol.Driver.QpCommands.GetDeviceConfig.Response GetDeviceConfig(QpChannel channel, Protocol.Driver.QpCommands.GetDeviceConfig.Request request);
        protected abstract Protocol.Driver.QpCommands.GetChannelConfig.Response GetChannelConfig(QpChannel channel, Protocol.Driver.QpCommands.GetChannelConfig.Request request);
        protected abstract Protocol.Driver.QpCommands.CreateChannelLiveStream.Response CreateChannelLiveStream(QpChannel channel, Protocol.Driver.QpCommands.CreateChannelLiveStream.Request request);
        protected abstract Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Response CreateChannelPlaybackStream(QpChannel channel, Protocol.Driver.QpCommands.CreateChannelPlaybackStream.Request request);
        protected abstract Protocol.Driver.QpCommands.DestoryChannelStream.Response DestoryChannelStream(QpChannel channel, Protocol.Driver.QpCommands.DestoryChannelStream.Request request);
        protected abstract Protocol.Driver.QpCommands.PtzControl.Response PtzControl(QpChannel channel, Protocol.Driver.QpCommands.PtzControl.Request request);
        private Protocol.Driver.QpCommands.Snapshot.Response ConvertToSnapshotResponse(byte[] buffer, ImageParameter parameter = null)
        {
            var image = Image.Load(buffer);
            var imageFormat = image.Metadata.DecodedImageFormat;
            var currentWidth = image.Width;
            var currentHeight = image.Height;
            var currentFormat = Enum.Parse<ImageFormat>(imageFormat.Name);
            if (parameter != null)
            {
                currentFormat = parameter.Format;
                var currentP = currentWidth * 1D / currentHeight;
                var maxP = parameter.MaxWidth * 1D / parameter.MaxHeight;

                if (currentP > maxP)
                {
                    if (currentWidth > parameter.MaxWidth)
                    {
                        currentHeight = currentHeight * parameter.MaxWidth / currentWidth;
                        currentWidth = parameter.MaxWidth;
                    }
                }
                else
                {
                    if (currentHeight > parameter.MaxHeight)
                    {
                        currentWidth = currentWidth * parameter.MaxHeight / currentHeight;
                        currentHeight = parameter.MaxHeight;
                    }
                }
                image.Mutate(x => x.Resize(currentWidth, currentHeight));
                var ms = new MemoryStream();
                switch (parameter.Format)
                {
                    case ImageFormat.PNG: image.SaveAsPng(ms); break;
                    case ImageFormat.BMP: image.SaveAsBmp(ms); break;
                    case ImageFormat.GIF: image.SaveAsGif(ms); break;
                    case ImageFormat.WEBP: image.SaveAsWebp(ms); break;
                    case ImageFormat.PBM: image.SaveAsPbm(ms); break;
                    case ImageFormat.TIFF: image.SaveAsTiff(ms); break;
                    case ImageFormat.TGA: image.SaveAsTga(ms); break;
                    case ImageFormat.JPEG: default: image.SaveAsJpeg(ms); break;
                }
                buffer = ms.ToArray();
                imageFormat = Image.DetectFormat(buffer);
            }
            return new Protocol.Driver.QpCommands.Snapshot.Response()
            {
                Content = buffer,
                Width = currentWidth,
                Height = currentHeight,
                Format = currentFormat,
                MimeType = imageFormat.DefaultMimeType
            };
        }

        protected virtual byte[] Snapshot(string deviceId, string channelId, ImageParameter parameter)
        {
            throw new IOException("当前驱动不支持通道快照");
        }

        private Protocol.Driver.QpCommands.Snapshot.Response Snapshot(QpChannel channel, Protocol.Driver.QpCommands.Snapshot.Request request)
        {
            var buffer = Snapshot(request.DeviceId, request.ChannelId, request.Parameter);
            if (buffer == null || buffer.Length == 0)
                throw new IOException("快照结果为空");
            return ConvertToSnapshotResponse(buffer, request.Parameter);
        }

        protected virtual VideoFileInfo[] FindPlaybackFiles(string deviceId, string channelId, DateTime startTime, DateTime endTime)
        {
            return new VideoFileInfo[0];
        }

        protected Protocol.Driver.QpCommands.FindPlaybackFiles.Response FindPlaybackFiles(QpChannel channel, Protocol.Driver.QpCommands.FindPlaybackFiles.Request request)
        {
            return new Protocol.Driver.QpCommands.FindPlaybackFiles.Response()
            {
                Files = FindPlaybackFiles(request.DeviceId, request.ChannelId, request.StartTime, request.EndTime)
            };
        }

        public override void Start()
        {
            Config = ReadConfig();
            base.Start();
            cts = new CancellationTokenSource();

            clientOptions = QpClientOptions.Parse(new Uri(Config.QuickNVDriverInterfaceUrl));
            clientOptions.Password = Config.QuickNVDriverInterfacePassword;
            clientOptions.InstructionSet = [Protocol.Driver.Instruction.Instance];
            clientOptions.RegisterCommandExecuterManager(commandExecuterManager);
            clientOptions.RegisterNoticeHandlerManager(noticeHandlerManager);
            _ = beginConnect(cts.Token);
        }

        private void Client_Disconnected(object sender, EventArgs e)
        {
            if (IsConnected)
            {
                IsConnected = false;
                AgentContext.LogDebug($"[QuickNV驱动接口]到[{Config.QuickNVDriverInterfaceUrl}]的连接已断开。原因：{ExceptionUtils.GetExceptionMessage(Client.LastException)}");
                try
                {
                    OnDriverDisconnected();
                }
                catch (Exception ex)
                {
                    AgentContext.LogError(ExceptionUtils.GetExceptionString(ex));
                }
                delayToConnect(cts.Token);
            }
        }

        private void delayToConnect(CancellationToken token)
        {
            clean();
            if (token.IsCancellationRequested)
                return;
            AgentContext.LogDebug($"[QuickNV驱动接口]将在5秒后重试连接到[{Config.QuickNVDriverInterfaceUrl}]...");
            Task.Delay(5000, token).ContinueWith(task =>
            {
                if (task.IsCanceled)
                    return;
                _ = beginConnect(token);
            });
        }

        //当驱动连接上QuickNV时
        protected abstract void OnDriverConnected();
        //当驱动与QuickNV的连接断开时
        protected abstract void OnDriverDisconnected();

        private DriverInfo _DriverInfo;
        public DriverInfo DriverInfo
        {
            get
            {
                if (_DriverInfo == null)
                    _DriverInfo = new DriverInfo()
                    {
                        Id = GetType().Namespace,
                        Name = AgentContext.Container?.Image?.Name ?? GetType().Namespace,
                        Version = AgentContext.Container?.Image?.Version ?? "0.0.0.0",
                        HasChannelConfig = HasChannelConfig,
                        CanImportChannel = CanImportChannel
                    };
                return _DriverInfo;
            }
        }

        public virtual bool HasChannelConfig { get; } = false;
        public virtual bool CanImportChannel { get; } = false;

        private void clean()
        {
            var client = Client;
            if (client != null)
            {
                client.Disconnected -= Client_Disconnected;
                client.Close();
                Client = null;
            }
        }

        private async Task beginConnect(CancellationToken token)
        {
            try
            {
                AgentContext.LogDebug($"[QuickNV驱动接口]正在连接到[{Config.QuickNVDriverInterfaceUrl}]...");
                var client = Client = clientOptions.CreateClient();
                client.Disconnected += Client_Disconnected;
                await client.ConnectAsync();

                try
                {
                    var rep = await client.SendCommand(new Protocol.Driver.QpCommands.Register.Request()
                    {
                        CurrentDriver = DriverInfo
                    });
                    deviceDict = rep.Devices
                                    .Select(t => new DriverDevice<TDeviceConfig, TChannelConfig>(t))
                                    .ToDictionary(t => t.Id, t => t);
                    foreach (var channelGroup in rep.Channels.GroupBy(t => t.DeviceId))
                    {
                        var deviceId = channelGroup.Key;
                        if (!deviceDict.TryGetValue(deviceId, out var device))
                            continue;
                        foreach (var channel in channelGroup)
                            device.AddChannel(new DriverChannel<TChannelConfig>(channel));
                    }
                    AgentContext.LogDebug($"[QuickNV驱动接口]已加载[{rep.Devices.Length}]个设备.");
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("注册驱动接口失败", ex);
                }
                AgentContext.LogDebug($"[QuickNV驱动接口]连接到[{Config.QuickNVDriverInterfaceUrl}]成功.");
                IsConnected = true;
                try
                {
                    OnDriverConnected();
                }
                catch (Exception ex)
                {
                    AgentContext.LogWarn($"[QuickNV驱动接口]通知驱动注册成功时出错，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                }
            }
            catch (Exception ex)
            {
                AgentContext.LogDebug($"[QuickNV驱动接口]连接到[{Config.QuickNVDriverInterfaceUrl}]失败，原因：{ExceptionUtils.GetExceptionMessage(ex)}");
                delayToConnect(token);
            }
        }

        public override void Stop()
        {
            cts?.Cancel();
            cts = null;
            clean();
            IsConnected = false;
            base.Stop();
        }

        public void SendDeviceLogNotice(string deviceId, string message)
        {
            Client.SendNoticePackage(new Protocol.Driver.QpNotices.DeviceLogNotice() { DeviceId = deviceId, Message = message });
        }

        public void SendDeviceOnlineNotice(DriverDevice<TDeviceConfig, TChannelConfig> device)
        {
            Client.SendNoticePackage(new Protocol.Driver.QpNotices.DeviceOnlineNotice(device));
        }

        public void SendDeviceOfflineNotice(string deviceId, string reason = null)
        {
            Client.SendNoticePackage(new Protocol.Driver.QpNotices.DeviceOfflineNotice() { DeviceId = deviceId, Reason = reason });
        }

        public async Task<StreamInfo> MediaServerAddStreamProxy(int mediaId, StreamInfo streamInfo, string streamUrl)
        {
            var rep = await Client.SendCommand(new Protocol.Driver.QpCommands.MediaServerAddStreamProxy.Request()
            {
                MediaId = mediaId,
                StreamInfo = streamInfo,
                StreamUrl = streamUrl
            });
            return rep.StreamInfo;
        }

        public async Task<MediaInfo> ChangeLiveStreamSSRC(string mediaServerId, int mediaId, string SSRC)
        {
            var rep = await Client.SendCommand(new Protocol.Driver.QpCommands.ChangeLiveStreamSSRC.Request()
            {
                MediaServerId = mediaServerId,
                MediaId = mediaId,
                SSRC = SSRC
            });
            return rep.MediaInfo;
        }

        public async Task<StreamInfo> GetMediaServerStreamInfo(string mediaServerId, int mediaId)
        {
            var rep = await Client.SendCommand(new Protocol.Driver.QpCommands.GetMediaServerStreamInfo.Request()
            {
                MediaServerId = mediaServerId,
                MediaId = mediaId
            });
            return rep.StreamInfo;
        }
    }
}