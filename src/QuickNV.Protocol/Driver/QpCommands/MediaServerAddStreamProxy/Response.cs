using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.MediaServerAddStreamProxy
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_MediaServerAddStreamProxyCommandSerializerContext.Default.Response;
        public StreamInfo StreamInfo { get; set; }
    }
}
