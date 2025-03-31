using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 国家信息
    /// </summary>
    public sealed class CountryEntity : BaseEntity
    {

        /// <summary>
        /// 中文名称
        /// </summary>
        public string? ChName { get; set; }

        /// <summary>
        /// 英文名称
        /// </summary>
        public string? EnName { get; set; }

        /// <summary>
        /// 缩写
        /// </summary>
        public string? Abbr { get; set; }

        /// <summary>
        /// 电话代码
        /// </summary>
        public string? PhoneCode { get; set; }
    }
}