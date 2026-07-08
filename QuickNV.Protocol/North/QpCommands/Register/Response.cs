using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.North.QpCommands.Register
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_RegisterCommandSerializerContext.Default.Response;
    }
}
