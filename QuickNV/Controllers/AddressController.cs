using Microsoft.AspNetCore.Mvc;
using Quick.EntityFrameworkCore.Plus;
using System.ComponentModel;
using QuickNV.Core;

namespace QuickNV.Controllers
{
    [DisplayName("地点相关")]
    [ApiController]
    [Route("/api/address")]
    public class AddressController
    {
        /// <summary>
        /// 获取地点列表
        /// </summary>
        /// <returns></returns>
        [HttpGet]
        public Model.Address[] GetAddresss()
        {
            return ConfigDbContext.CacheContext.Query<Model.Address>();
        }
    }
}
