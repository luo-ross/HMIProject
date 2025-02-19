using RS.HMI.Client.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace RS.HMI.Client.ValidationAttributes
{
    /// <summary>
    /// 密码确认验证类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class PasswordConfirm : ValidationAttribute
    {
        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            //如果是邮箱注册实体
            if (validationContext.ObjectInstance is SignUpModel signUpModel)
            {
                if (!signUpModel.PasswordConfirm.Equals(signUpModel.Password))
                {
                    return new ValidationResult("确认密码与输入密码不一致！");
                }
            }
            return null;
        }
    }
}
