using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.ChangeLiveStreamSSRC
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_ChangeLiveStreamSSRCCommandSerializerContext.Default.Response;
        /// <summary>
        /// 媒体信息
        /// </summary>
        public MediaInfo MediaInfo { get; set; }
    }
}
