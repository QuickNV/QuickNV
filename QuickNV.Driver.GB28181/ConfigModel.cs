using System.Text.Json.Serialization;
using YiQiDong.Core.JsonConverters;
using QuickNV.Driver.Agent;

namespace QuickNV.Driver.GB28181
{
    [JsonSerializable(typeof(ConfigModel))]
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

    public class ConfigModel : AbstractDriverConfigModel
    {
        /// <summary>
        /// SIP服务IP地址
        /// </summary>
        public string SipServerIpAddress { get; set; } = "192.168.31.72";
        /// <summary>
        /// SIP服务端口
        /// </summary>
        [JsonConverter(typeof(JsonInt32Converter))]
        public int SipServerPort { get; set; } = 5060;
        /// <summary>
        /// SIP设备编号
        /// </summary>
        public string SipDeviceId { get; set; } = "44522200442000000001";
        /// <summary>
        /// SIP服务域编号
        /// </summary>
        public string SipRealm { get; set; } = "4452220044";
        /// <summary>
        /// SIP密码
        /// </summary>
        public string SipPassword { get; set; } = "123456";
        /// <summary>
        /// 字符编码
        /// </summary>
        public string Encoding { get; set; } = "GB18030";
    }
}
