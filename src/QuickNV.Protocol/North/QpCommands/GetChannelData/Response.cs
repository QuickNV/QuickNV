using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.North.Protocol.QpModels;

namespace QuickNV.North.Protocol.QpCommands.GetChannelData
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_GetChannelDataCommandSerializerContext.Default.Response;
        public ChannelInfo[] Data { get; set; }
    }
}
