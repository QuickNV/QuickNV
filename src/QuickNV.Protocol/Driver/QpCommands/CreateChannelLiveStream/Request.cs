using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.CreateChannelLiveStream
{
    [DisplayName("获取通道实时流")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_CreateChannelLiveStreamCommandSerializerContext.Default.Request;
        /// <summary>
        /// 媒体服务器信息
        /// </summary>
        public MediaServerInfo MediaServerInfo { get; set; }
        /// <summary>
        /// 媒体信息
        /// </summary>
        public MediaInfo MediaInfo { get; set; }
    }
}
