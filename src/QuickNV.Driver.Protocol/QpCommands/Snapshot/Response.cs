using Quick.Protocol;
using System.Text.Json.Serialization.Metadata;
using QuickNV.Driver.Protocol.QpModels;

namespace QuickNV.Driver.Protocol.QpCommands.Snapshot
{
    public class Response : AbstractQpSerializer<Response>
    {
        protected override JsonTypeInfo<Response> GetTypeInfo() => SnapshotCommandSerializerContext.Default.Response;
        /// <summary>
        /// 图片格式
        /// </summary>
        public ImageFormat Format { get; set; }
        /// <summary>
        /// 图片宽度
        /// </summary>
        public int Width { get; set; }
        /// <summary>
        /// 图片高度
        /// </summary>
        public int Height { get; set; }
        /// <summary>
        /// 图片内容
        /// </summary>
        public byte[] Content { get; set; }
        /// <summary>
        /// MIME类型
        /// </summary>
        public string MimeType { get; set; }
    }
}
