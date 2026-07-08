using QuickNV.DahuaNetSDK.Api;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.DahuaDeviceNetwork
{
    [JsonSerializable(typeof(ChannelConfig))]
    [JsonSourceGenerationOptions(
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        WriteIndented = true)]
    internal partial class ChannelConfigSerializerContext : JsonSerializerContext { }

    public class ChannelConfig
    {
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public int ChannelId { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter<DhStreamType>))]
        public DhStreamType StreamType { get; set; } = DhStreamType.Sub;
    }
}
