using System.ComponentModel.DataAnnotations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion.Internal;
using Quick.EntityFrameworkCore.Plus;

namespace QuickNV.Model
{
    /// <summary>
    /// ZLMediaKit媒体服务器
    /// </summary>
    public class MediaServer
    {
        /// <summary>
        /// 编号
        /// </summary>
        [Key]
        [MaxLength(100)]
        public string Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
        /// <summary>
        /// API地址
        /// </summary>
        public string ApiUrl { get; set; } = "http://127.0.0.1:8180/index/api";
        /// <summary>
        /// API密码
        /// </summary>
        public string ApiSecret { get; set; } = "035c73f7-bb6b-4889-a715-d9eb2d1925cc";
        /// <summary>
        /// 公开访问IP地址
        /// </summary>
        public string PublicIpAddress { get; set; } = "127.0.0.1";
        /// <summary>
        /// 公开访问URL
        /// </summary>
        public string PublicUrl { get; set; }

        public override int GetHashCode()
        {
            return this.GetHashCode(
                t => t.Id);
        }

        public override bool Equals(object obj)
        {
            return this.Equals(obj,
                t => t.Id);
        }

        public string GetWsUrlForProxy(string path)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "ws";
            uriBuilder.Path = path;
            
            var uri = new Uri(ApiUrl);
            if (uri.Scheme == "https" || uri.Scheme == "wss")
                uriBuilder.Scheme = "wss";
            uriBuilder.Host = uri.Host;
            uriBuilder.Port = uri.Port;           
            return uriBuilder.ToString();
        }

        public string GetWsUrl(string path)
        {
            var uriBuilder = new UriBuilder();
            uriBuilder.Scheme = "ws";
            uriBuilder.Path = path;
            if (string.IsNullOrEmpty(PublicUrl))
            {
                uriBuilder.Host = PublicIpAddress;
                uriBuilder.Port = new Uri(ApiUrl).Port;
            }
            else
            {
                var uri = new Uri(PublicUrl);
                if (uri.Scheme == "https" || uri.Scheme == "wss")
                    uriBuilder.Scheme = "wss";
                uriBuilder.Host = uri.Host;
                uriBuilder.Port = uri.Port;
                if (!string.IsNullOrEmpty(uri.AbsolutePath) && uri.AbsolutePath != "/")
                    uriBuilder.Path = uri.AbsolutePath + path;
            }
            return uriBuilder.ToString();
        }
    }
}
