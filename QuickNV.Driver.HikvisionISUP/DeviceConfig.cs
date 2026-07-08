using QuickNV.HikvisionISUPSDK.Api;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.HikvisionISUP
{
    [JsonSerializable(typeof(DeviceConfig))]
    [JsonSourceGenerationOptions(
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
WriteIndented = true)]
    internal partial class DeviceConfigSerializerContext : JsonSerializerContext { }

    public class DeviceConfig
    {
        public string SdkDeviceId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter<SmsLinkMode>))]
        public SmsLinkMode StreamTransferMode { get; set; } = SmsLinkMode.TCP;
    }
}
