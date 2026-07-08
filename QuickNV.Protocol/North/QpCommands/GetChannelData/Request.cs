using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.North.QpCommands.GetChannelData
{
    [DisplayName("获取通道数据")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => North_GetChannelDataCommandSerializerContext.Default.Request;

    }
}
