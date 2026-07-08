using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.Keepalive
{
    /// <summary>
    /// 设备报送信息通知消息
    /// </summary>
    [Serializable]
    [XmlRoot]
    public class Notify : AbstractCommandModel
    {
        /// <summary>
        /// 设备编码
        /// </summary>
        [XmlElement]
        public string DeviceID { get; set; }
        /// <summary>
        /// 状态
        /// </summary>
        [XmlElement]
        public string Status { get; set; }

        public Notify()
        {
            CmdType = nameof(Keepalive);
        }
    }
}
