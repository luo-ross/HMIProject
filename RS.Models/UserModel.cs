using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 用户类
    /// </summary>
    public class UserModel : ModelBase
    {
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserPic { get; set; }

        /// <summary>
        /// 邮箱 一个邮箱只能绑定一个手机号
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 电话 每个账户只能绑定一个手机号
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool? IsDisabled { get; set; }
    }
}
