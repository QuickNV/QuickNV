using System.Text.Json.Serialization;

namespace QuickNV.Driver.Onvif
{
    [JsonSerializable(typeof(ChannelConfig))]
    [JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
    internal partial class ChannelConfigSerializerContext : JsonSerializerContext { }

    public class ChannelConfig
    {
        public string ProfileToken { get; set; }
        public string VideoSourceToken { get; set; }
    }
}
