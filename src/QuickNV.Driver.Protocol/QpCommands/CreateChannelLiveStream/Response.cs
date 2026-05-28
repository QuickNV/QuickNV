using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.CreateChannelLiveStream
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => CreateChannelLiveStreamCommandSerializerContext.Default.Response;
        /// <summary>
        /// 流信息
        /// </summary>
        public StreamInfo LiveStreamInfo { get; set; }
    }
}