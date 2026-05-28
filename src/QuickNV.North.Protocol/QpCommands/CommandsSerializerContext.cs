using System.Text.Json.Serialization;

namespace QuickNV.North.Protocol.QpCommands;

[JsonSerializable(typeof(Register.Request))]
[JsonSerializable(typeof(Register.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class RegisterCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetAddressData.Request))]
[JsonSerializable(typeof(GetAddressData.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class GetAddressDataCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetDeviceData.Request))]
[JsonSerializable(typeof(GetDeviceData.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class GetDeviceDataCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetChannelData.Request))]
[JsonSerializable(typeof(GetChannelData.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class GetChannelDataCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Sync.Request))]
[JsonSerializable(typeof(Sync.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class SyncCommandSerializerContext : JsonSerializerContext { }
