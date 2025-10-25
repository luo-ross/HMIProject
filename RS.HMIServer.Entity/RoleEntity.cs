using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 角色表(岗位职务)
    /// </summary>
    public sealed class RoleEntity : BaseEntity
    {
        /// <summary>
        /// 角色名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 绑定公司
        /// </summary>
        public string? CompanyId { get; set; }

    }
}