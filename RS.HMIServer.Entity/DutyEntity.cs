using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 职位表
    /// </summary>
    public sealed class DutyEntity : BaseEntity
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

        /// <summary>
        /// 绑定部门
        /// </summary>
        public long? PartmentId { get; set; }



    }
}