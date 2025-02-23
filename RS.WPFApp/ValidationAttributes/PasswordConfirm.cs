using RS.WPFApp.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace RS.WPFApp.ValidationAttributes
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
            if (validationContext.ObjectInstance is EmailRegisterModel emailRegisterModel)
            {
                if (!emailRegisterModel.PasswordConfirm.Equals(emailRegisterModel.Password))
                {
                    return new ValidationResult("确认密码与输入密码不一致！");
                }
            }
            return null;
        }
    }
}
