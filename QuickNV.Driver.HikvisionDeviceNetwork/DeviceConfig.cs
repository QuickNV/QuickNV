
using System.Text.Json.Serialization;
using QuickNV.HikvisionNetSDK.Api;

namespace QuickNV.Driver.HikvisionDeviceNetwork
{
    [JsonSerializable(typeof(DeviceConfig))]
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true)]
    internal partial class DeviceConfigSerializerContext : JsonSerializerContext { }

    public class DeviceConfig
    {
        public string Host { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int Port { get; set; } = 8000;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Encoding { get; set; } = "GB18030";
        [JsonConverter(typeof(JsonStringEnumConverter<HvRtspPathFormat>))]
        public HvRtspPathFormat RtspPathFormat { get; set; } = HvRtspPathFormat.Streaming;
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int RtspPort { get; set; }
    }
}
