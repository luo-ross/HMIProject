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
    public class RegisterSessionModel
    {
        /// <summary>
        /// 邮箱地址 可以用来登录
        /// </summary>
        public string Email { get; set; }

        /// <summary>
        /// 电话号码 可以用来登录
        /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 用户密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 邮箱验证码
        /// </summary>
        public string EmailVerificataion { get; set; }

        /// <summary>
        /// 邮箱验证码验证码有效时间
        /// </summary>
        public long EmailVerificationExpireTime { get; set; }

        /// <summary>
        /// 手机验证码
        /// </summary>
        public string PhoneVerificataion { get; set; }

        /// <summary>
        /// 手机验证码失效时间
        /// </summary>
        public long PhoneVerificationExpireTime { get; set; }

        /// <summary>
        /// 会话主键
        /// </summary>
        public string SessionId { get; set; }
    }
}
