﻿using RS.Widgets.Models;
using System.ComponentModel.DataAnnotations;
using RS.Annotation.Validation;
using RS.Commons.Validation;

namespace RS.Annotation.Models
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
                this.SetProperty(ref emailAddress, value);
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
                var isChanged = this.SetProperty(ref password, value);
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
                var isChanged = this.SetProperty(ref passwordConfirm, value);
                if (isChanged)
                {
                    ValidProperty(value);
                }
            }
        }


        private string verify;
        /// <summary>
        /// 验证码
        /// </summary>
        [Required(ErrorMessage = "验证码不能为空")]
        public string Verify
        {
            get { return verify; }
            set
            {
                this.SetProperty(ref verify, value);
                ValidProperty(value);
            }
        }
    }
}
