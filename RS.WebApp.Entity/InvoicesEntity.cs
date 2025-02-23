using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 发票
    /// </summary>
    public sealed class InvoicesEntity : BaseEntity
    {

        public long? BillId { get; set; }

        /// <summary>
        /// 发票代码
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 发票号
        /// </summary>
        public string? Number { get; set; }

        /// <summary>
        /// 开票日期
        /// </summary>
        public long? Date { get; set; }

        /// <summary>
        /// 发票编号
        /// </summary>
        public string? Serial { get; set; }

        /// <summary>
        /// 校验码
        /// </summary>
        public string? CheckCode { get; set; }

        /// <summary>
        /// 收款人
        /// </summary>
        public long? PayeeId { get; set; }

        /// <summary>
        /// 复核人
        /// </summary>
        public long? ReviewerId { get; set; }

        /// <summary>
        /// 开票人
        /// </summary>
        public long? IssuedbyId { get; set; }



    }
}