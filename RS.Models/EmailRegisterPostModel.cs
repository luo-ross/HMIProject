using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{

    /// <summary>
    /// 邮箱注册信息类
    /// </summary>
    public class EmailRegisterPostModel
    {
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public virtual string Email { get; set; }

        /// <summary>
        /// 密码
        /// </summary>
        public virtual string Password { get; set; }

    }
}
