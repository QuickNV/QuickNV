using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.Driver.QpNotices
{
    [DisplayName("通道删除通知")]
    public class ChannelDeletedNotice : AbstractQpSerializer<ChannelDeletedNotice>
    {
        protected override JsonTypeInfo<ChannelDeletedNotice> GetTypeInfo() => Driver_NoticesSerializerContext.Default.ChannelDeletedNotice;
        public QpModels.ChannelInfo Channel { get; set; }
    }
}
