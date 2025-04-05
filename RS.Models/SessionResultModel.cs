using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 返回给客户端会话签名数据
    /// </summary>
    public class SessionResultModel : SignModel
    {
        /// <summary>
        /// 服务端回传给客户端的加密公钥
        /// </summary>
        public string RSAEncryptPublicKey { get; set; }

        /// <summary>
        /// 服务端回传给客户端的签名公钥
        /// </summary>
        public string RSASignPublicKey { get; set; }

        /// <summary>
        /// 会话实体
        /// </summary>
        public SessionModel SessionModel { get; set; }
    }
}
