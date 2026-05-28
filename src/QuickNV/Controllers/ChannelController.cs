using Microsoft.AspNetCore.Mvc;
using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel;
using System.Data;
using YiQiDong.Core.Utils;
using QuickNV.Core;
using QuickNV.Driver.Protocol.QpModels;
using QuickNV.Model;

namespace QuickNV.Controllers
{
    [DisplayName("通道相关")]
    [ApiController]
    [Route("/api/channel")]
    public class ChannelController : ControllerBase
    {
        /// <summary>
        /// 获取通道列表
        /// </summary>
        /// <param name="searchChannelId">搜索通道编号</param>
        /// <param name="searchChannelName">搜索通道名称</param>
        /// <param name="searchIsOnline">搜索是否在线</param>
        /// <param name="offset">偏移量</param>
        /// <param name="limit">页大小</param>
        /// <returns></returns>
        [HttpGet]
        public Model.PagedListResult<Model.Channel> GetChannels(
            string searchChannelId,
            string searchChannelName,
            bool? searchIsOnline,
            int offset,
            int limit)
        {
            IEnumerable<Model.Channel> query = ChannelManager.Instance.GetChannels(null);
            if (!string.IsNullOrEmpty(searchChannelId))
                query = query.Where(t => t.Id.Contains(searchChannelId));
            if (!string.IsNullOrEmpty(searchChannelName))
                query = query.Where(t => t.Name.Contains(searchChannelName));
            if (searchIsOnline.HasValue)
                query = query.Where(t => DeviceManager.Instance.GetDevice(t.DeviceId).IsOnline == searchIsOnline.Value);
            var channels = query.ToArray();
            return new PagedListResult<Channel>()
            {
                Total = channels.Length,
                Root = channels.Skip(offset).Take(limit).ToArray()
            };
        }

        /// <summary>
        /// 添加通道
        /// </summary>
        /// <param name="channel"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task AddOrEditChannel(Model.Channel channel)
        {
            await ChannelManager.Instance.AddOrEditChannel(channel);
        }

        /// <summary>
        /// 删除通道
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="channelId"></param>
        /// <returns></returns>
        [HttpDelete("{deviceId}/{channelId}")]
        public async Task<ActionResult> DeleteChannel(string deviceId, string channelId)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                return NotFound($"未找到编号为[{deviceId}]的设备");
            var channel = device.GetChannel(channelId);
            if (channel == null)
                return NotFound($"设备[{deviceId}]中未找到编号为[{channelId}]的通道");

            await ChannelManager.Instance.DeleteChannel(channel);
            return Ok();
        }

        /// <summary>
        /// 获取通道的快照
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <param name="channelId">通道编号</param>
        /// <returns></returns>
        [HttpGet("snapshot/{deviceId}/{channelId}")]
        public async Task<ActionResult> GetChannelSnapshot(string deviceId, string channelId)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                return NotFound($"未找到编号为[{deviceId}]的设备");
            var channel = device.GetChannel(channelId);
            if (channel == null)
                return NotFound($"设备[{deviceId}]中未找到编号为[{channelId}]的通道");
            var driverContext = device.GetDriverContext();
            if (driverContext == null)
                return NotFound($"设备[{deviceId}]关联的驱动[{device.DriverId}]未找到或者未连接");
            var snapshotResponse = await driverContext.Snapshot(deviceId, channelId);
            Response.Headers.CacheControl = new Microsoft.Extensions.Primitives.StringValues("no-store");
            return File(snapshotResponse.Content, snapshotResponse.MimeType);
        }

        /// <summary>
        /// 云台控制
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <param name="channelId">通道编号</param>
        /// <param name="commandType">命令类型</param>
        /// <param name="moveSpeed">移动速度(0~1)</param>
        /// <returns></returns>
        [HttpPost("ptz/{deviceId}/{channelId}")]
        public async Task<ActionResult> PtzControl(string deviceId, string channelId, PTZCommandType commandType, float moveSpeed)
        {
            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                return NotFound($"未找到编号为[{deviceId}]的设备");
            var channel = device.GetChannel(channelId);
            if (channel == null)
                return NotFound($"设备[{deviceId}]中未找到编号为[{channelId}]的通道");
            var driverContext = device.GetDriverContext();
            if (driverContext == null)
                return NotFound($"设备[{deviceId}]关联的驱动[{device.DriverId}]未找到或者未连接");
            await driverContext.PtzControl(deviceId, channelId, commandType, moveSpeed);
            return Ok();
        }

        /// <summary>
        /// 预览通道
        /// </summary>
        /// <param name="deviceId">设备编号</param>
        /// <param name="channelId">通道编号</param>
        /// <param name="withPtz">是否带云台</param>
        /// <returns></returns>
        [HttpGet("/preview/{deviceId}/{channelId}")]
        public ActionResult Preview(string deviceId, string channelId, bool? withPtz)
        {
            var queryString = new QueryString();
            queryString = queryString.Add("DeviceId", deviceId);
            queryString = queryString.Add("ChannelId", channelId);
            //如果不带云台
            if (withPtz == null || !withPtz.Value)
                return Redirect($"../../live.html{queryString.ToUriComponent()}");
            return Redirect($"../../LiveWithPtz{queryString.ToUriComponent()}");
        }

        /// <summary>
        /// 外部调用显示通道
        /// </summary>
        /// <param name="externalId">外部编号</param>
        /// <returns></returns>
        [HttpGet("/show/external/{externalId}")]
        public ActionResult Show_External(string externalId)
        {
            try
            {
                Pages.View.Show(externalId);
                return Ok();
            }
            catch (Exception ex)
            {
                return Problem(title: "外部调用显示通道失败", detail: ExceptionUtils.GetExceptionMessage(ex));
            }
        }
    }
}
