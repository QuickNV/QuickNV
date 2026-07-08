using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Driver.GB28181.Model
{
    public class GbStreamInfo
    {
        public string DeviceId { get; set; }
        public string ChannelId { get; set; }
        public string ToTag { get; set; }
        public string CallId { get; set; }
        public DateTime CreateTime { get; set; }
    }
}
