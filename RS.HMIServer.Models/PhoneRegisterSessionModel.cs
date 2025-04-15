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
    public class PhoneRegisterSessionModel:RegisterSessionBaseModel
    {

        /// <summary>
        /// 电话号码 可以用来登录
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 手机验证码
        /// </summary>
        public string? PhoneVerificataion { get; set; }

        /// <summary>
        /// 手机验证码失效时间
        /// </summary>
        public long PhoneVerifyExpireTime { get; set; }
        
    }
}
