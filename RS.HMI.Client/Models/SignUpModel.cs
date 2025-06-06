﻿using CommunityToolkit.Mvvm.ComponentModel;
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
    public partial class SignUpModel : UserBaseModel
    {


        private string? password = string.Empty;
        /// <summary>
        /// 用户密码
        /// </summary>
        [MaxLength(30, ErrorMessage = "密码长度不能超过30")]
        [Required(ErrorMessage = "密码输入不能为空")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "密码长度必须在8到30个字符之间")]
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
                if (this.SetProperty(ref passwordConfirm, value))
                {
                    ValidProperty(value);
                }
            }
        }


        /// <summary>
        /// 是否立即登录
        /// </summary>
        [ObservableProperty]
        private bool isLoginNow;
      
    }
}
