using QuickNV.DahuaNetSDK;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.DahuaDeviceNetwork
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
        public int Port { get; set; } = 37777;
        public string UserName { get; set; }
        public string Password { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter<EM_LOGIN_SPAC_CAP_TYPE>))]
        public EM_LOGIN_SPAC_CAP_TYPE LoginType { get; set; } = EM_LOGIN_SPAC_CAP_TYPE.TCP;
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int RtspPort { get; set; }
    }
}
