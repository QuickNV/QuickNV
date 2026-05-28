using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command
{
    public abstract class AbstractCommandModel
    {
        /// <summary>
        /// 命令类型
        /// </summary>
        [XmlElement]
        public string CmdType { get; set; }
        /// <summary>
        /// 命令序列号
        /// </summary>
        [XmlElement]
        public int SN { get; set; } = 1;
    }
}
