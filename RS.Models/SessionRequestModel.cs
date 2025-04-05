using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 这是客户端向服务端发送会话请求类
    /// </summary>
    public class SessionRequestModel : SignModel
    {
        /// <summary>
        /// 签名验签公钥
        /// </summary>
        public string RSASignPublicKey { get; set; }

        /// <summary>
        /// 加解密公钥
        /// </summary>
        public string RSAEncryptPublicKey { get; set; }

        /// <summary>
        /// 客户端类型
        /// </summary>
        public string AudienceType { get; set; }
    }
}
