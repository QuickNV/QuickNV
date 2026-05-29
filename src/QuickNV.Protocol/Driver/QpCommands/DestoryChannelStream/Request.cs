using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpCommands.DestoryChannelStream
{
    [DisplayName("销毁通道的流")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_DestoryChannelStreamCommandSerializerContext.Default.Request;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 通道编号
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 媒体编号
        /// </summary>
        public int MediaId { get; set; }
    }
}
