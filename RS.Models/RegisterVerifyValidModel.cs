using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 注册验证码类
    /// </summary>
    public class RegisterVerifyValidModel
    {
        /// <summary>
        /// 注册会话Id
        /// </summary>
        public string RegisterSessionId { get; set; }

        /// <summary>
        /// 验证码
        /// </summary>
        public string Verify { get; set; }
    }
}
