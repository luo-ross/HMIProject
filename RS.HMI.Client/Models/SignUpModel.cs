using CommunityToolkit.Mvvm.ComponentModel;
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
    public class SignUpModel : UserBaseModel
    {

        public bool IsPasswordChanged { get; set; }
        public bool IsPasswordConfirmChanged { get; set; }

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
                this.IsPasswordChanged = this.SetProperty(ref password, value);
                if (IsPasswordChanged)
                {
                    ValidProperty(value);
                    if (this.IsPasswordConfirmChanged)
                    {
                        ValidProperty(value, nameof(PasswordConfirm));
                    }
                }
            }
        }


        private string passwordConfirm = string.Empty;
        /// <summary>
        /// 密码确认
        /// </summary>
        [MaxLength(30, ErrorMessage = "密码长度不能超过30")]
        [MinLength(8, ErrorMessage = "密码长度至少8位")]
        [Required(ErrorMessage = "密码输入不能为空")]
        [PasswordConfirm]
        public string PasswordConfirm
        {
            get { return passwordConfirm; }
            set
            {
                this.IsPasswordConfirmChanged = this.SetProperty(ref passwordConfirm, value);
                if (IsPasswordConfirmChanged)
                {
                    ValidProperty(value);
                    if (this.IsPasswordChanged)
                    {
                        ValidProperty(value, nameof(Password));
                    }
                }
            }
        }

    }
}
