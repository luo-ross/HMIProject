using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 付费信息
    /// </summary>
    public sealed class PaymentsEntity : BaseEntity
    {

        /// <summary>
        /// 付费方式
        /// </summary>
        public int? Method { get; set; }

        /// <summary>
        /// 付费金额
        /// </summary>
        public decimal? Money { get; set; }

        /// <summary>
        /// 账单
        /// </summary>
        public long? BillId { get; set; }

        /// <summary>
        /// 备注说明
        /// </summary>
        public string? Description { get; set; }



    }
}