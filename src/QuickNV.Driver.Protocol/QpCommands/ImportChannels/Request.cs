using Quick.Fields;
using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpCommands.ImportChannels
{
    [DisplayName("导入通道")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => ImportChannelsCommandSerializerContext.Default.Request;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 改变字段的完整编号
        /// </summary>
        public string[] FieldIds { get; set; }
        /// <summary>
        /// 全部字段
        /// </summary>
        public FieldForPost[] Fields { get; set; }
    }
}
