using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.North.Protocol.QpCommands.Register
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => RegisterCommandSerializerContext.Default.Response;
    }
}
