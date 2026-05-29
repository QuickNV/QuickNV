using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;

namespace QuickNV.Driver.Protocol.QpCommands.FindPlaybackFiles;

[DisplayName("查询录像回放文件")]
public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
{
    protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_FindPlaybackFilesCommandSerializerContext.Default.Request;
    /// <summary>
    /// 设备编号
    /// </summary>
    public string DeviceId { get; set; }

    /// <summary>
    /// 通道编号
    /// </summary>
    public string ChannelId { get; set; }

    /// <summary>
    /// 起始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }
}