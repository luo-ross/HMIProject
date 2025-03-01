using RS.Widgets.Models;
using System.ComponentModel.DataAnnotations;
using RS.WPFApp.Validation;
using RS.Commons.Validation;

namespace RS.WPFApp.Models
{
    /// <summary>
    /// 邮箱注册类
    /// </summary>
    public class EmailRegisterModel : NotifyBase
    {
        private string emailAddress;
        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [Email(ErrorMessage = "输入邮箱地址不合法")]
        public string Email
        {
            get { return emailAddress; }
            set
            {
                OnPropertyChanged(ref emailAddress, value);
                ValidProperty(value);
            }
        }


        private string password = string.Empty;
        /// <summary>
        /// 输入密码
        /// </summary>
        [Required(ErrorMessage = "密码输入不能为空")]
        [MinLength(8, ErrorMessage = "密码长度至少8位")]
        public string Password
        {
            get { return password; }
            set
            {
                var isChanged = OnPropertyChanged(ref password, value);
                if (isChanged)
                {
                    ValidProperty(value);
                }
            }
        }


        private string passwordConfirm = string.Empty;
        /// <summary>
        /// 再次确认密码
        /// </summary>
        [Required(ErrorMessage = "密码输入不能为空")]
        [MinLength(8, ErrorMessage = "密码长度至少8位")]
        [PasswordConfirm]
        public string PasswordConfirm
        {
            get { return passwordConfirm; }
            set
            {
                var isChanged = OnPropertyChanged(ref passwordConfirm, value);
                if (isChanged)
                {
                    ValidProperty(value);
                }
            }
        }


        private string verification;
        /// <summary>
        /// 验证码
        /// </summary>
        [Required(ErrorMessage = "验证码不能为空")]
        public string Verification
        {
            get { return verification; }
            set
            {
                OnPropertyChanged(ref verification, value);
                ValidProperty(value);
            }
        }
    }
}
