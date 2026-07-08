using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Protocol.Driver.QpModels
{
    public class DeviceAndChannelsInfo
    {
        public DeviceInfo Device { get; set; }
        public ChannelInfo[] Channels { get; set; }
    }
}
