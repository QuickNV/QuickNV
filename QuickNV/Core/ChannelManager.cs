using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Core
{
    public class ChannelManager
    {
        public static ChannelManager Instance { get; } = new ChannelManager();

        public Model.Channel[] GetChannels(string deviceId)
        {
            if (string.IsNullOrEmpty(deviceId))
                return ConfigDbContext.CacheContext.Query<Model.Channel>();
            return ConfigDbContext.CacheContext.Query<Model.Channel>(t => t.DeviceId == deviceId);
        }

        public async Task AddOrEditChannel(Model.Channel model)
        {
            if (string.IsNullOrEmpty(model.DeviceId))
                throw new ArgumentNullException(nameof(model.DeviceId));
            if (string.IsNullOrEmpty(model.Id))
                throw new ArgumentNullException(nameof(model.Id));

            if (ConfigDbContext.CacheContext.Find(model) == null)
                await AddChannel(model);
            else
                await EditChannel(model);
        }

        public async Task AddChannel(Model.Channel model)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(model.DeviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{model.DeviceId}]的设备！");
            var driverContext = device.GetDriverContext();
            await AddChannel(driverContext, device, model);
        }

        public async Task AddChannel(DriverContext driverContext, Model.Device device, Model.Channel model)
        {
            if (ConfigDbContext.CacheContext.Find(model) != null)
                throw new ApplicationException($"设备[{model.DeviceId}]中已经存在编号为[{model.Id}]的通道");
            ConfigDbContext.CacheContext.Add(model);
            device.UpdateChannelsCount();
            await driverContext.OnAddChannel(model);
        }

        public async Task AddChannels(Model.Device device, DriverContext driverContext, Model.Channel[] models)
        {
            foreach (var model in models)
            {
                try
                {
                    if (ConfigDbContext.CacheContext.Find(model) != null)
                        throw new ApplicationException($"已经存在编号为[{model.Id}]的通道");
                    ConfigDbContext.CacheContext.Add(model);
                    await driverContext.OnAddChannel(model);
                }
                catch (Exception ex)
                {
                    throw new ApplicationException($"导入通道[{model.Name}]时出错", ex);
                }
            }
            device.UpdateChannelsCount();
        }

        public async Task EditChannel(Model.Channel model)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(model.DeviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{model.DeviceId}]的设备！");
            var driverContext = device.GetDriverContext();
            await EditChannel(driverContext, device, model);
        }

        public async Task EditChannel(DriverContext driverContext, Model.Device device, Model.Channel model)
        {
            if (driverContext != null)
                await driverContext.OnDelChannel(model);
            ConfigDbContext.CacheContext.Update(model);
            if (driverContext != null)
                await driverContext?.OnAddChannel(model);
        }

        public async Task DeleteChannel(Model.Channel model)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(model.DeviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{model.DeviceId}]的设备！");
            var driverContext = device.GetDriverContext();
            await DeleteChannel(driverContext, device, model);
        }


        public async Task DeleteChannel(DriverContext driverContext, Model.Device device, Model.Channel model)
        {
            await driverContext.OnDelChannel(model);
            ConfigDbContext.CacheContext.Remove(model);
            device.UpdateChannelsCount();
        }
    }
}
