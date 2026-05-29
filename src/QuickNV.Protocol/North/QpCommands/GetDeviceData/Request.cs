using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.North.Protocol.QpCommands.GetDeviceData
{
    [DisplayName("获取设备数据")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => North_GetDeviceDataCommandSerializerContext.Default.Request;

    }
}
