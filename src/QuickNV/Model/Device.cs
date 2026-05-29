using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using QuickNV.Core;
using QuickNV.Protocol.Driver.QpModels;
using System.Text.Json.Serialization;
using YiQiDong.Core.JsonConverters;

namespace QuickNV.Model
{
    /// <summary>
    /// 视频设备
    /// </summary>
    public class Device : DeviceInfo, IHasDependcyRelation
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Key]
        [MaxLength(100)]
        public override string Id { get; set; }
        /// <summary>
        /// 地点编号
        /// </summary>
        public string AddressId { get; set; }
        /// <summary>
        /// 通道数量
        /// </summary>
        [JsonConverter(typeof(JsonInt32Converter))]
        public int ChannelsCount { get; set; }
        /// <summary>
        /// 启用状态
        /// </summary>
        [JsonConverter(typeof(JsonBoolConverter))]
        public bool Enable { get; set; } = true;

        /// <summary>
        /// 设备是否在线
        /// </summary>
        [NotMapped]
        public bool IsOnline { get; set; } = false;

        [JsonIgnore]
        [NotMapped]
        public WithLogContext LogContext { get; set; } = WithLogContext.CreateLogContext();
        /// <summary>
        /// 更新通道数量
        /// </summary>
        public void UpdateChannelsCount()
        {
            var currentCount = ConfigDbContext.CacheContext
                .Query<Channel>(t => t.DeviceId == Id).Length;
            if (currentCount != ChannelsCount)
            {
                ChannelsCount = currentCount;
                ConfigDbContext.CacheContext.Update(this);
            }
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(
                t => t.Id);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj,
                t => t.Id);
        }

        public override string ToString()
        {
            return $"设备[编号:{Id},名称:{Name}]";
        }

        public Device() { }
        public Device(string deviceId) { Id = deviceId; }

        public ModelDependcyInfo[] GetDependcyRelation()
        {
            return new ModelDependcyInfo[]
            {
                new ModelDependcyInfo<Device,Address>(
                    source => new Address() { Id=source.AddressId },
                    source => target=> source.Id == target.AddressId
                    )
            };
        }

        public void PushLog(string message)
        {
            LogContext.PushLog(message);
        }

        public Channel[] GetChannels()
        {
            return ConfigDbContext.CacheContext
                .Query<Channel>(t => t.DeviceId == Id)
                .OrderBy(t => t.Id)
                .ToArray();
        }

        public Channel GetChannel(string channelId)
        {
            return ConfigDbContext.CacheContext.Find(new Channel(Id, channelId));
        }
        public DriverContext GetDriverContext() => DriverManager.Instance.GetDriverContext(DriverId);
        public string GetDriverName() => GetDriverContext()?.DriverInfo?.Name ?? DriverId;
    }
}
