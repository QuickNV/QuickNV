using QuickNV.HikvisionNetSDK.Api;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.HikvisionDeviceNetwork
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
        [JsonConverter(typeof(JsonStringEnumConverter<HvStreamType>))]
        public HvStreamType StreamType { get; set; } = HvStreamType.Sub;
    }
}
