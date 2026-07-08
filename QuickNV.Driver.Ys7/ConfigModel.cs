using System.Text.Json.Serialization;
using QuickNV.Driver.Agent;

namespace QuickNV.Driver.Ys7
{
    [JsonSerializable(typeof(ConfigModel))]
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

    public class ConfigModel : AbstractDriverConfigModel
    {
        public string Ys7ServerUrl { get; set; } = "https://open.ys7.com";
        public string Ys7AppKey { get; set; }
        public string Ys7Secret { get; set; }
    }
}
