using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 公司资料
    /// </summary>
    public sealed class CompanyProfileEntity : BaseEntity
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
        /// 统一社会信用代码
        /// </summary>
        public string? Code { get; set; }

        /// <summary>
        /// 法定代表人
        /// </summary>
        public string? LegalPerson { get; set; }

        /// <summary>
        /// 注册资本
        /// </summary>
        public decimal? RegisteredCapital { get; set; }

        /// <summary>
        /// 注册时间
        /// </summary>
        public long? RegisterTime { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 邮箱地址
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 公司网站
        /// </summary>
        public string? WebSite { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long? CreatorId { get; set; }

        /// <summary>
        /// 创建时间
        /// </summary>
        public long? CreateTime { get; set; }







    }
}