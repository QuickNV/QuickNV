using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.MediaServerAddStreamProxy
{
    [DisplayName("媒体服务器添加流代理")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_MediaServerAddStreamProxyCommandSerializerContext.Default.Request;
        /// <summary>
        /// 媒体编号
        /// </summary>
        public int MediaId { get; set; }
        /// <summary>
        /// 媒体信息
        /// </summary>
        public StreamInfo StreamInfo { get; set; }
        /// <summary>
        /// 媒体URL
        /// </summary>
        public string StreamUrl { get; set; }
    }
}
