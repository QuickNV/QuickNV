using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Core
{
    public class DeviceManager
    {
        public static DeviceManager Instance { get; } = new DeviceManager();

        private bool IsStringChanged(string a, string b)
        {
            if (a == null && b == null)
                return false;
            if (a == null)
                return true;
            return a != b;
        }

        public Model.Device GetDevice(string deviceId)
        {
            return ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
        }

        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <returns></returns>
        public Model.Device[] GetDevices()
        {
            return ConfigDbContext.CacheContext.Query<Model.Device>()
                .OrderBy(t => t.Id)
                .ToArray();
        }

        /// <summary>
        /// 添加或编辑设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public async Task AddOrEditDevice(Model.Device model)
        {
            if (string.IsNullOrEmpty(model.Id))
                throw new ArgumentNullException(nameof(model.Id));

            if (ConfigDbContext.CacheContext.Find(model) == null)
                await AddDevice(model);
            else
                await EditDevice(model);
        }

        /// <summary>
        /// 添加设备
        /// </summary>
        /// <param name="model">设备模型</param>
        public async Task AddDevice(Model.Device model)
        {
            if (ConfigDbContext.CacheContext.Find(model) != null)
                throw new ApplicationException($"已经存在编号为[{model.Id}]的设备");
            ConfigDbContext.CacheContext.Add(model);
            var driverContext = model.GetDriverContext();
            if (driverContext != null && model.Enable)
                await driverContext.OnAddDevice(model);
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        public async Task DeleteDevice(string deviceId)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{deviceId}]的设备！");
            var driverContext = device.GetDriverContext();
            if (driverContext != null)
                await driverContext.OnDelDevice(device);
            ConfigDbContext.CacheContext.Remove(device, true);
        }

        /// <summary>
        /// 编辑设备
        /// </summary>
        /// <param name="model">设备模型</param>
        /// <returns></returns>
        public async Task EditDevice(Model.Device model)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(model.Id));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{model.Id}]的设备！");
            var preDriverContext = device.GetDriverContext();
            model.IsOnline = device.IsOnline;
            model.LogContext = device.LogContext;
            ConfigDbContext.CacheContext.Update(model);
            var driverContext = model.GetDriverContext();
            //检查是否需要通知驱动
            if (IsStringChanged(model.DriverId, device.DriverId)
                || IsStringChanged(model.DriverConfig, device.DriverConfig))
            {
                Interfaces.Driver.Manager.Instance.NoticeDeviceOffline(model, "设备驱动或配置变化");
                //通知原驱动删除设备
                if (preDriverContext != null)
                    await preDriverContext.OnDelDevice(device);
                //通知新驱动添加设备
                if (driverContext != null && model.Enable)
                    await driverContext.OnAddDevice(model);
            }

            //如果配置了地点
            if (!string.IsNullOrEmpty(model.AddressId))
            {
                //检查通道是否有配置地点，如果没有配置，则设置为与设备相同的地点
                foreach (var channel in model.GetChannels())
                {
                    if (string.IsNullOrEmpty(channel.AddressId))
                    {
                        channel.AddressId = model.AddressId;
                        ConfigDbContext.CacheContext.Update(channel);
                    }
                }
            }
            //如果配置了经纬度
            if (model.Lat.HasValue && model.Lng.HasValue)
            {
                //检查通道是否有配置经纬度，如果没有配置，则设置为与设备相同的经纬度
                foreach (var channel in model.GetChannels())
                {
                    if (channel.Lat == null || channel.Lng == null)
                    {
                        channel.Lat = model.Lat;
                        channel.Lng = model.Lng;
                        ConfigDbContext.CacheContext.Update(channel);
                    }
                }
            }
        }

        /// <summary>
        /// 获取指定设备的通道列表
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        public Model.Channel[] GetChannels(string deviceId)
        {
            return ConfigDbContext.CacheContext.Query<Model.Channel>(t => t.DeviceId == deviceId)
                .OrderBy(t => t.Id)
                .ToArray();
        }

        /// <summary>
        /// 启用设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task Enable(Model.Device model)
        {
            model.Enable = true;
            ConfigDbContext.CacheContext.Update(model);
            var driverContext = model.GetDriverContext();
            if (driverContext != null)
                await driverContext.OnAddDevice(model);
        }

        /// <summary>
        /// 禁用设备
        /// </summary>
        /// <param name="model"></param>
        /// <returns></returns>
        public async Task Disable(Model.Device model)
        {
            model.Enable = false;
            ConfigDbContext.CacheContext.Update(model);
            var driverContext = model.GetDriverContext();
            if (driverContext != null)
                await driverContext.OnDelDevice(model);
            model.IsOnline = false;
        }
    }
}