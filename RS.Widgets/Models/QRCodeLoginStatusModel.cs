using RS.Commons.Enums;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 二维码登录状态
    /// </summary>
    public class QRCodeLoginStatusModel
    {
        /// <summary>
        /// 是否获取成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 二维码登录状态
        /// </summary>
        public QRCodeLoginStatusEnum QRCodeLoginStatus { get; set; }

        /// <summary>
        /// 二维码登录状态的描述
        /// </summary>
        public string Message { get; set; }
    }
}
