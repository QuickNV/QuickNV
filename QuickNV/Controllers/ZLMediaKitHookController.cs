using Microsoft.AspNetCore.Mvc;
using Quick.ZLMediaKit.WebHook.Model;
using System.ComponentModel;
using System.Text.Json;
using YiQiDong.Agent;

namespace QuickNV.Controllers
{
    [DisplayName("ZLMediaKit回调相关")]
    [ApiController]
    [Route("/api/zlhook")]
    public class ZLMediaKitHookController : ControllerBase
    {
        [HttpPost("on_server_keepalive")]
        public ResultBase OnKeepalive([FromBody] EventBase content)
        {
            var mediaServer = Core.MediaServerManager.Instance.GetMediaServer(content.MediaServerId);
            if (mediaServer == null)
                return new ResultBase() { Code = -1, Msg = $"未找到编号为[{content.MediaServerId}]的媒体服务器" };
            return mediaServer.Keepalive(HttpContext.Connection.RemoteIpAddress);            
        }

        [HttpPost("on_flow_report")]
        public ResultBase OnFlowReport([FromBody] StreamChangedInfo content)
        {
            AgentContext.LogTrace("on_flow_report: " + JsonSerializer.Serialize(content));
            return new ResultBase();
        }

        [HttpPost("on_stream_none_reader")]
        public StreamNoneReaderInfoResult OnStreamNoneReader([FromBody] StreamNoneReaderInfo content)
        {
            AgentContext.LogTrace("on_stream_none_reader: " + JsonSerializer.Serialize(content));
            var mediaServer = Core.MediaServerManager.Instance.GetMediaServer(content.MediaServerId);
            if (mediaServer == null)
                return new StreamNoneReaderInfoResult();
            return mediaServer.OnStreamNoneReader(content);
        }

        [HttpPost("on_stream_changed")]
        public ResultBase OnStreamChanged([FromBody] StreamChangedInfo content)
        {
            AgentContext.LogTrace("on_stream_changed: " + JsonSerializer.Serialize(content));
            var mediaServer = Core.MediaServerManager.Instance.GetMediaServer(content.MediaServerId);
            if (mediaServer == null)
                return new ResultBase() { Code = -1, Msg = $"未找到编号为[{content.MediaServerId}]的媒体服务器" };
            mediaServer.OnStreamChanged(content);
            return new ResultBase();
        }

        [HttpPost("on_stream_not_found")]
        public ResultBase OnStreamNotFound([FromBody] StreamNotFoundInfo content)
        {
            AgentContext.LogTrace("on_stream_not_found: " + JsonSerializer.Serialize(content));
            var mediaServer = Core.MediaServerManager.Instance.GetMediaServer(content.MediaServerId);
            if (mediaServer == null)
                return new ResultBase() { Code = -1, Msg = $"未找到编号为[{content.MediaServerId}]的媒体服务器" };
            var ret = mediaServer.OnStreamNotFound(content);
            AgentContext.LogTrace("on_stream_not_found result: " + JsonSerializer.Serialize(ret));
            return ret;
        }

        [HttpPost("on_play")]
        public PlayInfoResult OnPlay([FromBody] PlayInfo content)
        {
            AgentContext.LogTrace("on_play: " + JsonSerializer.Serialize(content));
            var mediaServer = Core.MediaServerManager.Instance.GetMediaServer(content.MediaServerId);
            if (mediaServer == null)
                return new PlayInfoResult() { Code = -1, Msg = $"未找到编号为[{content.MediaServerId}]的媒体服务器" };

            return new PlayInfoResult();
        }

        [HttpPost("on_publish")]
        public object OnPublish([FromBody] PublishInfo content)
        {
            AgentContext.LogTrace("on_publish: " + JsonSerializer.Serialize(content));
            var mediaServer = Core.MediaServerManager.Instance.GetMediaServer(content.MediaServerId);
            if (mediaServer == null)
                return new { code = -1, msg = $"未找到编号为[{content.MediaServerId}]的媒体服务器" };
            var ret = mediaServer.OnPublish(content);
            AgentContext.LogTrace("on_publish result: " + JsonSerializer.Serialize(ret));
            return ret;
        }
    }
}
