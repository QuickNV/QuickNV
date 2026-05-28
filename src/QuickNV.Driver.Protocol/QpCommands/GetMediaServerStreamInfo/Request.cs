using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpCommands.GetMediaServerStreamInfo
{
    [DisplayName("获取媒体服务器流信息")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => GetMediaServerStreamInfoCommandSerializerContext.Default.Request;
        public string MediaServerId { get; set; }
        public int MediaId { get; set; }
    }
}
