using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpNotices
{
    [DisplayName("设备在线通知")]
    public class DeviceOnlineNotice : AbstractQpSerializer<DeviceOnlineNotice>
    {
        protected override JsonTypeInfo<DeviceOnlineNotice> GetTypeInfo() => Driver_NoticesSerializerContext.Default.DeviceOnlineNotice;
        /// <summary>
        /// 设备编号
        /// </summary>
        public DeviceInfo Device { get; set; }

        public DeviceOnlineNotice() { }
        public DeviceOnlineNotice(DeviceInfo device)
        {
            Device = device;
        }
    }
}
