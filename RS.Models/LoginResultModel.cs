using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{

    /// <summary>
    /// 登录结果
    /// </summary>
    public class LoginResultModel
    {
        /// <summary>
        /// 新的会话Model
        /// </summary>
        public SessionModel SessionModel { get; set; }

        /// <summary>
        /// 返回昵称
        /// </summary>
        public string NickName { get; set; }

        /// <summary>
        /// 返回用户的头像
        /// </summary>
        public byte[] UserImgUrl { get; set; }


    }
}
