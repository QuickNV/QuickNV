using Microsoft.Extensions.Primitives;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Microsoft.AspNetCore.Http
{
    public static class HttpContextExtenstions
    {
        /// <summary>
        /// 获取查询中的字符串值
        /// </summary>
        /// <param name="request"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        public static string GetQueryStringValue(this HttpRequest request, string key)
        {
            StringValues ret;
            if (request.Query.TryGetValue(key, out ret))
                return ret.ToString();
            return null;
        }
    }
}
