using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace QuickNV.Driver.GB28181.Command.RecordInfo
{
    [Serializable]
    public class RecordListInfo
    {
        [XmlElement]
        public RecordItem[] Item { get; set; }
    }
}
