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
    public partial class SignUpModel : NotifyBase
    {

        private string userName;
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(30, ErrorMessage = "用户名长度不能超过30")]
        [Required(ErrorMessage = "用户名不能为空")]
        [RegularExpression("(?:[a-z0-9!#$%&'*+/=?^_`{|}~-]+(?:\\.[a-z0-9!#$%&'*+/=?^_`{|}~-]+)*|\"(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21\\x23-\\x5b\\x5d-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])*\")@(?:(?:[a-z0-9](?:[a-z0-9-]*[a-z0-9])?\\.)+[a-z0-9](?:[a-z0-9-]*[a-z0-9])?|\\[(?:(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?)\\.){3}(?:25[0-5]|2[0-4][0-9]|[01]?[0-9][0-9]?|[a-z0-9-]*[a-z0-9]:(?:[\\x01-\\x08\\x0b\\x0c\\x0e-\\x1f\\x21-\\x5a\\x53-\\x7f]|\\\\[\\x01-\\x09\\x0b\\x0c\\x0e-\\x7f])+)\\])", ErrorMessage = "用户名格式不正确")]
        public string UserName
        {
            get { return userName; }
            set
            {
                this.SetProperty(ref userName, value);
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
