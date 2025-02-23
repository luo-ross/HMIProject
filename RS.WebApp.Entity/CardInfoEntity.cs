using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 客人证件信息
    /// </summary>
    public sealed class CardInfoEntity : BaseEntity
    {
        /// <summary>
        /// 证件类型
        /// </summary>
        public int? Type { get; set; }

        /// <summary>
        /// 证件号
        /// </summary>
        public string? CardNo { get; set; }

        /// <summary>
        /// 绑定客人资料
        /// </summary>
        public long? GuestId { get; set; }

    }
}