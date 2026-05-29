using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.Register
{
    [DisplayName("注册驱动")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_RegisterCommandSerializerContext.Default.Request;
        public DriverInfo CurrentDriver { get; set; }
    }
}
