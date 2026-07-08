using Microsoft.AspNetCore.Mvc;
using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel;

namespace QuickNV.Core
{
    [DisplayName("媒体服务器相关")]
    [ApiController]
    [Route("/api/mediaServer")]
    public class MediaServerController
    {
        /// <summary>
        /// 获取全部媒体服务器
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Model.MediaServer[] GetMediaServers()
        {
            return ConfigDbContext.CacheContext.Query<Model.MediaServer>();
        }
    }
}
