using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.DeviceInfo
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
        /// 厂商
        /// </summary>
        [XmlElement]
        public string Manufacturer { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        [XmlElement]
        public string Model { get; set; }
        /// <summary>
        /// 固件
        /// </summary>
        [XmlElement]
        public string Firmware { get; set; }
        /// <summary>
        /// 扩展信息
        /// </summary>
        [XmlElement]
        public string Info { get; set; }
    }
}
