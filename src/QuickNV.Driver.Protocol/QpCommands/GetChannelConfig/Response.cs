using Quick.Fields;
using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpCommands.GetChannelConfig
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => GetChannelConfigCommandSerializerContext.Default.Response;
        public string Config { get; set; }
        public FieldForGet[] Fields { get; set; }
    }
}
