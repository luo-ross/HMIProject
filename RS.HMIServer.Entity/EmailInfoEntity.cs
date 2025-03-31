using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 邮箱绑定
    /// </summary>
    public sealed class EmailInfoEntity : BaseEntity
    {
        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 客人资料主键
        /// </summary>
        public long? GuestId { get; set; }

    }
}