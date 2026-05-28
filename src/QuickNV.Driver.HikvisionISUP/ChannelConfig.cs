using QuickNV.HikvisionISUPSDK.Api;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.HikvisionISUP
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
        [JsonConverter(typeof(JsonStringEnumConverter<SmsStreamType>))]
        public SmsStreamType StreamType { get; set; } = SmsStreamType.Sub;
    }
}
