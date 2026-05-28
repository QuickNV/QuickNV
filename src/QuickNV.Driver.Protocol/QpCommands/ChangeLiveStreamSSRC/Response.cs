using Quick.Protocol;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization.Metadata;
using System.Threading.Tasks;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.ChangeLiveStreamSSRC
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => ChangeLiveStreamSSRCCommandSerializerContext.Default.Response;
        /// <summary>
        /// 媒体信息
        /// </summary>
        public MediaInfo MediaInfo { get; set; }
    }
}
