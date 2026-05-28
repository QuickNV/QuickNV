using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.Catalog
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
        /// 报警开始时间
        /// </summary>
        [XmlElement]
        public string StartAlarmPriority { get; set; }

        /// <summary>
        /// 报警结束时间
        /// </summary>
        [XmlElement]
        public string EndAlarmPriority { get; set; }

        /// <summary>
        /// 报警方法
        /// </summary>
        [XmlElement]
        public string AlarmMethod { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [XmlElement]
        public string StartTime { get; set; }

        /// <summary>
        /// 结束时间
        /// </summary>
        [XmlElement]
        public string EndTime { get; set; }

        public Query()
        {
            CmdType = nameof(Catalog);
        }
    }
}
