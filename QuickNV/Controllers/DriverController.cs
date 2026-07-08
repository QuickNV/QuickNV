using Microsoft.AspNetCore.Mvc;
using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel;
using QuickNV.Core;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Controllers
{
    [DisplayName("驱动相关")]
    [ApiController]
    [Route("/api/driver")]
    public class DriverController
    {
        /// <summary>
        /// 获取驱动列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public DriverInfo[] GetDrivers()
        {
            return DriverManager.Instance.DriverContexts.Select(t => t.DriverInfo).ToArray();
        }

        /// <summary>
        /// 获取设备配置
        /// </summary>
        /// <param name="driverId">驱动编号</param>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        [HttpPost("{driverId}/GetDeviceConfig")]
        public async Task<Protocol.Driver.QpCommands.GetDeviceConfig.Response> GetDeviceConfig(
            string driverId,
            [FromBody] Protocol.Driver.QpCommands.GetDeviceConfig.Request request)
        {
            var driverContext = DriverManager.Instance.GetDriverContext(driverId);
            return await driverContext.GetDeviceConfig(request.Config, request.FieldIds, request.Fields);
        }

        /// <summary>
        /// 获取通道配置
        /// </summary>
        /// <param name="driverId">驱动编号</param>
        /// <param name="request">请求参数</param>
        /// <returns></returns>
        [HttpPost("{driverId}/GetChannelConfig")]
        public async Task<Protocol.Driver.QpCommands.GetChannelConfig.Response> GetChannelConfig(
            string driverId,
            [FromBody] Protocol.Driver.QpCommands.GetChannelConfig.Request request)
        {

            var driverContext = DriverManager.Instance.GetDriverContext(driverId);
            return await driverContext.GetChannelConfig(
                request.DeviceId,
                request.Config,
                request.FieldIds,
                request.Fields);
        }
    }
}
