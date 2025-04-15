using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    /// <summary>
    /// 注册会话类
    /// </summary>
    public class EmailRegisterSessionModel : RegisterSessionBaseModel
    {
        /// <summary>
        /// 邮箱地址 可以用来登录
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        public string? EmailVerificataion { get; set; }

        /// <summary>
        /// 邮箱验证码验证码有效时间
        /// </summary>
        public long EmailVerifyExpireTime { get; set; }

    }
}
