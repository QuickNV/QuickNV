using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.DeviceControl
{
    [Serializable]
    [XmlRoot]
    public class Control : AbstractCommandModel
    {
        /// <summary>
        /// 系统编码/行政区划码/设备编码/业务分组编码/虚拟组织编码
        /// </summary>
        [XmlElement]
        public string DeviceID { get; set; }

        [XmlElement]
        public string PTZCmd { get; set; }
        public Control()
        {
            CmdType = nameof(DeviceControl);
        }
    }
}
