using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 电话信息
    /// </summary>
    public sealed class PhoneInfoEntity : BaseEntity
    {

        /// <summary>
        /// 国家电话代码
        /// </summary>
        public string? CountryCode { get; set; }

        /// <summary>
        /// 电话号码
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 客人资料主键
        /// </summary>
        public long? GuestId { get; set; }

    }
}