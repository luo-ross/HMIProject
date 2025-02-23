using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 注册验证码类
    /// </summary>
    public class RegisterVerificationValidModel
    {
        /// <summary>
        /// 注册会话Id
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string Verification { get; set; }
    }
}
