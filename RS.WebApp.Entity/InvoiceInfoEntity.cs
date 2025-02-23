using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 发票信息
    /// </summary>
    public sealed class InvoiceInfoEntity : BaseEntity
    {

        /// <summary>
        /// 公司资料
        /// </summary>
        public long? CompanyId { get; set; }

        /// <summary>
        /// 开户银行
        /// </summary>
        public string? Bank { get; set; }

        /// <summary>
        /// 开户账号
        /// </summary>
        public string? Account { get; set; }

    }
}