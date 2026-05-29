using Quick.Protocol;
using System.ComponentModel;
using System.Text.Json.Serialization.Metadata;

namespace QuickNV.Protocol.North.QpCommands.Register
{
    [DisplayName("注册北向")]
    public class Request : AbstractQpSerializer<Request>, IQpCommandRequest<Request, Response>
    {
        protected override JsonTypeInfo<Request> GetTypeInfo() => North_RegisterCommandSerializerContext.Default.Request;
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }
}
