using System.ServiceModel;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.RTSP
{
    [JsonSerializable(typeof(DeviceConfig))]
    [JsonSourceGenerationOptions(
DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
WriteIndented = true)]
    internal partial class DeviceConfigSerializerContext : JsonSerializerContext { }

    public class DeviceConfig
    {
        public string RtspUrlTemplate { get; set; }
    }
}
