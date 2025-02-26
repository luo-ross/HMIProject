using RS.HMI.Client.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public class SignUpModel : UserModel
    {
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
