using Quick.Fields.AppSettings;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Protocol.Driver.QpModels
{
    public class MediaInfo
    {
        /// <summary>
        /// 媒体编号
        /// </summary>
        public int MediaId { get; set; }
        /// <summary>
        /// SSRC
        /// </summary>
        public string SSRC { get; set; }
        /// <summary>
        /// StreamId
        /// </summary>
        public string StreamId { get; set; }
        /// <summary>
        /// 设备
        /// </summary>
        public DeviceInfo Device { get; set; }
        /// <summary>
        /// 通道
        /// </summary>
        public ChannelInfo Channel { get; set; }
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }
        /// <summary>
        /// 鉴权时间
        /// </summary>
        public DateTime? PublishTime { get; set; }
        /// <summary>
        /// 流注册时间
        /// </summary>
        public DateTime? StreamRegistTime { get; set; }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.Append("媒体信息[");
            sb.Append("MediaId: ");
            sb.Append(MediaId);
            sb.Append(", SSRC: ");
            sb.Append(SSRC);
            sb.Append(", StreamId: ");
            sb.Append(StreamId);
            sb.Append(", Channel: ");
            sb.Append(Channel.Name);
            if (CreateTime.HasValue)
            {
                sb.Append(", CreateTime: ");
                sb.Append(CreateTime);
            }
            if (PublishTime.HasValue)
            {
                sb.Append(", PublishTime: ");
                sb.Append(PublishTime);
            }
            if (StreamRegistTime.HasValue)
            {
                sb.Append(", StreamRegistTime: ");
                sb.Append(StreamRegistTime);
            }
            sb.Append("]");
            var ret = sb.ToString();
            sb.Clear();
            return ret;
        }
    }
}
