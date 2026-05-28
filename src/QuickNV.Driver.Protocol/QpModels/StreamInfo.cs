using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Driver.Protocol.QpModels
{
    public class StreamInfo
    {
        public string MediaServerId { get; set; }
        public string App { get; set; }
        public string Stream { get; set; }
        public string StreamProxyKey { get; set; }
    }
}
