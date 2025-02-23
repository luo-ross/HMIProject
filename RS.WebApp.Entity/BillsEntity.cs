using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 个人账单数据
    /// </summary>
    public sealed class BillsEntity : BaseEntity
    {

        /// <summary>
        /// 预定绑定
        /// </summary>
        public long? ReservationId { get; set; }

        /// <summary>
        /// 入账代码
        /// </summary>
        public string? PostCode { get; set; }

        /// <summary>
        /// 描述
        /// </summary>
        public string? Description { get; set; }

        /// <summary>
        /// 金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 所属分组
        /// </summary>
        public int? Group { get; set; }

        /// <summary>
        /// 入账日期
        /// </summary>
        public long? PostDate { get; set; }




    }
}