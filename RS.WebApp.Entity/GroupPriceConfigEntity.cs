using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 团队价格配置
    /// </summary>
    public sealed class GroupPriceConfigEntity : BaseEntity
    {

        /// <summary>
        /// 房价日期
        /// </summary>
        public long? Date { get; set; }

        /// <summary>
        /// 房间类型的代码
        /// </summary>
        public string? RoomType { get; set; }

        /// <summary>
        /// 入账代码 就是房价
        /// </summary>
        public string? PostCode { get; set; }

        /// <summary>
        /// 所胡团队
        /// </summary>
        public long? GroupId { get; set; }

    }
}