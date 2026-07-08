using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.Driver.QpCommands.ChangeLiveStreamSSRC
{
    [DisplayName("修改实时流SSRC")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_ChangeLiveStreamSSRCCommandSerializerContext.Default.Request;

        /// <summary>
        /// 媒体服务器编号
        /// </summary>
        public string MediaServerId { get; set; }
        /// <summary>
        /// 媒体编号
        /// </summary>
        public int MediaId{ get; set; }
        /// <summary>
        /// 要改变到的SSRC
        /// </summary>
        public string SSRC { get; set; }
    }
}
