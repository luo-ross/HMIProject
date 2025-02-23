using RS.WPFApp.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace RS.WPFApp.ValidationAttributes
{
    /// <summary>
    /// 国际电话验证类
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    public sealed class InternationalPhone : ValidationAttribute
    {

        protected override ValidationResult? IsValid(object? value, ValidationContext validationContext)
        {
            if (validationContext.ObjectInstance is SMSRegisterModel smsRegisterModel)
            {
                string e164PhoneNumber = $"+{smsRegisterModel.CountryCode} {value}";
                var phoneNumberUtil = PhoneNumbers.PhoneNumberUtil.GetInstance();
                var phoneNumber = phoneNumberUtil.Parse(e164PhoneNumber, null);
                if (!phoneNumberUtil.IsValidNumber(phoneNumber))
                {
                    return new ValidationResult("电话号码错误，请重新输入！");
                }
            }
            return null;
        }
    }
}
