using Quick.Fields;
using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.ImportChannels
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => Driver_ImportChannelsCommandSerializerContext.Default.Response;
        /// <summary>
        /// 要导入的设备
        /// </summary>
        public ChannelInfo[] Channels { get; set; }
        public FieldForGet[] Fields { get; set; }
    }
}
