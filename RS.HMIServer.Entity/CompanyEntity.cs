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
        public string? Name { get; set; }

        /// <summary>
        /// 所在地区
        /// </summary>
        public string? Area { get; set; }

        /// <summary>
        /// 关联公司认证
        /// </summary>
        public long? RealCompanyId { get; set; }

        /// <summary>
        /// 绑定用户
        /// </summary>
        public long?  UserId { get; set; }

    }
}