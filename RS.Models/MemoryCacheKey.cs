using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 缓存主键
    /// </summary>
    public class MemoryCacheKey
    {
        static MemoryCacheKey()
        {
            GlobalRSAPublicKey = Guid.NewGuid().ToString();
            GlobalRSAPrivateKey = Guid.NewGuid().ToString();
            SessionModelKey = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// RSA全局公钥
        /// </summary>
        public static readonly string GlobalRSAPublicKey;

        /// <summary>
        /// RSA全局私钥
        /// </summary>
        public static readonly string GlobalRSAPrivateKey;

        /// <summary>
        /// 会话
        /// </summary>
        public static readonly string SessionModelKey;
     
    }
}
