using System.ComponentModel;
using System.Text.Json.Serialization;

namespace QuickNV.Driver.GB28181
{
    [JsonSerializable(typeof(DeviceConfig))]
    [JsonSourceGenerationOptions(
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true)]
    internal partial class DeviceConfigSerializerContext : JsonSerializerContext { }

    public class DeviceConfig
    {
        public enum TransferMode
        {
            [Description("UDP被动")]
            UDP_Passive,
            [Description("TCP被动")]
            TCP_Passive,
            [Description("TCP主动")]
            TCP_Active
        }

        [JsonConverter(typeof(JsonStringEnumConverter<TransferMode>))]
        public TransferMode StreamTransferMode { get; set; } = TransferMode.UDP_Passive;
    }
}
