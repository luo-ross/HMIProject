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
            GlobalRSASignPublicKey = Guid.NewGuid().ToString();
            GlobalRSASignPrivateKey = Guid.NewGuid().ToString();
            GlobalRSAEncryptPublicKey = Guid.NewGuid().ToString();
            GlobalRSAEncryptPrivateKey = Guid.NewGuid().ToString();
            SessionModelKey = Guid.NewGuid().ToString();
        }

        /// <summary>
        /// RSA全局公钥
        /// </summary>
        public static readonly string GlobalRSASignPublicKey;

        /// <summary>
        /// RSA全局私钥
        /// </summary>
        public static readonly string GlobalRSASignPrivateKey;

        /// <summary>
        /// RSA全局公钥
        /// </summary>
        public static readonly string GlobalRSAEncryptPublicKey;

        /// <summary>
        /// RSA全局私钥
        /// </summary>
        public static readonly string GlobalRSAEncryptPrivateKey;

        /// <summary>
        /// 会话
        /// </summary>
        public static readonly string SessionModelKey;


        /// <summary>
        /// 服务端RSA全局公钥
        /// </summary>
        public static readonly string ServerGlobalRSASignPublicKey;

        /// <summary>
        /// 服务端RSA全局私钥
        /// </summary>
        public static readonly string ServerGlobalRSAEncryptPublicKey;

    }
}
