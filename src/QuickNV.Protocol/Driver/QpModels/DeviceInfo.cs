using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Driver.Protocol.QpModels
{
    public class DeviceInfo
    {
        /// <summary>
        /// 设备编号
        /// </summary>
        public virtual string Id { get; set; }
        /// <summary>
        /// 驱动编号
        /// </summary>
        public virtual string DriverId { get; set; }
        /// <summary>
        /// 驱动配置
        /// </summary>
        public virtual string DriverConfig { get; set; }
        /// <summary>
        /// 设备名称
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
        /// 厂商
        /// </summary>
        public virtual string Manufacturer { get; set; }
        /// <summary>
        /// 型号
        /// </summary>
        public virtual string Model { get; set; }
        /// <summary>
        /// 序列号
        /// </summary>
        public virtual string SerialNumber { get; set; }
        /// <summary>
        /// 固件版本
        /// </summary>
        public string FirmwareVersion { get; set; }
    }
}
