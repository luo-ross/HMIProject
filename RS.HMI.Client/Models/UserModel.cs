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
    public class UserModel : NotifyBase
    {

        /// <summary>
        /// 数据主键
        /// </summary>
        public long Id { get; set; }


        private string email;
        /// <summary>
        /// 用户昵称
        /// </summary>
        [MaxLength(30, ErrorMessage = "邮箱长度不能超过30")]
        [Required(ErrorMessage = "邮箱不能为空")]
        [RegularExpression("^(?(\")(\".+?\"@)|(([0-9a-zA-Z]((\\.(?!\\.))|[-!#\\$%&'\\*\\+/=\\?\\^`\\{\\}\\|~\\w])*)(?<=[0-9a-zA-Z])@))(?(\\[)(\\[(\\d{1,3}\\.){3}\\d{1,3}\\])|(([0-9a-zA-Z][-\\w]*[0-9a-zA-Z]\\.)+[a-zA-Z]{2,6}))$", ErrorMessage = "用户名格式不正确")]
        public string Email
        {
            get { return email; }
            set
            {
                this.OnPropertyChanged(ref email, value);
                this.ValidProperty(value);
            }
        }



        private string? nickName;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? NickName
        {
            get { return nickName; }
            set
            {
                this.OnPropertyChanged(ref nickName, value);
            }
        }

        private string userPic;
        /// <summary>
        /// 用户头像
        /// </summary>
        public string UserPic
        {
            get { return userPic; }
            set
            {
                this.OnPropertyChanged(ref userPic, value);
            }
        }


        private string? phone;
        /// <summary>
        /// 电话 每个账户只能绑定一个手机号
        /// </summary>
        public string? Phone
        {
            get { return phone; }
            set
            {
                this.OnPropertyChanged(ref phone, value);
            }
        }

        /// <summary>
        /// 关联实名认证
        /// </summary>
        public long? RealNameId { get; set; }


        private bool? isDisabled;
        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool? IsDisabled
        {
            get { return isDisabled; }
            set
            {
                this.OnPropertyChanged(ref isDisabled, value);
            }
        }
    }
}
