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
        /// 客户端发送给服务端的
        /// </summary>
        public string RsaPublicKey { get; set; }

        /// <summary>
        /// 客户端类型
        /// </summary>
        public string AudienceType { get; set; }
    }
}
