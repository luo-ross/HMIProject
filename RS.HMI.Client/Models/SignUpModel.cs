using RS.HMI.Client.Validation;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public class SignUpModel : NotifyBase
    {

        private string userName;
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(30, ErrorMessage = "用户名长度不能超过30")]
        [Required(ErrorMessage = "用户名不能为空")]
        [RegularExpression("^(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$", ErrorMessage = "用户名格式不正确")]
        public string UserName
        {
            get { return userName; }
            set
            {
                this.OnPropertyChanged(ref userName, value);
                this.ValidProperty(value);
            }
        }

        private string password = string.Empty;
        /// <summary>
        /// 用户密码
        /// </summary>
        [Required(ErrorMessage = "用户密码不能为空")]
        public string Password
        {
            get { return password; }
            set
            {
                if (OnPropertyChanged(ref password, value))
                {
                    ValidProperty(value);
                }
            }
        }


        private string passwordConfirm = string.Empty;
        /// <summary>
        /// 密码确认
        /// </summary>
        [MaxLength(30, ErrorMessage = "密码长度不能超过30")]
        [Required(ErrorMessage = "密码输入不能为空")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "密码长度必须在8到30个字符之间")]
        [PasswordConfirm]
        public string PasswordConfirm
        {
            get { return passwordConfirm; }
            set
            {
                if (OnPropertyChanged(ref passwordConfirm, value))
                {
                    ValidProperty(value);
                }
            }
        }


        private bool isLoginNow;
        /// <summary>
        /// 是否立即登录
        /// </summary>
        public bool IsLoginNow
        {
            get { return isLoginNow; }
            set
            {
                this.OnPropertyChanged(ref isLoginNow, value);
            }
        }
    }
}
