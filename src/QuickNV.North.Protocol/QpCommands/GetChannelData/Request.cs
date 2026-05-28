using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.North.Protocol.QpCommands.GetChannelData
{
    [DisplayName("获取通道数据")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => GetChannelDataCommandSerializerContext.Default.Request;

    }
}
