using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public class LoginModel : NotifyBase
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


        private bool isRememberPassword;
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsRememberPassword
        {
            get { return isRememberPassword; }
            set
            {
                this.OnPropertyChanged(ref isRememberPassword, value);
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


        //private string verify;
        ///// <summary>
        ///// 验证码
        ///// </summary>
        //[MaxLength(11, ErrorMessage = "验证码长度不能超过11位")]
        //[Required(ErrorMessage = "验证码不能为空")]
        //public string Verify
        //{
        //    get { return verify; }
        //    set
        //    {
        //        this.OnPropertyChanged(ref verify, value);
        //        this.ValidProperty(value);
        //    }
        //}
    }
}
