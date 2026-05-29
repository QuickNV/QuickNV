using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.ChangeLiveStreamSSRC
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
