using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
namespace RS.WebApp.Entity
{

    /// <summary>
    /// 入账代码配置
    /// </summary>
    public sealed class PostCodeEntity 
    {
        /// <summary>
        /// 入账代码
        /// </summary>
        [Key]
        public string Code { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal Money { get; set; }

        /// <summary>
        /// 是否合并
        /// </summary>
        public bool IsMerge { get; set; }












    }
}