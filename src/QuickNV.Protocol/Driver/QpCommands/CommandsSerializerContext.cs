using System.Text.Json.Serialization;

namespace QuickNV.Driver.Protocol.QpCommands;

[JsonSerializable(typeof(CreateChannelLiveStream.Request))]
[JsonSerializable(typeof(CreateChannelLiveStream.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_CreateChannelLiveStreamCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(ChangeLiveStreamSSRC.Request))]
[JsonSerializable(typeof(ChangeLiveStreamSSRC.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_ChangeLiveStreamSSRCCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(CreateChannelPlaybackStream.Request))]
[JsonSerializable(typeof(CreateChannelPlaybackStream.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_CreateChannelPlaybackStreamCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(DestoryChannelStream.Request))]
[JsonSerializable(typeof(DestoryChannelStream.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_DestoryChannelStreamCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(FindPlaybackFiles.Request))]
[JsonSerializable(typeof(FindPlaybackFiles.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_FindPlaybackFilesCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetChannelConfig.Request))]
[JsonSerializable(typeof(GetChannelConfig.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_GetChannelConfigCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetDeviceConfig.Request))]
[JsonSerializable(typeof(GetDeviceConfig.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_GetDeviceConfigCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(GetMediaServerStreamInfo.Request))]
[JsonSerializable(typeof(GetMediaServerStreamInfo.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_GetMediaServerStreamInfoCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(ImportChannels.Request))]
[JsonSerializable(typeof(ImportChannels.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_ImportChannelsCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(ImportDevices.Request))]
[JsonSerializable(typeof(ImportDevices.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_ImportDevicesCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(MediaServerAddStreamProxy.Request))]
[JsonSerializable(typeof(MediaServerAddStreamProxy.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_MediaServerAddStreamProxyCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(PtzControl.Request))]
[JsonSerializable(typeof(PtzControl.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_PtzControlCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Register.Request))]
[JsonSerializable(typeof(Register.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_RegisterCommandSerializerContext : JsonSerializerContext { }

[JsonSerializable(typeof(Snapshot.Request))]
[JsonSerializable(typeof(Snapshot.Response))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class Driver_SnapshotCommandSerializerContext : JsonSerializerContext { }
