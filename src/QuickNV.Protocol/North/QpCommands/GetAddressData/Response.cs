using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.North.Protocol.QpModels;

namespace QuickNV.North.Protocol.QpCommands.GetAddressData
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_GetAddressDataCommandSerializerContext.Default.Response;
        public AddressInfo[] Data { get; set; }
    }
}
