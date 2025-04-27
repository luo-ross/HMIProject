using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{

    /// <summary>
    /// 登录验证类
    /// </summary>
    public class LoginValidModel
    {
        /// <summary>
        /// 用户名
        /// </summary>
        public string UserName { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public string Password { get; set; }

        /// <summary>
        /// 验证会话Id
        /// </summary>
        public string VerifySessionId { get; set; }

        /// <summary>
        /// 验证码结果 一个矩形框
        /// </summary>
        public RectModel Verify { get; set; }

    }
}
