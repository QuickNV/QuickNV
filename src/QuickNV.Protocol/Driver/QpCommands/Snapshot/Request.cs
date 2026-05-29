using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Protocol.Driver.QpModels;

namespace QuickNV.Protocol.Driver.QpCommands.Snapshot
{
    [DisplayName("快照")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_SnapshotCommandSerializerContext.Default.Request;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 通道编号
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 快照参数
        /// </summary>
        public ImageParameter Parameter { get; set; }
    }
}
