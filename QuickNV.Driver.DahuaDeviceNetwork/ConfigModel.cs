using System.Text.Json.Serialization;
using QuickNV.Driver.Agent;

namespace QuickNV.Driver.DahuaDeviceNetwork
{
    [JsonSerializable(typeof(ConfigModel))]
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

    public class ConfigModel:AbstractDriverConfigModel
    {
    }
}
