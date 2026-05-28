using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.RecordInfo
{
    [Serializable]
    public class RecordItem
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
        /// 文件路径名 (可选)
        /// </summary>
        [XmlElement]
        public string FilePath { get; set; }
        /// <summary>
        /// 录像地址(可选)
        /// </summary>
        [XmlElement]
        public string Address { get; set; }
        /// <summary>
        /// 录像开始时间(可选)
        /// </summary>
        [XmlElement]
        public string StartTime { get; set; }
        [XmlIgnore]
        public DateTime DT_StartTime => DateTime.Parse(StartTime);
        /// <summary>
        /// 录像结束时间(可选)
        /// </summary>
        [XmlElement]
        public string EndTime { get; set; }
        [XmlIgnore]
        public DateTime DT_EndTime => DateTime.Parse(EndTime);
        /// <summary>
        /// 保密属性(必选)缺省为0;0:不涉密,1:涉密-
        /// </summary>
        [XmlElement]
        public int? Secrecy { get; set; }
        /// <summary>
        /// 录像产生类型(可选)time或alarm 或 manual
        /// </summary>
        [XmlElement]
        public string Type { get; set; }
        /// <summary>
        /// 录像触发者ID(可选)
        /// </summary>
        [XmlElement]
        public string RecorderID { get; set; }
        /// <summary>
        /// 录像文件大小,单位:Byte(可选)
        /// </summary>
        [XmlElement]
        public long FileSize { get; set; }
    }
}
