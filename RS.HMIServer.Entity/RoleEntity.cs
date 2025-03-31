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
        /// 父级
        /// </summary>
        public long? ParentId { get; set; }

        /// <summary>
        /// 备注
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 层级
        /// </summary>
        public int Level { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 绑定公司
        /// </summary>
        public long? CompanyId { get; set; }


        /// <summary>
        /// 绑定部门
        /// </summary>
        public long? DepartmentId { get; set; }





    }
}