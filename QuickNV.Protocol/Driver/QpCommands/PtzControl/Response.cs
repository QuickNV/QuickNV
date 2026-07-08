using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.Driver.QpCommands.PtzControl
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_PtzControlCommandSerializerContext.Default.Response;
    }
}
