using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Configuration;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public partial class UserBaseModel : ModelBase
    {
       
        private string? email;
        /// <summary>
        /// 用户昵称
        /// </summary>
        [MaxLength(30, ErrorMessage = "邮箱长度不能超过30")]
        [Required(ErrorMessage = "邮箱不能为空")]
        [RegularExpression("^(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$", ErrorMessage = "用户名格式不正确")]
        public string? Email
        {
            get { return email; }
            set
            {
                this.SetProperty(ref email, value);
                this.ValidProperty(value);
            }
        }
    }
}
