using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.MediaServerAddStreamProxy
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_MediaServerAddStreamProxyCommandSerializerContext.Default.Response;
        public StreamInfo StreamInfo { get; set; }
    }
}
