using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.North.Protocol.QpCommands.GetAddressData
{
    [DisplayName("获取地点数据")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => North_GetAddressDataCommandSerializerContext.Default.Request;

    }
}
