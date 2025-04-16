using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 注册验证码返回值类
    /// </summary>
    public class RegisterVerifyModel
    {
        /// <summary>
        /// 注册会话Id
        /// </summary>
        public string RegisterSessionId { get; set; }

        /// <summary>
        /// 验证码有效时间
        /// </summary>
        public long ExpireTime { get; set; }

        /// <summary>
        /// 新的会话JWT
        /// </summary>
        public string Token { get; set; }
    }
}
