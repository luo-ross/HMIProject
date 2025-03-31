using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Interceptors;
using RS.Models;
using System.Security.Claims;
using Microsoft.AspNetCore.Http.Extensions;

namespace RS.HMIServer.Areas
{
    /// <summary>
    /// WebApi控制器基类
    /// </summary>
    public abstract class BaseController : Controller
    {
        /// <summary>
        /// 获取会话Id
        /// </summary>
        public string SessionId
        {
            get
            {
                return User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Sid)?.Value;
            }
        }

        /// <summary>
        /// 完整的协议+主机名+端口
        /// </summary>
        public string HostWithScheme
        {
            get
            {
                var host = HttpContext.Request.Host.Value;
                var hostWithScheme = HttpContext.Request.Scheme + "://" + host;
                return hostWithScheme;
            }
        }



    }
}
