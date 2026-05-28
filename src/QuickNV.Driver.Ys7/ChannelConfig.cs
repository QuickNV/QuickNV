using QuickNV.YS7.Model;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.Ys7
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
        [JsonConverter(typeof(JsonStringEnumConverter<VideoQuality>))]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public VideoQuality StreamType { get; set; } = VideoQuality.Sub;
        [JsonConverter(typeof(JsonStringEnumConverter<VideoProtocol>))]
        [Newtonsoft.Json.JsonConverter(typeof(Newtonsoft.Json.Converters.StringEnumConverter))]
        public VideoProtocol StreamProtocol { get; set; } = VideoProtocol.RTMP;
    }
}
