using System.Xml.Serialization;
using static System.Runtime.CompilerServices.RuntimeHelpers;

namespace QuickNV.Driver.GB28181.Command.Catalog
{
    /// <summary>
    /// 扩展信息
    /// </summary>
    [Serializable]
    public class ChannelExtra
    {
        /// <summary>
        /// 摄像机类型扩展，标识摄像机类型
        /// 1，球机
        /// 2，半球
        /// 3，固定枪机
        /// 4，遥控枪机
        /// 当目录项为摄像机时可选
        /// </summary>
        [XmlIgnore]
        public int? PTZType { get; set; }
        [XmlElement(nameof(PTZType))]
        public string PTZTypeAsText
        {
            get { return PTZType?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    PTZType = numberValue;
                else
                    PTZType = null;
            }
        }

        /// <summary>
        /// 摄像机位置类型扩展
        /// 1，省际检查站
        /// 2，党政机关
        /// 3，车站码头
        /// 4，中心广场
        /// 5，体育场馆
        /// 6，商业中心
        /// 7，宗教场所
        /// 8，校园周边
        /// 9，治安复杂区域
        /// 10，交通干线
        /// 当目录项为摄像机时可选
        /// </summary>
        [XmlIgnore]
        public int? PositionType { get; set; }
        [XmlElement(nameof(PositionType))]
        public string PositionTypeAsText
        {
            get { return PositionType?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    PositionType = numberValue;
                else
                    PositionType = null;
            }
        }

        /// <summary>
        /// 摄像机按照位置室外、室内属性
        /// 1，室外
        /// 2，室内
        /// 当目录项为摄像机时可选，缺省为1
        /// </summary>
        [XmlIgnore]
        public int? RoomType { get; set; }
        [XmlElement(nameof(RoomType))]
        public string RoomTypeAsText
        {
            get { return RoomType?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    RoomType = numberValue;
                else
                    RoomType = null;
            }
        }

        /// <summary>
        /// 摄像机用途属性
        /// 1，治安
        /// 2，交通
        /// 3，重点
        /// 当目录项为摄像机时可选
        /// </summary>
        [XmlIgnore]
        public int? UseType { get; set; }
        [XmlElement(nameof(UseType))]
        public string UseTypeAsText
        {
            get { return UseType?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    UseType = numberValue;
                else
                    UseType = null;
            }
        }

        /// <summary>
        /// 摄像机补光属性
        /// 1，无补光
        /// 2，红外补光
        /// 3，白光补光
        /// 当目录项为摄像机时可选，缺省为1
        /// </summary>
        [XmlIgnore]
        public int? SupplyLightType { get; set; }
        [XmlElement(nameof(SupplyLightType))]
        public string SupplyLightTypeAsText
        {
            get { return SupplyLightType?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    SupplyLightType = numberValue;
                else
                    SupplyLightType = null;
            }
        }

        /// <summary>
        /// 摄像机监视方位属性
        /// 1，东
        /// 2，西
        /// 3，南
        /// 4，北
        /// 5，东南
        /// 6，东北
        /// 7，西南
        /// 8，西北
        /// 当目录项为摄像机时且为固定摄像机或设置看守位摄像机时可选
        /// </summary>
        [XmlIgnore]
        public int? DirectionType { get; set; }
        [XmlElement(nameof(DirectionType))]
        public string DirectionTypeAsText
        {
            get { return DirectionType?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    DirectionType = numberValue;
                else
                    DirectionType = null;
            }
        }

        /// <summary>
        /// 摄像机支持的分辨率，可有多个分辨率值，各个取值间以"/"分隔。
        /// 分辨率取值参见附录F中SDP f字段规定。
        /// 当目录项为摄像机时可选
        /// </summary>
        [XmlElement()]
        public string Resolution { get; set; }

        /// <summary>
        /// 虚拟组织所属的业务分组ID，
        /// 业务分组根据特定的业务需求制定，
        /// 一个业务分组包含一组特定的虚拟组织。
        /// </summary>
        [XmlElement()]
        public string BusinessGroupID { get; set; }

        /// <summary>
        /// 下载倍速范围(可选)，各可选参数以"/"分隔
        /// 如设备支持1,2,4倍下载则应写为"1/2/4"
        /// </summary>
        [XmlElement()]
        public string DownloadSpeed { get; set; }

        /// <summary>
        /// 空域编码能力
        /// 0，不支持
        /// 1，1级增强
        /// 2，2级增强
        /// 3，3级增强
        /// (可选)
        /// </summary>
        [XmlIgnore]
        public int? SVCSpaceSupportMode { get; set; }
        [XmlElement(nameof(SVCSpaceSupportMode))]
        public string SVCSpaceSupportModeAsText
        {
            get { return SVCSpaceSupportMode?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    SVCSpaceSupportMode = numberValue;
                else
                    SVCSpaceSupportMode = null;
            }
        }

        /// <summary>
        /// 时域编码能力
        /// 0，不支持
        /// 1，1级增强
        /// 2，2级增强
        /// 3，3级增强
        /// </summary>
        [XmlIgnore]
        public int? SVCTimeSupportMode { get; set; }
        [XmlElement(nameof(SVCTimeSupportMode))]
        public string SVCTimeSupportModeAsText
        {
            get { return SVCTimeSupportMode?.ToString(); }
            set
            {
                if (int.TryParse(value, out var numberValue))
                    SVCTimeSupportMode = numberValue;
                else
                    SVCTimeSupportMode = null;
            }
        }
    }
}
