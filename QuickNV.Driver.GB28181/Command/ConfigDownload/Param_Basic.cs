using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.ConfigDownload
{
    public class Param_Basic
    {
        /// <summary>
        /// 设备名称
        /// </summary>
        [XmlElement]
        public string Name { get; set; }
        /// <summary>
        /// 注册过期时间
        /// </summary>
        [XmlElement]
        public string Expiration { get; set; }
        /// <summary>
        /// 心跳间隔时间
        /// </summary>
        [XmlElement]
        public int HeartBeatInterval { get; set; }
        /// <summary>
        /// 心跳超时次数
        /// </summary>
        [XmlElement]
        public int HeartBeatCount { get; set; }
        /// <summary>
        /// 定位功能支持情况。取值:0-不支持;1-支持 GPS定位;2-支持北斗定位(可选, 默认取值为0)
        /// </summary>
        [XmlElement]
        public int PositionCapability { get; set; }
        /// <summary>
        /// 经度(可选)
        /// </summary>
        [XmlElement]
        public double Longitude { get; set; }
        /// <summary>
        /// 纬度(可选)
        /// </summary>
        [XmlElement]
        public double Latitude { get; set; }
    }
}
