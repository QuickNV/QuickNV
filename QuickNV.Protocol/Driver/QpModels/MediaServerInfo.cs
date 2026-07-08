using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Protocol.Driver.QpModels
{
    public class MediaServerInfo
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string PublicIpAddress { get; set; }
        public int RtpProxyPort { get; set; }
    }
}
