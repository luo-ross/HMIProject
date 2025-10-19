using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 公司
    /// </summary>
    public sealed class CompanyEntity : BaseEntity
    {

        /// <summary>
        /// 公司名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 所在地区
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// 关联公司认证
        /// </summary>
        public string? RealCompanyId { get; set; }

        /// <summary>
        /// 父级公司 用于集团继承
        /// </summary>
        public string? ParentId { get; set; }

    }
}