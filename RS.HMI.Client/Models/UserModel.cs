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
    public partial class UserModel : UserBaseModel
    {
       
        /// <summary>
        /// 用户昵称
        /// </summary>
        [ObservableProperty]
        private string? nickName;


        /// <summary>
        /// 用户头像
        /// </summary>
        [ObservableProperty]
        private string? userPic;


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
        /// 创建时间
        /// </summary>
        [ObservableProperty]
        private DateTime? createTime;
    }
}
