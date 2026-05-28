using System.ServiceModel;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.Onvif
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
        public int Port { get; set; } = 80;
        [JsonConverter(typeof(JsonStringEnumConverter<HttpClientCredentialType>))]
        public HttpClientCredentialType ClientCredentialType { get; set; } = HttpClientCredentialType.Digest;
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Scheme { get; set; } = "http";
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int RtspPort { get; set; }
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int SnapshotPort { get; set; }
    }
}
