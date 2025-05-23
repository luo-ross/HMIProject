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
    public partial class UserModel : NotifyBase
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
                this.SetProperty(ref email, value);
                this.ValidProperty(value);
            }
        }

        /// <summary>
        /// 用户昵称
        /// </summary>
        [ObservableProperty]
        private string? nickName;


        /// <summary>
        /// 用户头像
        /// </summary>
        [ObservableProperty]
        private string userPic;


        /// <summary>
        /// 电话 每个账户只能绑定一个手机号
        /// </summary>
        [ObservableProperty]
        private string? phone;

        /// <summary>
        /// 关联实名认证
        /// </summary>
        public long? RealNameId { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        [ObservableProperty]
        private bool? isDisabled;

        /// <summary>
        /// 创建
        /// </summary>
        [ObservableProperty]
        private bool? createTime;
    }
}
