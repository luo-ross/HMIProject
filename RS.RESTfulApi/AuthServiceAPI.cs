using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace RS.RESTfulApi
{
    /// <summary>
    /// API路由配置类
    /// </summary>
    public class RSAppAPI
    {
        /// <summary>
        /// API版本
        /// </summary>
        private const string ApiVersion = "v1";

        /// <summary>
        /// 通用API
        /// </summary>
        public static class General
        {
            /// <summary>
            /// 心跳检测接口
            /// </summary>
            public static readonly string HeartBeatCheck = $"/Api/{ApiVersion}/General/HeartBeatCheck";

            /// <summary>
            /// 获取会话接口
            /// </summary>
            public static readonly string GetSessionModel = $"/Api/{ApiVersion}/General/GetSessionModel";
        }

        /// <summary>
        /// 注册相关API
        /// </summary>
        public static class Register
        {
            /// <summary>
            /// 获取注册邮箱验证码接口
            /// </summary>
            public static readonly string GetEmailVerify = $"/Api/{ApiVersion}/Register/GetEmailVerify";

            /// <summary>
            /// 注册邮箱验证码验证接口
            /// </summary>
            public static readonly string EmailVerifyValid = $"/Api/{ApiVersion}/Register/EmailVerifyValid";
        }

        /// <summary>
        /// 用户相关API
        /// </summary>
        public static class User
        {
            /// <summary>
            /// 获取用户接口
            /// </summary>
            public static readonly string GetUser = $"/Api/{ApiVersion}/User/GetUser";

            /// <summary>
            /// 验证登录接口
            /// </summary>
            public static readonly string ValidLogin = $"/Api/{ApiVersion}/User/ValidLogin";
        }
    }
}
