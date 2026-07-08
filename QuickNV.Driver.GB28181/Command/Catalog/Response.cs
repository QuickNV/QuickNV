using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.Catalog
{
    /// <summary>
    /// 设备目录查询结果信息
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class Response : AbstractCommandModel
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        [XmlElement()]
        public string DeviceID { get; set; }

        /// <summary>
        /// 设备总条数
        /// </summary>
        [XmlElement]
        public int SumNum { get; set; }

        /// <summary>
        /// 列表显示条数
        /// </summary>
        [XmlElement("DeviceList")]
        public AllChannelsInfo AllChannelsInfo { get; set; }
    }
}
