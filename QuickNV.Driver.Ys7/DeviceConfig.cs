using System.Text.Json.Serialization;

namespace QuickNV.Driver.Ys7
{
    [JsonSerializable(typeof(DeviceConfig))]
    [JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
    internal partial class DeviceConfigSerializerContext : JsonSerializerContext { }

    public class DeviceConfig
    {
        public string Ys7DeviceSerial { get; set; }
    }
}
