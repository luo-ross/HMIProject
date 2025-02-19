using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Models.Widgets
{
    public class ValidErrorModel
    {
        /// <summary>
        /// 错误消息类型 推荐使用Guid确保唯一
        /// </summary>
        public string ErrorKey { get; set; }

        /// <summary>
        /// 错误消息
        /// </summary>
        public string ErrorMsg { get; set; }
    }
}
