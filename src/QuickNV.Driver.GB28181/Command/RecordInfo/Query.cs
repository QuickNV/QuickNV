using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.RecordInfo
{
    [Serializable]
    [XmlRoot]
    public class Query : AbstractCommandModel
    {
        /// <summary>
        /// 目录设备/视频监控联网系统/区域编码(必选)
        /// </summary>
        [XmlElement]
        public string DeviceID { get; set; }
        /// <summary>
        /// 录像起始时间(必选)
        /// </summary>
        [XmlElement]
        public DateTime StartTime { get; set; }
        /// <summary>
        /// 录像终止时间(必选)
        /// </summary>
        [XmlElement]
        public DateTime EndTime { get; set; }
        /// <summary>
        /// 文件路径名 (可选)
        /// </summary>
        [XmlElement]
        public string FilePath { get; set; }
        /// <summary>
        /// 录像地址(可选 支持不完全查询)
        /// </summary>
        [XmlElement]
        public string Address { get; set; }
        /// <summary>
        /// 保密属性(可选)缺省为0;0:不涉密,1:涉密
        /// </summary>
        //[XmlElement]
        //public int Secrecy { get; set; } = 0;
        /// <summary>
        /// 录像产生类型(可选)time或alarm 或 manual或all
        /// </summary>
        [XmlElement]
        public string Type { get; set; } = "time";
        /// <summary>
        /// 录像触发者ID(可选)
        /// </summary>
        [XmlElement]
        public string RecorderID { get; set; }
        /// <summary>
        /// 录像模糊查询属性(可选)缺省为0;0:不进行模糊查询,此时根据 SIP 消息中 To头域
        /// URI中的ID值确定查询录像位置,若ID值为本域系统ID 则进行中心历史记录检索, 若为前
        /// 端设备ID则进行前端设备历史记录检索;1:进行模糊查询,此时设备所在域应同时进行中心
        /// 检索和前端检索并将结果统一返回。
        /// </summary>
        //[XmlElement]
        //public string IndistinctQuery { get; set; } = "0";

        public Query()
        {
            CmdType = nameof(RecordInfo);
        }
    }
}
