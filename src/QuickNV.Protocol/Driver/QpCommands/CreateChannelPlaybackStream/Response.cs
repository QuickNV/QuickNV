using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.CreateChannelPlaybackStream;

public class Response : AbstractQpSerializer<Response>
{
    protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_CreateChannelPlaybackStreamCommandSerializerContext.Default.Response;
    /// <summary>
    /// 流信息
    /// </summary>
    public StreamInfo PlaybackStreamInfo { get; set; }
}