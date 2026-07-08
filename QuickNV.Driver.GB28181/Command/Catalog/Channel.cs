using static QuickNV.Driver.GB28181.Command.Catalog.Response;
using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.Catalog
{
    [Serializable]
    /// <summary>
    /// 设备信息
    /// </summary>
    public class Channel
    {
        /// <summary>
        /// 设备/区域/系统编码(必选)
        /// </summary>
        [XmlElement("DeviceID")]
        public string ChannelId { get; set; }

        /// <summary>
        /// 设备/区域/系统名称(必选)
        /// </summary>
        [XmlElement]
        public string Name { get; set; }

        /// <summary>
        /// 当为设备时，设备厂商(必选)
        /// </summary>
        [XmlElement]
        public string Manufacturer { get; set; }

        /// <summary>
        /// 当为设备时，设备型号(必选)
        /// </summary>
        [XmlElement]
        public string Model { get; set; }

        /// <summary>
        /// 当为设备时，设备归属(必选)
        /// </summary>
        [XmlElement]
        public string Owner { get; set; }

        /// <summary>
        /// 行政区域(必选)
        /// </summary>
        [XmlElement]
        public string CivilCode { get; set; }

        /// <summary>
        /// 警区(可选)
        /// </summary>
        [XmlElement]
        public string Block { get; set; }

        /// <summary>
        /// 当为设备时，安装地址(必选)
        /// </summary>
        [XmlElement]
        public string Address { get; set; }

        /// <summary>
        /// 当为设备时，是否有子设备(必选)，
        /// 1有
        /// 0没有
        /// </summary>
        [XmlElement]
        public int? Parental { get; set; }

        /// <summary>
        /// 父设备/区域/系统ID(必选)
        /// </summary>
        [XmlElement("ParentID")]
        public string DeviceId { get; set; }

        /// <summary>
        /// 虚拟分组ID
        /// </summary>
        [XmlElement]
        public string BusinessGroupID { get; set; }

        /// <summary>
        /// 信令安全模式(可选)缺省为0； 
        /// 0：不采用
        /// 2：S/MIME签名方式 
        /// 3：S/MIME加密签名同时采用方式 
        /// 4：数字摘要方式
        /// </summary>
        [XmlIgnore]
        public int? SafetyWay { get; set; }
        [XmlElement(nameof(SafetyWay))]
        public string SafetyWayAsText
        {
            get { return SafetyWay?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    SafetyWay = numberValue;
                else
                    SafetyWay = null;
            }
        }

        /// <summary>
        /// 注册方式(必选)缺省为1；
        /// 1:符合IETF FRC 3261标准的认证注册模式；
        /// 2:基于口令的双向认证注册模式；
        /// 3:基于数字证书的双向认证注册模式；
        /// </summary>
        [XmlElement]
        public int? RegisterWay { get; set; }

        /// <summary>
        /// 证书序列号（有证书的设备必选）
        /// </summary>
        [XmlElement]
        public string CertNum { get; set; }

        /// <summary>
        /// 证书有效标志(有证书的设备必选)，
        /// 0无效
        /// 1有效
        /// </summary>
        [XmlElement]
        public int? Certifiable { get; set; }

        /// <summary>
        /// 证书无效原因码(可选)
        /// </summary>
        [XmlIgnore]
        public int? ErrCode { get; set; }
        [XmlElement(nameof(ErrCode))]
        public string ErrCodeAsText
        {
            get { return ErrCode?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    ErrCode = numberValue;
                else
                    ErrCode = null;
            }
        }

        /// <summary>
        /// 证书终止有效期(可选)
        /// </summary>
        [XmlElement]
        public string EndTime { get; set; }

        /// <summary>
        /// 保密属性(必选)
        /// 0：不涉密
        /// 1涉密
        /// </summary>
        [XmlElement]
        public int? Secrecy { get; set; }

        /// <summary>
        /// 设备/区域/系统IP地址（可选）
        /// </summary>
        [XmlElement]
        public string IPAddress { get; set; }

        /// <summary>
        /// 设备/区域/系统端口(可选)
        /// </summary>
        [XmlIgnore]
        public ushort? Port { get; set; }
        [XmlElement(nameof(Port))]
        public string PortAsText
        {
            get { return Port?.ToString(); }
            set
            {
                if (ushort.TryParse(value, out var numberValue))
                    Port = numberValue;
                else
                    Port = null;
            }
        }

        /// <summary>
        /// 设备口令（可选）
        /// </summary>
        [XmlElement]
        public string Password { get; set; }

        /// <summary>
        /// 设备状态(必选)
        /// </summary>
        [XmlElement]
        public string Status { get; set; }

        /// <summary>
        /// 经度(可选)
        /// </summary>
        [XmlIgnore]
        public double? Longitude { get; set; }

        [XmlElement(nameof(Longitude))]
        public string LongitudeAsText
        {
            get { return Longitude?.ToString(); }
            set
            {
                if (double.TryParse(value, out var numberValue))
                    Longitude = numberValue;
                else
                    Longitude = null;
            }
        }
        /// <summary>
        /// 纬度(可选)
        /// </summary>
        [XmlIgnore]
        public double? Latitude { get; set; }
        [XmlElement(nameof(Latitude))]
        public string LatitudeAsText
        {
            get { return Latitude?.ToString(); }
            set
            {
                if (double.TryParse(value, out var numberValue))
                    Latitude = numberValue;
                else
                    Latitude = null;
            }
        }

        /// <summary>
        /// 信息项
        /// </summary>
        [XmlElement("Info")]
        public ChannelExtra Extra { get; set; }
    }
}
