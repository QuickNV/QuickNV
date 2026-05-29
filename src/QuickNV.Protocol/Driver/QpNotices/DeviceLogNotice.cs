using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.Driver.QpNotices
{
    [DisplayName("设备日志通知")]
    public class DeviceLogNotice : AbstractQpSerializer<DeviceLogNotice>
    {
        protected override JsonTypeInfo<DeviceLogNotice> GetTypeInfo() => Driver_NoticesSerializerContext.Default.DeviceLogNotice;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 消息内容
        /// </summary>
        public string Message { get; set; }
    }
}
