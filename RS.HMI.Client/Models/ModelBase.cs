using CommunityToolkit.HighPerformance.Buffers;
using CommunityToolkit.Mvvm.ComponentModel;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Client.Models
{
    public  class ModelBase : NotifyBase
    {
        /// <summary>
        /// <summary>
        /// 主键 
        /// </summary>
        public string? Id { get; set; }


        private bool? isDelete;
        /// <summary>
        /// 是否删除
        /// </summary>
        public bool? IsDelete
        {
            get { return isDelete; }
            set
            {
                this.SetProperty(ref isDelete, value);
            }
        }


        /// <summary>
        /// 创建人主键
        /// </summary>
        public string? CreateId { get; set; }


        private string? createBy;
        /// <summary>
        /// 创建人
        /// </summary>
        public string? CreateBy
        {
            get { return createBy; }
            set
            {
                this.SetProperty(ref createBy, value);
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

        /// <summary>
        /// 最后一次更新人主键
        /// </summary>
        public string? UpdateId { get; set; }


        private string? updateBy;
        /// <summary>
        /// 更新人
        /// </summary>
        public string? UpdateBy
        {
            get { return updateBy; }
            set
            {
                this.SetProperty(ref updateBy, value);
            }
        }
      

        private DateTime? updateTime;
        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        public DateTime? UpdateTime
        {
            get { return updateTime; }
            set
            {
                this.SetProperty(ref updateTime, value);
            }
        }

        /// <summary>
        /// 删除人主键
        /// </summary>
        public string? DeleteId { get; set; }


        private string? deleteBy;
        /// <summary>
        /// 删除人
        /// </summary>
        public string? DeleteBy
        {
            get { return deleteBy; }
            set
            {
                this.SetProperty(ref deleteBy, value);
            }
        }


        private DateTime? deleteTime;
        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeleteTime
        {
            get { return deleteTime; }
            set
            {
                this.SetProperty(ref deleteTime, value);
            }
        }

    }
}
