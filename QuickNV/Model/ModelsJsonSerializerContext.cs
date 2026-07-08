using System;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace QuickNV.Model;


[JsonSerializable(typeof(Address))]
[JsonSerializable(typeof(Channel))]
[JsonSerializable(typeof(Config))]
[JsonSerializable(typeof(Device))]
[JsonSerializable(typeof(MediaServer))]
[JsonSerializable(typeof(PlayerConfigModel))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public partial class ModelsJsonSerializerContext : JsonSerializerContext
{
    public static ModelsJsonSerializerContext Default2 { get; } = new ModelsJsonSerializerContext(new JsonSerializerOptions()
    {
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    });
}