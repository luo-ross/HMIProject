using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 角色和权限映射
    /// </summary>
    [PrimaryKey("RoleId", "PermissionId")]
    public sealed class RolePermissionMapEntity 
    {
        /// <summary>
        /// 角色主键
        /// </summary>
        [Key]
        public long RoleId { get; set; }

        /// <summary>
        /// 权限主键
        /// </summary>
        [Key]
        public long PermissionId { get; set; }

        /// <summary>
        /// 权限类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 是否可以创建
        /// </summary>
        public bool? C { get; set; }

        /// <summary>
        /// 是否可以读取
        /// </summary>
        public bool? R { get; set; }

        /// <summary>
        /// 是否可以更新
        /// </summary>
        public bool? U { get; set; }

        /// <summary>
        /// 是否可以删除
        /// </summary>
        public bool? D { get; set; }
    }
}