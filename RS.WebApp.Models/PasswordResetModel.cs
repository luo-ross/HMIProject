using RS.Commons.Validation;
using System.ComponentModel.DataAnnotations;

namespace RS.WebApp.Models
{
    public class PasswordResetModel
    {
        /// <summary>
        /// 邮箱
        /// </summary>
        [Required(ErrorMessage = "邮箱不能为空")]
        [Email(ErrorMessage = "输入邮箱地址不合法")]
        public string Email { get; set; }
    }
}
