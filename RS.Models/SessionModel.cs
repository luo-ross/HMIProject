using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 会话类
    /// </summary>
    public class SessionModel
    {
        /// <summary>
        /// 加密后的AES 秘钥
        /// </summary>
        public string AesKey { get; set; }

        /// <summary>
        /// 会话ID 
        /// </summary>
        public string AppId { get; set; }

        /// <summary>
        /// 回传给客户端的Token
        /// </summary>
        public string Token { get; set; }

    }
}
