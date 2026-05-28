using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpNotices
{
    [DisplayName("设备离线通知")]
    public class DeviceOfflineNotice : AbstractQpSerializer<DeviceOfflineNotice>
    {
        protected override JsonTypeInfo<DeviceOfflineNotice> GetTypeInfo() => NoticesSerializerContext.Default.DeviceOfflineNotice;
        /// <summary>
        /// 设备编号
        /// </summary>
        public string DeviceId { get; set; }
        /// <summary>
        /// 离线原因
        /// </summary>
        public string Reason { get; set; }
    }
}
