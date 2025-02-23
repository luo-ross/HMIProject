using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 验证码类
    /// </summary>
    public  class VerificationModel
    {
        /// <summary>
        /// 验证码是否获取成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 时间戳 单位毫秒
        /// </summary>
        public long ExpireTime { get; set; }
    }
}
