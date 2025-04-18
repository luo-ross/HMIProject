using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.IdentityModel.Logging;
using Microsoft.IdentityModel.Tokens;
using Newtonsoft.Json.Linq;
using RS.Commons.Extend;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace RS.HMIServer.Controllers
{
    /// <summary>
    /// WebApi控制器基类
    /// </summary>
    public abstract class BaseController : Controller
    {
        public BaseController()
        {


        }

        /// <summary>
        /// 角色
        /// </summary>
        public string Role
        {
            get
            {
                return User.Claims.FirstOrDefault(t => t.Type == ClaimTypes.Role)?.Value;
            }
        }

        public string Audiences
        {
            get
            {
                return User.Claims.FirstOrDefault(t => t.Type == "aud")?.Value;
            }
        }


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
