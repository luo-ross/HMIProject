using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIClient.Models
{
    public class UserModel : NotifyBase
    {
        private string name;
        /// <summary>
        /// 用户名
        /// </summary>
        [MaxLength(30,ErrorMessage = "用户名长度不能超过30")]
        [Required(ErrorMessage = "用户名不能为空")]
        public string Name
        {
            get { return name; }
            set
            {
                this.OnPropertyChanged(ref name, value);
                this.ValidProperty(value);
            }
        }


        private string email;
        /// <summary>
        /// 用户邮箱
        /// </summary>
        //[RegularExpression(@"^[a-zA - Z0 - 9_.+-]+@[a-zA - Z0 - 9 -]+\.[a-zA - Z0 - 9-.]+$", ErrorMessage = "电子邮件格式不正确")]
        public string Email
        {
            get { return email; }
            set
            {
                this.OnPropertyChanged(ref email, value);
                this.ValidProperty(value);
            }
        }


        private string password = string.Empty;
        /// <summary>
        /// 用户密码
        /// </summary>
        [MaxLength(30,ErrorMessage = "密码长度不能超过30")]
        [Required(ErrorMessage="用户密码不能为空")]
        [StringLength(30, MinimumLength = 8, ErrorMessage = "密码长度必须在8到30个字符之间")]
        public string Password
        {
            get { return password; }
            set
            {
                if (OnPropertyChanged(ref password, value))
                {
                    ValidProperty(value);
                }
            }
        }
    }
}
