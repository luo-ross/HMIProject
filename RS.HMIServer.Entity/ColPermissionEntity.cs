﻿using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 数据表列权限
    /// </summary>
    public sealed class ColPermissionEntity : BaseEntity
    {

        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 排序
        /// </summary>
        public int? Sort { get; set; }


    }
}