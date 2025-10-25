using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public  class LoginModel : UserBaseModel
    {

        private bool isAutoLogin;
        /// <summary>
        /// 是否自动登录
        /// </summary>
        public bool IsAutoLogin
        {
            get { return isAutoLogin; }
            set
            {
                this.SetProperty(ref isAutoLogin, value);
            }
        }


        private string? password = string.Empty;
        /// <summary>
        /// 用户密码
        /// </summary>
        [MaxLength(30, ErrorMessage = "密码长度不能超过30")]
        [MinLength(8, ErrorMessage = "密码长度至少8位")]
        [Required(ErrorMessage = "密码输入不能为空")]
        public string? Password
        {
            get { return password; }
            set
            {
                if (this.SetProperty(ref password, value))
                {
                    ValidProperty(value);
                }
            }
        }
    }
}
