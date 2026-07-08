using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QuickNV.Protocol.Driver.QpModels
{
    public class ImageParameter
    {
        public int MaxWidth { get; set; }
        public int MaxHeight { get; set; }
        public ImageFormat Format { get; set; }
    }
}
