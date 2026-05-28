using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.Catalog
{
    /// <summary>
    /// 设备列表
    /// </summary>
    [Serializable]
    public class AllChannelsInfo
    {
        /// <summary>
        /// 设备项
        /// </summary>
        [XmlElement("Item")]
        public List<Channel> Items { get; set; }
    }
}
