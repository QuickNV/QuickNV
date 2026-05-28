using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.RecordInfo
{
    [Serializable]
    [XmlRoot]
    public class Response : AbstractCommandModel
    {
        /// <summary>
        /// 设备/区域编码(必选)
        /// </summary>
        [XmlElement]
        public string DeviceID { get; set; }
        /// <summary>
        /// 设备/区域名称(必选)
        /// </summary>
        [XmlElement]
        public string Name { get; set; }
        /// <summary>
        /// 查询结果总数(必选)
        /// </summary>
        [XmlElement]
        public int SumNum { get; set; }
        /// <summary>
        /// 文件目录项列表
        /// </summary>
        [XmlElement]
        public RecordListInfo RecordList { get; set; }
    }
}
