using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 入账代码合并列表
    /// </summary>
    public sealed class PostCodeMergeEntity : BaseEntity
    {

        /// <summary>
        /// 父级绑定PostCode主键Code
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 绑定PostCode的Code
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 金额设置
        /// </summary>
        public decimal Money { get; set; }


    }
}