using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.North.Protocol.QpModels;

namespace QuickNV.North.Protocol.QpCommands.GetDeviceData
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => North_GetDeviceDataCommandSerializerContext.Default.Response;
        public DeviceInfo[] Data { get; set; }
    }
}
