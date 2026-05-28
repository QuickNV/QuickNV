using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpNotices
{
    [DisplayName("设备删除通知")]
    public class DeviceDeletedNotice : AbstractQpSerializer<DeviceDeletedNotice>
    {
        protected override JsonTypeInfo<DeviceDeletedNotice> GetTypeInfo() => NoticesSerializerContext.Default.DeviceDeletedNotice;
        public QpModels.DeviceInfo Device { get; set; }
    }
}
