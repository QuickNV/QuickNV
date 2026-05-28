using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol;
using YiQiDong.Agent;
using QuickNV.Core;
using QuickNV.Interfaces.Core;
using QuickNV.Model;
using Quick.Utils;

namespace QuickNV.Interfaces.North
{
    public class Manager
    {
        public static Manager Instance { get; } = new Manager();
        private Dictionary<string, QpChannel> channelDict = new Dictionary<string, QpChannel>();
        private AllInterface allInterface;
        private AllInterfaceConfig config;

        public void Init(IApplicationBuilder app, ConfigModel configModel)
        {
            config = new AllInterfaceConfig()
            {
                InterfaceName = "北向接口",
                InstructionSet = [QuickNV.North.Protocol.Instruction.Instance],
                Password = configModel.NorthInterfacePassword,
                WebSocketEnable = configModel.NorthInterfaceWebSocketEnable,
                WebSocketPath = "/ws/north",
                PipeEnable = configModel.NorthInterfacePipeEnable,
                PipeName = configModel.NorthInterfacePipeName,
                TcpEnable = configModel.NorthInterfaceTcpEnable,
                TcpListenAddress = configModel.NorthInterfaceTcpListenAddress,
                TcpListenPort = configModel.NorthInterfaceTcpListenPort
            };
            allInterface = new AllInterface(config, app);
        }

        public KeyValuePair<string, QpChannel>[] GetAllChannels()
        {
            return channelDict.ToArray();
        }

        public void Start()
        {
            allInterface.Start(config, commandExecuterManagerForRegister, noticeHandlerManager);
        }

        public void Stop()
        {
            allInterface.Stop();
        }

        private CommandExecuterManager commandExecuterManagerForRegister;
        private CommandExecuterManager commandExecuterManager;
        private NoticeHandlerManager noticeHandlerManager;

        private Manager()
        {
            commandExecuterManagerForRegister = new CommandExecuterManager();
            commandExecuterManagerForRegister.Register(new QuickNV.North.Protocol.QpCommands.Register.Request(), Register);

            commandExecuterManager = new CommandExecuterManager();
            commandExecuterManager.Register(new QuickNV.North.Protocol.QpCommands.GetAddressData.Request(), GetAddressData);
            commandExecuterManager.Register(new QuickNV.North.Protocol.QpCommands.GetDeviceData.Request(), GetDeviceData);
            commandExecuterManager.Register(new QuickNV.North.Protocol.QpCommands.GetChannelData.Request(), GetChannelData);
            commandExecuterManager.Register(new QuickNV.North.Protocol.QpCommands.Sync.Request(), Sync);
            noticeHandlerManager = new NoticeHandlerManager();
        }
        private QuickNV.North.Protocol.QpCommands.Register.Response Register(QpChannel channel, QuickNV.North.Protocol.QpCommands.Register.Request request)
        {
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                channel.Disconnected -= handler;
                lock (channelDict)
                    if (channelDict.ContainsKey(request.Name))
                        channelDict.Remove(request.Name);
                AgentContext.LogInfo($"[北向接口][{channel.ChannelName}]名称为[{request.Name}]的北向程序已经断开。原因：{ExceptionUtils.GetExceptionMessage(channel.LastException)}");
            };
            channel.Disconnected += handler;
            channel.AddCommandExecuterManager(commandExecuterManager);
            lock (channelDict)
                channelDict[request.Name] = channel;
            AgentContext.LogInfo($"[北向接口][{channel.ChannelName}]名称为[{request.Name}]的北向程序已经注册。");
            return new QuickNV.North.Protocol.QpCommands.Register.Response();
        }

        private QuickNV.North.Protocol.QpCommands.GetAddressData.Response GetAddressData(QpChannel channel, QuickNV.North.Protocol.QpCommands.GetAddressData.Request request)
        {
            var data = ConfigDbContext.CacheContext.Query<Address>()
                .Select(t => new QuickNV.North.Protocol.QpModels.AddressInfo()
                {
                    Id = t.Id,
                    Name = t.Name,
                    ParentId = t.ParentId
                })
                .ToArray();
            return new QuickNV.North.Protocol.QpCommands.GetAddressData.Response() { Data = data };
        }

        private QuickNV.North.Protocol.QpCommands.GetDeviceData.Response GetDeviceData(QpChannel channel, QuickNV.North.Protocol.QpCommands.GetDeviceData.Request request)
        {
            var data = ConfigDbContext.CacheContext.Query<Device>()
                .Select(t => new QuickNV.North.Protocol.QpModels.DeviceInfo()
                {
                    Id = t.Id,
                    Name = t.Name,
                    AddressId = t.AddressId,
                    ChannelsCount = t.ChannelsCount,
                    DriverConfig = t.DriverConfig,
                    DriverId = t.DriverId,
                    Enable = t.Enable,
                    FirmwareVersion = t.FirmwareVersion,
                    Lat = t.Lat,
                    Lng = t.Lng,
                    Manufacturer = t.Manufacturer,
                    Model = t.Model,
                    SerialNumber = t.SerialNumber
                })
                .ToArray();
            return new QuickNV.North.Protocol.QpCommands.GetDeviceData.Response() { Data = data };
        }

        private QuickNV.North.Protocol.QpCommands.GetChannelData.Response GetChannelData(QpChannel channel, QuickNV.North.Protocol.QpCommands.GetChannelData.Request request)
        {
            var data = ConfigDbContext.CacheContext.Query<Channel>()
                .Select(t => new QuickNV.North.Protocol.QpModels.ChannelInfo()
                {
                    DeviceId = t.DeviceId,
                    Id = t.Id,
                    Name = t.Name,
                    AddressId = t.AddressId,
                    DriverConfig = t.DriverConfig,
                    Lat = t.Lat,
                    Lng = t.Lng
                })
                .ToArray();
            return new QuickNV.North.Protocol.QpCommands.GetChannelData.Response() { Data = data };
        }

        private ModelChecker<Address> addressModelChecker = new ModelChecker<Address>(false,
            new ModelChecker<Address>.PropertyInfo(t => t.Id, (t, v) => t.Id = (string)v),
            new ModelChecker<Address>.PropertyInfo(t => t.Name, (t, v) => t.Name = (string)v),
            new ModelChecker<Address>.PropertyInfo(t => t.ParentId, (t, v) => t.ParentId = (string)v)
        );
        private ModelChecker<Device> deviceModelChecker = new ModelChecker<Device>(true,
            new ModelChecker<Device>.PropertyInfo(t => t.Id, (t, v) => t.Id = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.Name, (t, v) => t.Name = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.AddressId, (t, v) => t.AddressId = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.DriverId, (t, v) => t.DriverId = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.DriverConfig, (t, v) => t.DriverConfig = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.Lng, (t, v) => t.Lng = (double?)v),
            new ModelChecker<Device>.PropertyInfo(t => t.Lat, (t, v) => t.Lat = (double?)v),
            new ModelChecker<Device>.PropertyInfo(t => t.Manufacturer, (t, v) => t.Manufacturer = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.Model, (t, v) => t.Model = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.SerialNumber, (t, v) => t.SerialNumber = (string)v),
            new ModelChecker<Device>.PropertyInfo(t => t.FirmwareVersion, (t, v) => t.FirmwareVersion = (string)v)
        );
        private ModelChecker<Channel> channelModelChecker = new ModelChecker<Channel>(true,
            new ModelChecker<Channel>.PropertyInfo(t => t.DeviceId, (t, v) => t.DeviceId = (string)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.Id, (t, v) => t.Id = (string)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.Name, (t, v) => t.Name = (string)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.DriverConfig, (t, v) => t.DriverConfig = (string)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.Lng, (t, v) => t.Lng = (double?)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.Lat, (t, v) => t.Lat = (double?)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.ExternalId, (t, v) => t.ExternalId = (string)v),
            new ModelChecker<Channel>.PropertyInfo(t => t.AddressId, (t, v) => t.AddressId = (string)v)
        );

        private QuickNV.North.Protocol.QpCommands.Sync.Response Sync(QpChannel channel, QuickNV.North.Protocol.QpCommands.Sync.Request request)
        {
            //同步地点
            if (request.Address != null)
            {
                try
                {
                    AgentContext.LogInfo($"[北向接口]开始同步{request.Address.Length}条地点数据...");
                    var existModels = ConfigDbContext.CacheContext.Query<Address>();
                    var newModels = request.Address.Select(t => new Address()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        ParentId = t.ParentId
                    }).ToArray();
                    addressModelChecker.CheckModels(existModels, newModels, out var addList, out var updateList, out var deleteList);
                    if (deleteList.Count > 0)
                        ConfigDbContext.CacheContext.RemoveRange(deleteList.ToArray(), true);
                    if (updateList.Count > 0)
                        foreach (var model in updateList)
                            ConfigDbContext.CacheContext.Update(model);
                    if (addList.Count > 0)
                        ConfigDbContext.CacheContext.AddRange(addList);
                    AgentContext.LogInfo($"[北向接口]地点已同步。新增[{addList.Count}]条，更新[{updateList.Count}]条，删除[{deleteList.Count}]条");
                }
                catch (Exception ex)
                {
                    AgentContext.LogWarn($"同步地点数据时出错，原因：{ExceptionUtils.GetExceptionString(ex)}");
                    throw new ApplicationException("同步地点数据时出错", ex);
                }
            }
            //同步设备
            if (request.Device != null)
            {
                try
                {
                    AgentContext.LogInfo($"[北向接口]开始同步{request.Device.Length}条设备数据...");
                    var existModels = ConfigDbContext.CacheContext.Query<Device>();
                    var newModels = request.Device.Select(t => new Device()
                    {
                        Id = t.Id,
                        Name = t.Name,
                        AddressId = t.AddressId,
                        ChannelsCount = t.ChannelsCount,
                        DriverId = t.DriverId,
                        DriverConfig = t.DriverConfig,
                        Enable = t.Enable,
                        FirmwareVersion = t.FirmwareVersion,
                        Lat = t.Lat,
                        Lng = t.Lng,
                        Manufacturer = t.Manufacturer,
                        Model = t.Model,
                        SerialNumber = t.SerialNumber
                    }).ToArray();
                    deviceModelChecker.CheckModels(existModels, newModels, out var addList, out var updateList, out var deleteList);
                    if (deleteList.Count > 0)
                        foreach (var model in deleteList)
                            DeviceManager.Instance.DeleteDevice(model.Id).Wait();
                    if (updateList.Count > 0)
                        foreach (var model in updateList)
                            DeviceManager.Instance.EditDevice(model).Wait();
                    if (addList.Count > 0)
                        foreach (var model in addList)
                            DeviceManager.Instance.AddDevice(model).Wait();
                    AgentContext.LogInfo($"[北向接口]设备已同步。新增[{addList.Count}]，更新[{updateList.Count}]，删除[{deleteList.Count}]");
                }
                catch (Exception ex)
                {
                    AgentContext.LogWarn($"同步设备数据时出错，原因：{ExceptionUtils.GetExceptionString(ex)}");
                    throw new ApplicationException("同步设备数据时出错", ex);
                }
            }
            //同步通道
            if (request.Channel != null)
            {
                try
                {
                    AgentContext.LogInfo($"[北向接口]开始同步{request.Channel.Length}条通道数据...");
                    var existModels = ConfigDbContext.CacheContext.Query<Channel>();
                    var newModels = request.Channel.Select(t => new Channel()
                    {
                        DeviceId = t.DeviceId,
                        Id = t.Id,
                        Name = t.Name,
                        AddressId = t.AddressId,
                        Lat = t.Lat,
                        Lng = t.Lng,
                        DriverConfig = t.DriverConfig
                    }).ToArray();
                    channelModelChecker.CheckModels(existModels, newModels, out var addList, out var updateList, out var deleteList);
                    if (deleteList.Count > 0)
                        foreach (var model in deleteList)
                            ChannelManager.Instance.DeleteChannel(model).Wait();
                    if (updateList.Count > 0)
                        foreach (var model in updateList)
                            ChannelManager.Instance.EditChannel(model).Wait();
                    if (addList.Count > 0)
                        foreach (var model in addList)
                            ChannelManager.Instance.AddChannel(model).Wait();
                    AgentContext.LogInfo($"[北向接口]通道已同步。新增[{addList.Count}]，更新[{updateList.Count}]，删除[{deleteList.Count}]");
                }
                catch (Exception ex)
                {
                    AgentContext.LogWarn($"同步通道数据时出错，原因：{ExceptionUtils.GetExceptionString(ex)}");
                    throw new ApplicationException("同步通道数据时出错", ex);
                }
            }
            return new QuickNV.North.Protocol.QpCommands.Sync.Response();
        }
    }
}
