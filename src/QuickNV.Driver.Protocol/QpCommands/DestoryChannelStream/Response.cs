using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpCommands.DestoryChannelStream
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => DestoryChannelStreamCommandSerializerContext.Default.Response;
    }
}
