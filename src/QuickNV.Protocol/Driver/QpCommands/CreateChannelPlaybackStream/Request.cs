using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using Quick.Protocol;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.CreateChannelPlaybackStream;

[DisplayName("获取通道实时流")]
public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
{
    protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_CreateChannelPlaybackStreamCommandSerializerContext.Default.Request;
    /// <summary>
    /// 媒体服务器信息
    /// </summary>
    public MediaServerInfo MediaServerInfo { get; set; }

    /// <summary>
    /// 媒体信息
    /// </summary>
    public MediaInfo MediaInfo { get; set; }
    /// <summary>
    /// 开始时间
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// 结束时间
    /// </summary>
    public DateTime EndTime { get; set; }

}