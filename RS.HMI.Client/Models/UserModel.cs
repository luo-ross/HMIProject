using CommunityToolkit.Mvvm.ComponentModel;
using NPOI.SS.Formula.Functions;
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
    public class UserModel : UserBaseModel
    {

        private string? nickName;
        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? NickName
        {
            get { return nickName; }
            set
            {
                this.SetProperty(ref nickName, value);
            }
        }



        private string? userPic;
        /// <summary>
        /// 用户头像
        /// </summary>
        public string? UserPic
        {
            get { return userPic; }
            set
            {
                this.SetProperty(ref userPic, value);
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
                this.SetProperty(ref phone, value);
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
                this.SetProperty(ref isDisabled, value);
            }
        }

        private DateTime? createTime;
        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime
        {
            get { return createTime; }
            set
            {
                this.SetProperty(ref createTime, value);
            }
        }
    }
}
