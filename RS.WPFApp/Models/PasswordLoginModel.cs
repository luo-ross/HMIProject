using RS.Widgets.Models;
using System.ComponentModel.DataAnnotations;

namespace RS.WPFApp.Models
{
    /// <summary>
    /// 密码登录类
    /// </summary>
    public class PasswordLoginModel : NotifyBase
    {

        private string userName;
        /// <summary>
        /// 用户名或者邮箱
        /// </summary>
        [Required(ErrorMessage = "电话或者邮箱不能为空")]
        public string UserName
        {
            get { return userName; }
            set
            {
                OnPropertyChanged(ref userName, value);
                ValidProperty(value);
            }
        }

        private string password = string.Empty;
        /// <summary>
        /// 密码
        /// </summary>
        [Required(ErrorMessage = "密码输入不能为空")]
        [MinLength(6, ErrorMessage = "密码长度至少6位")]
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

    }
}
