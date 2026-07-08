using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.North.QpModels;

namespace QuickNV.Protocol.North.QpCommands.GetDeviceData
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_GetDeviceDataCommandSerializerContext.Default.Response;
        public DeviceInfo[] Data { get; set; }
    }
}
