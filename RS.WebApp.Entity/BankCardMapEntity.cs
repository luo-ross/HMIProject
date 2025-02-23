using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 银行卡信息
    /// </summary>
    public sealed class BankCardMapEntity : BaseEntity
    {

        /// <summary>
        /// 付费绑定
        /// </summary>
        public long? PaymentId { get; set; }

        /// <summary>
        /// 银行卡号
        /// </summary>
        public string? CardNo { get; set; }

    }
}