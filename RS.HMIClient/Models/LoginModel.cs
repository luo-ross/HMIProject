using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIClient.Models
{
    public class LoginModel : UserModel
    {
        private bool isRememberPassword;
        /// <summary>
        /// 是否记住密码
        /// </summary>
        public bool IsRememberPassword
        {
            get { return isRememberPassword; }
            set
            {
                this.OnPropertyChanged(ref isRememberPassword, value);
            }
        }

        private string verify;
        /// <summary>
        /// 验证码
        /// </summary>
        [MaxLength(11, ErrorMessage = "验证码长度不能超过11位")]
        [Required(ErrorMessage = "验证码不能为空")]
        public string Verify
        {
            get { return verify; }
            set
            {
                this.OnPropertyChanged(ref verify, value);
                this.ValidProperty(value);
            }
        }
    }
}
