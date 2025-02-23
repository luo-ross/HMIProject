using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RS.RESTfulApi
{
    /// <summary>
    /// 服务接口类
    /// </summary>
    public class RSAppAPI
    {

        /// <summary>
        /// 获取会话接口
        /// </summary>
        public static readonly string GetSessionModel = "/Api/General/GetSessionModel";

        /// <summary>
        /// 获取注册邮箱验证码接口
        /// </summary>
        public static readonly string GetEmailVerification = "/Api/Register/GetEmailVerification";

        /// <summary>
        /// 注册邮箱验证码验证接口
        /// </summary>
        public static readonly string EmailVerificationValid = "/Api/Register/EmailVerificationValid";

        /// <summary>
        /// 获取用户接口
        /// </summary>
        public static readonly string GetUser = "/Api/User/GetUser";

        /// <summary>
        /// 验证登录接口
        /// </summary>
        public static readonly string ValidLogin = "/Api/User/ValidLogin";

    }
}
