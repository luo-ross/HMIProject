using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 客户端类型
    /// </summary>
    public class AudienceType
    {
        /// <summary>
        /// Windows客户端
        /// </summary>
        public static readonly string WindowsAudience = "WindowsAudience";

        /// <summary>
        /// 安卓客户端
        /// </summary>
        public static readonly string AndriodAudience = "AndriodAudience";

        /// <summary>
        /// IOS客户端
        /// </summary>
        public static readonly string IOSAudience = "IOSAudience";

        /// <summary>
        /// 微信客户端
        /// </summary>
        public static readonly string WeChatAudience = "WeChatAudience";

        /// <summary>
        /// 钉钉客户端
        /// </summary>
        public static readonly string DingTalkAudience = "DingTalkAudience";

        /// <summary>
        /// 抖音客户端
        /// </summary>
        public static readonly string TikTalkAudience = "TikTalkAudience";
    }
}
