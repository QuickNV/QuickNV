using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.ConfigDownload
{
    /// <summary>
    /// 设备信息查询结果信息
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class Response : AbstractCommandModel
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        [XmlElement]
        public string DeviceID { get; set; }
        /// <summary>
        /// 结果
        /// </summary>
        [XmlElement]
        public string Result { get; set; }
        /// <summary>
        /// 基本参数
        /// </summary>
        [XmlElement]
        public Param_Basic BasicParam { get; set; }        
    }
}
