using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.North.Protocol.QpCommands.Sync
{
    [DisplayName("同步")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => North_SyncCommandSerializerContext.Default.Request;

        /// <summary>
        /// 地点数据
        /// </summary>
        public QpModels.AddressInfo[] Address { get; set; }

        /// <summary>
        /// 设备数据
        /// </summary>
        public QpModels.DeviceInfo[] Device { get; set; }

        /// <summary>
        /// 通道数据
        /// </summary>
        public QpModels.ChannelInfo[] Channel { get; set; }
    }
}
