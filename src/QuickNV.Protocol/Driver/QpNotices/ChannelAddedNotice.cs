using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpNotices
{
    [DisplayName("通道添加通知")]
    public class ChannelAddedNotice : AbstractQpSerializer<ChannelAddedNotice>
    {
        protected override JsonTypeInfo<ChannelAddedNotice> GetTypeInfo() => Driver_NoticesSerializerContext.Default.ChannelAddedNotice;
        public QpModels.ChannelInfo Channel { get; set; }
    }
}
