using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.North.QpModels;

namespace QuickNV.Protocol.North.QpCommands.GetAddressData
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_GetAddressDataCommandSerializerContext.Default.Response;
        public AddressInfo[] Data { get; set; }
    }
}
