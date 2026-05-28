using Microsoft.AspNetCore.Mvc;
using NPOI.Util;
using Quick.EntityFrameworkCore.Plus;
using Quick.Protocol.Utils;
using System.ComponentModel;
using System.Text.Json;
using YiQiDong.Agent;
using QuickNV.Core;
using QuickNV.Core.Utils;
using QuickNV.Core.Web;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Controllers
{
    [DisplayName("播放器相关")]
    [ApiController]
    [Route("/api/player")]
    public class PlayerController : ControllerBase, IDisposable
    {
        private Timer timer;
        private Queue<GetLiveStreamContext> taskQueue = new Queue<GetLiveStreamContext>();

        public PlayerController()
        {
            timer = new Timer(checkLiveStreamTask, null, 1000, 1000);
        }

        private class GetLiveStreamContext
        {
            public Task Task { get; private set; }
            public GetLiveStreamContext()
            {
                Task = new Task(() => { });
            }

            public void SetDone()
            {
                Task.Start();
            }
        }


        private void checkLiveStreamTask(object _)
        {
            GetLiveStreamContext context = null;
            lock (taskQueue)
            {
                if (taskQueue.Count <= 0)
                    return;
                context = taskQueue.Dequeue();
            }
            context.SetDone();
        }

        public void Dispose()
        {
            timer?.Dispose();
        }

        [HttpGet("conf")]
        public Model.PlayerConfigModel GetConf() => Components.Controls.Pages.PlayerConfigManage.GlobalConfig;

        [HttpGet("live")]
        public async Task<ActionResult<LiveResponse>> GetLiveStreamUrl([FromQuery] string deviceId, [FromQuery] string channelId, [FromQuery] bool useProxy)
        {
            var context = new GetLiveStreamContext();
            lock (taskQueue)
                taskQueue.Enqueue(context);
            await context.Task;

            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                return NotFound($"未找到编号为[{deviceId}]的设备");
            if (!device.IsOnline)
                return NotFound($"{device}当前已离线");
            var driverContext = device.GetDriverContext();
            if (driverContext == null)
                return NotFound($"未找到{device}的驱动:{device.DriverId}");

            var channel = ConfigDbContext.CacheContext.Find(new Model.Channel(deviceId, channelId));
            if (channel == null)
                return NotFound($"在{device}中未找到编号为[{channelId}]的通道");

            MediaServerContext mediaServer = null;
            StreamInfo liveStreamInfo = channel.LiveStreamInfo;
            if (liveStreamInfo != null)
                mediaServer = MediaServerManager.Instance.GetMediaServer(liveStreamInfo.MediaServerId);

            //媒体服务器如果不存在或者未连接
            if (mediaServer == null || !mediaServer.IsConnected)
            {
                liveStreamInfo = channel.LiveStreamInfo = null;
            }
            else
            {
                var mediaId = MediaStreamUtils.GetMediaIdFromStreamId(liveStreamInfo.Stream);
                var mediaInfo = mediaServer.GetMediaInfo(mediaId);
                if (mediaInfo == null)
                    liveStreamInfo = channel.LiveStreamInfo = null;
            }
            if (liveStreamInfo == null)
            {
                mediaServer = MediaServerManager.Instance.GetNext();
                if (mediaServer != null)
                {
                    MediaInfo mediaInfo = null;
                    try
                    {
                        mediaInfo = mediaServer.GenerateMediaInfo(device, channel, true);
                        liveStreamInfo = await driverContext.CreateChannelLiveStream(mediaServer, mediaInfo);
                        channel.LiveStreamInfo = liveStreamInfo;
                    }
                    catch (Exception ex)
                    {
                        if (mediaInfo != null)
                            mediaServer.DestoryMediaId(mediaInfo.MediaId);

                        AgentContext.LogWarn($"[{device.Name}-{channel.Name}]创建实时媒体流失败，错误：{ExceptionUtils.GetExceptionString(ex)}");
                        return Problem(
                            ExceptionUtils.GetExceptionMessage(ex),
                            $"{device.Name}-{channel.Name}",
                            500, "创建实时媒体流失败",
                            ex.GetType().FullName);
                    }
                }
            }
            if (mediaServer == null)
                return NotFound($"没有找到连接的媒体服务器.");

            var url = mediaServer.Model.GetWsUrl($"/{liveStreamInfo.App}/{liveStreamInfo.Stream}.live.flv");
            //如果使用代理
            if (useProxy)
                url = "." + ReverseProxyManager.Instance.GetProxyMediaServerWsPath(mediaServer.Model, $"/{liveStreamInfo.App}/{liveStreamInfo.Stream}.live.flv");
            AgentContext.LogDebug($"[GetLiveStreamUrl]deviceId:{deviceId},channelId:{channelId} -> {url}");
            return new LiveResponse()
            {
                Url = url,
                ChannelName = channel.Name,
                DeviceName = device.Name
            };
        }

        /// <summary>
        /// 获取回放时间轴
        /// </summary>
        /// <param name="deviceId"></param>
        /// <param name="channelId"></param>
        /// <param name="startTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        [HttpGet("playback/timeline")]
        public async Task<ActionResult<VideoFileInfo[]>> GetPlaybackTimeline(
            [FromQuery] string deviceId,
            [FromQuery] string channelId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime)
        {
            var device = DeviceManager.Instance.GetDevice(deviceId);
            if (device == null)
                return NotFound($"未找到编号为[{deviceId}]的设备");
            var driverContext = device.GetDriverContext();
            if(driverContext==null)
                return Problem($"设备的驱动[{device.DriverId}]当前未连接！");
            var rep = await driverContext.FindPlaybackFiles(deviceId, channelId, startTime, endTime);
            var files = rep.Files;
            foreach (var file in files)
            {
                if (file.StartTime < startTime)
                    file.StartTime = startTime;
                if (file.EndTime > endTime)
                    file.EndTime = endTime;
            }
            return files;
        }

        [HttpGet("playback")]
        public async Task<ActionResult<LiveResponse>> GetPlaybackStreamUrl(
            [FromQuery] string deviceId,
            [FromQuery] string channelId,
            [FromQuery] DateTime startTime,
            [FromQuery] DateTime endTime,
            [FromQuery] bool useProxy)
        {
            var context = new GetLiveStreamContext();
            lock (taskQueue)
                taskQueue.Enqueue(context);
            await context.Task;

            var device = ConfigDbContext.CacheContext.Find(new Model.Device(deviceId));
            if (device == null)
                return NotFound($"未找到编号为[{deviceId}]的设备");
            if (!device.IsOnline)
                return NotFound($"{device}当前已离线");
            var driverContext = device.GetDriverContext();
            if (driverContext == null)
                return NotFound($"未找到{device}的驱动:{device.DriverId}");

            var channel = ConfigDbContext.CacheContext.Find(new Model.Channel(deviceId, channelId));
            if (channel == null)
                return NotFound($"在{device}中未找到编号为[{channelId}]的通道");
               
            MediaServerContext mediaServer = null;
            StreamInfo playbackStreamInfo = channel.PlaybackStreamInfo;
            //如果当前通道现在正在回放，则先关闭流
            if (playbackStreamInfo != null)
            {
                mediaServer = MediaServerManager.Instance.GetMediaServer(playbackStreamInfo.MediaServerId);
                if (mediaServer != null && !string.IsNullOrEmpty(playbackStreamInfo.Stream))
                {
                    var mediaId = MediaStreamUtils.GetMediaIdFromStreamId(playbackStreamInfo.Stream);
                    mediaServer.DestoryMediaId(mediaId);
                    if (!string.IsNullOrEmpty(playbackStreamInfo.StreamProxyKey))
                        await mediaServer.DelStreamProxy(playbackStreamInfo.StreamProxyKey);
                }
                channel.PlaybackStreamInfo = null;
            }
            MediaInfo mediaInfo = null;
            mediaServer = MediaServerManager.Instance.GetNext();
            if (mediaServer != null)
            {
                try
                {
                    mediaInfo = mediaServer.GenerateMediaInfo(device, channel, false);
                    playbackStreamInfo = await driverContext.CreateChannelPlaybackStream(mediaServer, mediaInfo, startTime, endTime);
                    channel.PlaybackStreamInfo = playbackStreamInfo;
                }
                catch (Exception ex)
                {
                    if (mediaInfo != null)
                        mediaServer.DestoryMediaId(mediaInfo.MediaId);

                    AgentContext.LogWarn($"[{device.Name}-{channel.Name}]创建回放媒体流失败，错误：{ExceptionUtils.GetExceptionString(ex)}");
                    return Problem(
                        ExceptionUtils.GetExceptionMessage(ex),
                        $"{device.Name}-{channel.Name}",
                        500, "创建回放媒体流失败",
                        ex.GetType().FullName);
                }
            }
            if (mediaServer == null)
                return NotFound($"没有找到连接的媒体服务器.");
            var url = mediaServer.Model.GetWsUrl($"/{playbackStreamInfo.App}/{playbackStreamInfo.Stream}.live.flv");
            //使用代理
            if (useProxy)
                url = "." + ReverseProxyManager.Instance.GetProxyMediaServerWsPath(mediaServer.Model, $"/{playbackStreamInfo.App}/{playbackStreamInfo.Stream}.live.flv");
            AgentContext.LogDebug($"[GetPlaybackStreamUrl]deviceId:{deviceId},channelId:{channelId} -> {url}");
            return new LiveResponse()
            {
                Url = url,
                ChannelName = channel.Name,
                DeviceName = device.Name
            };
        }

        public class LiveResponse
        {
            public string Url { get; set; }
            public string ChannelName { get; set; }
            public string DeviceName { get; set; }
        }
    }
}
