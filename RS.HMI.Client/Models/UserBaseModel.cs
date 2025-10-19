using CommunityToolkit.Mvvm.ComponentModel;
using RS.HMI.Client.Validation;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public  class UserBaseModel : ModelBase
    {
       
        private string? email;
        /// <summary>
        /// 用户昵称
        /// </summary>
        [MaxLength(30, ErrorMessage = "邮箱长度不能超过30")]
        [Required(ErrorMessage = "邮箱不能为空")]
        [RegularExpression("^(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$", ErrorMessage = "用户名格式不正确")]
        public string? Email
        {
            get { return email; }
            set
            {
                this.SetProperty(ref email, value);
                this.ValidProperty(value);
            }
        }


        private string? password = string.Empty;
        /// <summary>
        /// 用户密码
        /// </summary>
        [MaxLength(30, ErrorMessage = "密码长度不能超过30")]
        [MinLength(8, ErrorMessage = "密码长度至少8位")]
        [Required(ErrorMessage = "密码输入不能为空")]
        [PasswordConfirm]
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
