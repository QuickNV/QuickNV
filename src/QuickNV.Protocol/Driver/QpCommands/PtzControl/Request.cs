using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.PtzControl
{
    [DisplayName("云台控制")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => Driver_PtzControlCommandSerializerContext.Default.Request;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 通道编号
        /// </summary>
        public string ChannelId { get; set; }
        /// <summary>
        /// 命令类型
        /// </summary>
        public PTZCommandType CommandType { get; set; }
        /// <summary>
        /// 移动速度。范围：0-1
        /// </summary>
        public float MoveSpeed { get; set; }
    }
}
