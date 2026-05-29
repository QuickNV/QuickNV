using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Protocol.Driver.QpModels
{
    public class ChannelInfo
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public virtual string DeviceId { get; set; }
        /// <summary>
        /// 通道编号
        /// </summary>
        public virtual string Id { get; set; }
        /// <summary>
        /// 通道名称
        /// </summary>
        public virtual string Name { get; set; }
        /// <summary>
        /// 经度
        /// </summary>
        public double? Lng { get; set; }
        /// <summary>
        /// 纬度
        /// </summary>
        public double? Lat { get; set; }
        /// <summary>
        /// 驱动配置
        /// </summary>
        public virtual string DriverConfig { get; set; }
    }
}
