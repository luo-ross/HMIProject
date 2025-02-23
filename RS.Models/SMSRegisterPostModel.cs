using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 短信注册信息类
    /// </summary>
    public class SMSRegisterPostModel
    {
      /// <summary>
      /// 电话号码
      /// </summary>
        public string Phone { get; set; }

        /// <summary>
        /// 国家代码
        /// </summary>
        public string CountryCode { get; set; }

        /// <summary>
        /// 注册会话Token 先进行邮箱验证后 再进行短信验证 需要注册会话Id一起发送到服务端
        /// </summary>
        public string Token { get; set; }
    }
}
