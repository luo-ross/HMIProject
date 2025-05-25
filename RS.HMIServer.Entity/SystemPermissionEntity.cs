using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{
    /// <summary>
    /// 系统菜单按钮权限
    /// </summary>
    public sealed class SystemPermissionEntity : BaseEntity
    {

        /// <summary>
        /// 名称
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int Sort { get; set; }

        /// <summary>
        /// 父级
        /// </summary>
        public long? ParentId { get; set; }


    }
}