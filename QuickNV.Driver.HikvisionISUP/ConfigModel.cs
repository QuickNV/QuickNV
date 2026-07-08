using QuickNV.HikvisionISUPSDK.Api;
using System.Text.Json.Serialization;
using YiQiDong.Core.JsonConverters;
using QuickNV.Driver.Agent;

namespace QuickNV.Driver.HikvisionISUP
{
    [JsonSerializable(typeof(ConfigModel))]
    [JsonSourceGenerationOptions(DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
    internal partial class ConfigModelSerializerContext : JsonSerializerContext { }

    public class ConfigModel : AbstractDriverConfigModel
    {
        public string CmsListenIPAddress { get; set; } = "0.0.0.0";
        [JsonConverter(typeof(JsonInt32Converter))]
        public int CmsListenPort { get; set; } = 7660;
        public string CmsEncoding { get; set; } = "UTF-8";

        [JsonConverter(typeof(JsonStringEnumConverter<CmsAccessSecurity>))]
        public CmsAccessSecurity CmsAccessSecurity { get; set; } = CmsAccessSecurity.CompatibleMode;
        public string CmsPublicIPAddress { get; set; } = "192.168.31.72";
        [JsonConverter(typeof(JsonInt32Converter))]
        public int CmsPublicPort { get; set; } = 7660;
        public string CmsPassword { get; set; } = "123456";

        public string SmsListenIPAddress { get; set; } = "0.0.0.0";
        public string SmsPublicIPAddress { get; set; } = "192.168.31.72";
        [JsonConverter(typeof(JsonInt32Converter))]
        public int SmsListenPort { get; set; } = 7760;
        [JsonConverter(typeof(JsonStringEnumConverter<SmsLinkMode>))]
        public SmsLinkMode SmsLinkMode { get; set; } = SmsLinkMode.TCP;
    }
}
