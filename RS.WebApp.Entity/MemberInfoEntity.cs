using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 客人会员信息
    /// </summary>
    public sealed class MemberInfoEntity : BaseEntity
    {

        /// <summary>
        /// 客人资料
        /// </summary>
        public long? GuestId { get; set; }

        /// <summary>
        /// 会员卡号
        /// </summary>
        public string? CardNo { get; set; }

        /// <summary>
        /// 会员等级
        /// </summary>
        public string? Level { get; set; }

        /// <summary>
        /// 会员类型
        /// </summary>
        public int? Type { get; set; }

    }
}