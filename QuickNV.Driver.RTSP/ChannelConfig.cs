using System.Text.Json.Serialization;

namespace QuickNV.Driver.RTSP
{
    [JsonSerializable(typeof(ChannelConfig))]
    [JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
    internal partial class ChannelConfigSerializerContext : JsonSerializerContext { }

    public class ChannelConfig
    {
        public string RtspUrl { get; set; }
    }
}
