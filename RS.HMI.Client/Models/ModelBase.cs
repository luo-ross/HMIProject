using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public partial class ModelBase : NotifyBase
    {
        /// <summary>
        /// <summary>
        /// 主键 
        /// </summary>
        public long? Id { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        [ObservableProperty]
        private bool? isDelete;

        /// <summary>
        /// 创建人主键
        /// </summary>
        public long? CreateId { get; set; }

        /// <summary>
        /// 创建人姓名
        /// </summary>
        [ObservableProperty]
        private string? createBy;

        /// <summary>
        /// 创建时间
        /// </summary>
        [ObservableProperty]
        private DateTime? createTime;

        /// <summary>
        /// 最后一次更新人主键
        /// </summary>
        public long? UpdateId { get; set; }

        /// <summary>
        /// 更新人姓名
        /// </summary>
        [ObservableProperty]
        private string? updateBy;

        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        [ObservableProperty]
        private DateTime? updateTime;

        /// <summary>
        /// 删除人主键
        /// </summary>
        public long? DeleteId { get; set; }

        /// <summary>
        /// 删除人姓名
        /// </summary>
        [ObservableProperty]
        private string? deleteBy;

        /// <summary>
        /// 删除时间
        /// </summary>
        [ObservableProperty]
        private DateTime? deleteTime;





    }
}
