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
    public class RegisterVerificationModel
    {
        /// <summary>
        /// 这个Token值就是Redis数据库的键值Key
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 验证码有效时间
        /// </summary>
        public long ExpireTime { get; set; }
    }
}
