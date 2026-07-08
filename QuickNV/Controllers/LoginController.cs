using Microsoft.AspNetCore.Mvc;
using System.ComponentModel;

namespace QuickNV.Controllers
{
    [DisplayName("登录相关")]
    [ApiController]
    [Route("/api/login")]
    public class LoginController : ControllerBase
    {
        /// <summary>
        /// 心跳
        /// </summary>
        /// <returns></returns>
        [HttpGet("heartbeat")]
        public string Heartbeat()
        {
            return "OK";
        }

        /// <summary>
        /// 登录到主页面
        /// </summary>
        [HttpGet("login")]
        public void Login()
        {
            HttpContext.Response.Redirect("../../");
        }

        /// <summary>
        /// 退出登录
        /// </summary>
        [HttpGet("logout")]
        public void Logout()
        {
            //清除Session
            HttpContext.Session.Clear();
            //登录
            Login();
        }
    }
}
