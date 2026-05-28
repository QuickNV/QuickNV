using Microsoft.AspNetCore.Mvc;
using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel;
using QuickNV.Core;

namespace QuickNV.Controllers
{
    [DisplayName("设备相关")]
    [ApiController]
    [Route("/api/device")]
    public class DeviceController : ControllerBase
    {
        private bool isStringChanged(string a, string b)
        {
            if (a == null && b == null)
                return false;
            if (a == null)
                return b != a;
            else
                return a != b;
        }
        

        [HttpGet("export/excel")]
        public ActionResult ExportExcel()
        {
            var dbContextBackupContext = new DbContextBackup.Excel.XlsxDbContextBackupContext();
            using (var dbContext = new ConfigDbContext())
            using (var ms = new MemoryStream())
            {
                dbContextBackupContext.Backup(dbContext, ms, null, [typeof(Model.Channel)]);
                return File(ms.ToArray(), "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", DateTime.Now.ToString("yyyyMMddHHmmss") + ".xlsx");
            }
        }

        /// <summary>
        /// 获取设备列表
        /// </summary>
        /// <param name="offset">偏移量</param>
        /// <param name="limit">页大小</param>
        /// <returns></returns>
        [HttpGet]
        public Model.PagedListResult<Model.Device> GetDevices(int offset, int limit)
        {
            var devices = DeviceManager.Instance.GetDevices();
            return new Model.PagedListResult<Model.Device>()
            {
                Total = devices.Length,
                Root = devices.Skip(offset).Take(limit).ToArray()
            };
        }

        /// <summary>
        /// 添加或编辑设备
        /// </summary>
        /// <param name="model">设备模型</param>
        [HttpPost]
        public async Task AddOrEditDevice(Model.Device model)
        {
            await DeviceManager.Instance.AddOrEditDevice(model);
        }

        /// <summary>
        /// 删除设备
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        /// <exception cref="ApplicationException"></exception>
        [HttpDelete("{deviceId}")]
        public async Task DeleteDevice(string deviceId)
        {
            await DeviceManager.Instance.DeleteDevice(deviceId);
        }

        /// <summary>
        /// 启用设备
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        [HttpPost("{deviceId}/Enable")]
        public async Task EnableDevice(string deviceId)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{deviceId}]的设备！");
            await DeviceManager.Instance.Enable(device);
        }

        /// <summary>
        /// 禁用设备
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        [HttpPost("{deviceId}/Disable")]
        public async Task DisableDevice(string deviceId)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                throw new ApplicationException($"未找到编号为[{deviceId}]的设备！");
            await DeviceManager.Instance.Disable(device);
        }

        /// <summary>
        /// 获取指定设备的通道列表
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <returns></returns>
        [HttpGet("{deviceId}/Channels")]
        public Model.Channel[] GetChannels(string deviceId)
        {
            return DeviceManager.Instance.GetChannels(deviceId);
        }
    }
}
