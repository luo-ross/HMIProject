using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    /// <summary>
    /// 基础类
    /// </summary>
    public class ModelBase
    {
        /// <summary>
        /// <summary>
        /// 主键 
        /// </summary>
        public string? Id { get; set; }

        /// <summary>
        /// 是否删除
        /// </summary>
        public bool? IsDelete { get; set; }

        /// <summary>
        /// 创建人主键
        /// </summary>
        public string? CreateId { get; set; }

        /// <summary>
        /// 创建人名称
        /// </summary>
        public string? CreateBy { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime? CreateTime { get; set; }

        /// <summary>
        /// 最后一次更新人主键
        /// </summary>
        public string? UpdateId { get; set; }

        /// <summary>
        /// 更新人名称
        /// </summary>
        public string? UpdateBy { get; set; }

        /// <summary>
        /// 最后一次更新时间
        /// </summary>
        public DateTime? UpdateTime { get; set; }

        /// <summary>
        /// 删除人主键
        /// </summary>
        public string? DeleteId { get; set; }

        /// <summary>
        /// 删除人
        /// </summary>
        public string? DeleteBy { get; set; }

        /// <summary>
        /// 删除时间
        /// </summary>
        public DateTime? DeleteTime { get; set; }

    }
}
