using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    /// <summary>
    /// 注册会话类
    /// </summary>
    public class RegisterSessionBaseModel
    {

        /// <summary>
        /// 用户密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 会话主键
        /// </summary>
        public string? SessionId { get; set; }
    }
}
