using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Driver.Protocol.QpCommands.Register
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => RegisterCommandSerializerContext.Default.Response;
        /// <summary>
        /// 属于当前驱动的设备列表
        /// </summary>
        public QpModels.DeviceInfo[] Devices { get; set; }
        /// <summary>
        /// 属于当前驱动的通道列表
        /// </summary>
        public QpModels.ChannelInfo[] Channels { get; set; }
    }
}
