using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.ConfigDownload
{
    [Serializable]
    [XmlRoot]
    public class Query : AbstractCommandModel
    {
        /// <summary>
        /// 系统编码/行政区划码/设备编码/业务分组编码/虚拟组织编码
        /// </summary>
        [XmlElement]
        public string DeviceID { get; set; }
        /// <summary>
        /// 查询配置参数类型(必选),可查询的配置类型包括：
        ///     基本参数配置:BasicParam,
        ///     视频参数范围:VideoParamOpt,
        ///     SVAC 编码配置:SVACEncodeConfig,
        ///     SVAC 解码配置:SVACDecodeConfig。
        /// 可同时查询多个配置类型,各类型以“/”分隔,可返回与查询SN 值相同的多个响 应,每个响应对应一个配置类型。
        /// </summary>
        [XmlElement]
        public string ConfigType { get; set; } = "BasicParam";

        public Query()
        {
            CmdType = nameof(ConfigDownload);
        }
    }
}
