using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.CreateChannelLiveStream
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_CreateChannelLiveStreamCommandSerializerContext.Default.Response;
        /// <summary>
        /// 流信息
        /// </summary>
        public StreamInfo LiveStreamInfo { get; set; }
    }
}