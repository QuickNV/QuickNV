using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel.DataAnnotations.Schema;
using QuickNV.Core;
using QuickNV.Protocol.Driver.QpModels;
using System.Text.Json.Serialization;
using Microsoft.EntityFrameworkCore;

namespace QuickNV.Model
{
    /// <summary>
    /// 视频通道
    /// </summary>
    [Comment("视频通道")]
    public class Channel : ChannelInfo, IHasDependcyRelation
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Comment("编号")]
        public override string Id { get; set; }
        /// <summary>
        /// 外部编号
        /// </summary>
        [Comment("外部编号")]
        public string ExternalId{get;set;}
        /// <summary>
        /// 地点编号
        /// </summary>
        [Comment("地点编号")]
        public string AddressId { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        [Comment("名称")]
        public override string Name { get; set; }
        /// <summary>
        /// 驱动配置
        /// </summary>
        [Comment("驱动配置")]
        public override string DriverConfig { get; set; }

        [JsonIgnore]
        [NotMapped]
        public string AddressName => ConfigDbContext.CacheContext.Find(new Model.Address() { Id = AddressId })?.Name;

        [JsonIgnore]
        [NotMapped]
        public StreamInfo LiveStreamInfo { get; set; }
        
        [JsonIgnore]
        [NotMapped]
        public StreamInfo PlaybackStreamInfo { get; set; }

        [JsonIgnore]
        [NotMapped]
        public WithLogContext LogContext { get; private set; } = WithLogContext.CreateLogContext();

        public ModelDependcyInfo[] GetDependcyRelation()
        {
            return new ModelDependcyInfo[]
            {
                new ModelDependcyInfo<Channel,Device>(
                    source => new Device() { Id=source.DeviceId },
                    source => target=> source.Id == target.DeviceId
                    ),
                new ModelDependcyInfo<Channel,Address>(
                    source => new Address() { Id=source.AddressId },
                    source => target=> source.Id == target.AddressId
                    )
            };
        }

        public override int GetHashCode()
        {
            return this.GetHashCode(
                t => t.DeviceId,
                t => t.Id);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj,
                t => t.DeviceId,
                t => t.Id);
        }

        public override string ToString()
        {
            return $"通道[设备编号:{DeviceId},编号:{Id},名称:{Name}]";
        }

        public void PushLog(string message)
        {
            LogContext.PushLog(message);
        }

        public Channel() { }
        public Channel(string deviceId, string id)
        {
            DeviceId = deviceId;
            Id = id;
        }

        public string GetSnapshotUrl()
        {
            return $"api/channel/snapshot/{DeviceId}/{Id}";
        }
    }
}
