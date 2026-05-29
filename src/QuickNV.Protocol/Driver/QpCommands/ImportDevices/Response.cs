using Quick.Fields;
using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.ImportDevices
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_ImportDevicesCommandSerializerContext.Default.Response;
        /// <summary>
        /// 要导入的设备
        /// </summary>
        public DeviceAndChannelsInfo[] Devices { get; set; }
        public FieldForGet[] Fields { get; set; }
    }
}
