using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 房价信息
    /// </summary>
    public sealed class RoomRateInfoEntity : BaseEntity
    {
        /// <summary>
        /// 绑定预定号
        /// </summary>
        public long? ReservationId { get; set; }

        /// <summary>
        /// 绑定房型
        /// </summary>
        public string? RoomType { get; set; }

        /// <summary>
        /// 绑定入账代码
        /// </summary>

        public string? PostCode { get; set; }

        /// <summary>
        /// 折扣
        /// </summary>
        public double? Discount { get; set; }

        /// <summary>
        /// 实际金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 描述备注
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 折扣原因
        /// </summary>
        public string? Reason { get; set; }

    }
}