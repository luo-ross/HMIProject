using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 部门表
    /// </summary>
    public sealed class DepartmentEntity : BaseEntity
    {

        /// <summary>
        /// 中文名称
        /// </summary>
        public string? ChName { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string? EnName { get; set; }

        /// <summary>
        /// 缩写简称
        /// </summary>
        public string? Abbr { get; set; }

        /// <summary>
        /// 绑定公司
        /// </summary>
        public long? CompanyId { get; set; }




    }
}