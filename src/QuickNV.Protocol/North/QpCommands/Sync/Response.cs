using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.North.Protocol.QpCommands.Sync
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_SyncCommandSerializerContext.Default.Response;
        
    }
}
