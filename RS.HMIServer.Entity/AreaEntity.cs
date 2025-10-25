using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{
    /// <summary>
    /// 省市区级联
    /// </summary>
    public sealed class AreaEntity : BaseEntity
    {
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 父级
        /// </summary>
        public string? ParentId { get; set; }

        /// <summary>
        /// 级别
        /// </summary>
        public byte? Level { get; set; }
    }
}