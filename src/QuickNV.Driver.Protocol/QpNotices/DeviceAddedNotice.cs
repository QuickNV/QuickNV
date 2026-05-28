using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpNotices
{
    [DisplayName("设备添加通知")]
    public class DeviceAddedNotice : AbstractQpSerializer<DeviceAddedNotice>
    {
        protected override JsonTypeInfo<DeviceAddedNotice> GetTypeInfo() => NoticesSerializerContext.Default.DeviceAddedNotice;
        public QpModels.DeviceInfo Device { get; set; }
    }
}
