using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    /// <summary>
    /// 该类用于存储客户端登录相关的信息模型。
    /// 包含客户端的 IP 地址、代理信息、IP 哈希值以及用户代理信息等。
    /// </summary>
    public class LoginClientModel
    {
        /// <summary>
        /// 获取或设置客户端的远程 IP 地址。
        /// 此地址通常代表客户端在网络中的实际外部地址。
        /// </summary>
        public string RemoteIpAddress { get; set; }

        /// <summary>
        /// 获取或设置客户端的本地 IP 地址。
        /// 本地 IP 地址一般是客户端在其所在局域网内的地址。
        /// </summary>
        public string LocalIpAddress { get; set; }

        /// <summary>
        /// 获取或设置客户端的 X-Forwarded-For 信息。
        /// X-Forwarded-For 是一个 HTTP 头字段，用于标识客户端经过代理服务器时的原始 IP 地址。
        /// </summary>
        public string XForwardedFor { get; set; }

        /// <summary>
        /// 获取或设置客户端 IP 地址的哈希值。
        /// 该哈希值可用于在某些场景下唯一标识客户端的 IP 地址，同时保护其隐私。
        /// </summary>
        public string ClientIPHash { get; set; }

        /// <summary>
        /// 获取或设置客户端的用户代理信息。
        /// 用户代理信息包含了客户端使用的浏览器、操作系统等相关信息。
        /// </summary>
        public string UserAgent { get; set; }
    }
}
