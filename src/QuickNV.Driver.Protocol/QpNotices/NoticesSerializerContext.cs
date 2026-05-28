using System.Text.Json.Serialization;

namespace QuickNV.Driver.Protocol.QpNotices;

[JsonSerializable(typeof(ChannelAddedNotice))]
[JsonSerializable(typeof(ChannelDeletedNotice))]
[JsonSerializable(typeof(DeviceAddedNotice))]
[JsonSerializable(typeof(DeviceDeletedNotice))]
[JsonSerializable(typeof(DeviceLogNotice))]
[JsonSerializable(typeof(DeviceOfflineNotice))]
[JsonSerializable(typeof(DeviceOnlineNotice))]
[JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
internal partial class NoticesSerializerContext : JsonSerializerContext { }