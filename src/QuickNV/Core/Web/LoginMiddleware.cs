using System.Text;
using YiQiDong.Agent;

namespace QuickNV.Core.Web
{
    public class LoginMiddleware
    {
        public static LoginMiddleware Instance { get; private set; }
        private RequestDelegate _next;

        public LoginMiddleware(RequestDelegate next = null)
        {
            Instance = this;
            _next = next;
        }


        private static List<string> whiteUrlList = new List<string>(new[]
            {
                "/_blazor",
                "/Login"
            }
        );

        private static List<string> whiteUrlPrefixList = new List<string>(new[]
            {
                "/ws/",
                "/css/",
                "/js/",
                "/_blazor/",
                "/_framework/",
                "/api/zlhook/",
                "/api/player/conf"
            }
        );

        public static bool IsPathInWhiteList(string path)
        {
            if (whiteUrlPrefixList.Any(t => path.StartsWith(t)))
                return true;
            if (whiteUrlList.Contains(path))
                return true;
            return false;
        }
        private const string SESSION_IS_LOGIN = nameof(SESSION_IS_LOGIN);
        public async Task Invoke(HttpContext context)
        {
            var req = context.Request;
            var rep = context.Response;
            var path = req.Path.Value;
            var isMainPageVisit = path == "/";

            //允许跨域访问
            rep.Headers.AccessControlAllowOrigin = new Microsoft.Extensions.Primitives.StringValues("*");
            rep.Headers.AccessControlAllowMethods = new Microsoft.Extensions.Primitives.StringValues("*");
            rep.Headers.AccessControlAllowHeaders = new Microsoft.Extensions.Primitives.StringValues("*");
            rep.Headers.AccessControlMaxAge = new Microsoft.Extensions.Primitives.StringValues("86400");

            //如果在白名单中，则放行
            if (IsPathInWhiteList(path))
            {
                await _next.Invoke(context);
                return;
            }
            //验证Session中是否已经登录，如果已经登录，则放行
            if (!string.IsNullOrEmpty(context.Session.GetString(SESSION_IS_LOGIN)))
            {
                await _next.Invoke(context);
                return;
            }
            //验证ApiKey
            var apiKey = req.GetQueryStringValue(ApiKeyManager.API_KEY);
            if (!string.IsNullOrEmpty(apiKey))
            {
                //ApiKey认证成功
                if (ApiKeyManager.Instance.ValidateApiKey(apiKey))
                {
                    context.Session.SetString(SESSION_IS_LOGIN, true.ToString());
                    await _next.Invoke(context);
                    return;
                }
                if (!isMainPageVisit)
                {
                    rep.StatusCode = 401;
                    rep.ContentType = "text/plain;charset=utf-8";
                    await rep.WriteAsync($"认证失败，ApiKey[{apiKey}]无效", Encoding.UTF8);
                    return;
                }
            }
            //通过易认证接口进行认证
            if (YiRenZhengManager.Instance.Connected)
            {
                var parameters = new Dictionary<string, string>();
                //添加Header
                foreach (var item in rep.Headers)
                    parameters[item.Key] = item.Value;
                //添加Query
                foreach (var item in req.Query)
                    parameters[item.Key] = item.Value;
                //通过易认证进行认证
                var authRep = await YiRenZhengManager.Instance.Authenticate($"{req.Scheme}://{req.Host.Value}{req.Path.Value}{req.QueryString.ToUriComponent()}", parameters);
                //如果认证成功，则放行
                if (authRep.Success)
                {
                    context.Session.SetString(SESSION_IS_LOGIN, true.ToString());
                    await _next.Invoke(context);
                    return;
                }
                if (parameters.Count > 0)
                    AgentContext.LogDebug($"[易认证接口]认证失败，消息：{authRep.Message}");
                if (!isMainPageVisit)
                {
                    rep.StatusCode = 401;
                    rep.ContentType = "text/plain;charset=utf-8";
                    await rep.WriteAsync($"认证失败，消息：{authRep.Message}", Encoding.UTF8);
                    return;
                }
            }
            //如果是API访问
            if (!isMainPageVisit)
            {
                rep.StatusCode = 401;
                rep.ContentType = "text/plain;charset=utf-8";
                await rep.WriteAsync($"没有权限查看[{path}]", Encoding.UTF8);
                return;
            }
            //否则跳转到登录页面
            rep.Redirect("./Login");
        }
    }
}
