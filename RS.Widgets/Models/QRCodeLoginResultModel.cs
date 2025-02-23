using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 二维码登录
    /// </summary>
    public class QRCodeLoginResultModel
    {

        /// <summary>
        /// 是否获取成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 通信唯一识别码
        /// </summary>
        public string Token { get; set; }

        /// <summary>
        /// 二维码内容
        /// </summary>
        public string QRCodeContent { get; set; }

        /// <summary>
        /// 时间戳 单位毫秒
        /// </summary>
        public long ExpireTime { get; set; }
    }
}
