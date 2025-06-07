using Microsoft.AspNetCore.Mvc.Filters;
using RS.Commons.Attributs;

namespace RS.HMIServer.Filters
{
    /// <summary>
    /// 鉴权过滤器：用于自定义接口的权限校验逻辑
    /// </summary>
    public class AuthorizationFilter : IAuthorizationFilter
    {
        /// <summary>
        /// 鉴权逻辑
        /// </summary>
        /// <param name="context">鉴权上下文</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
         
        }
    }
}
