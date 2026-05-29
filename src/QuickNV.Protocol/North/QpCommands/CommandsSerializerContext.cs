using System.Text.Json.Serialization;

namespace QuickNV.North.Protocol.QpCommands;

[JsonSerializable(typeof(Register.Request))]
[JsonSerializable(typeof(Register.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class North_RegisterCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetAddressData.Request))]
[JsonSerializable(typeof(GetAddressData.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class North_GetAddressDataCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetDeviceData.Request))]
[JsonSerializable(typeof(GetDeviceData.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class North_GetDeviceDataCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetChannelData.Request))]
[JsonSerializable(typeof(GetChannelData.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class North_GetChannelDataCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Sync.Request))]
[JsonSerializable(typeof(Sync.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class North_SyncCommandSerializerContext : JsonSerializerContext { }
