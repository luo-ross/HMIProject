using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 公司认证
    /// </summary>
    public sealed class RealCompanyEntity : BaseEntity
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
        /// 统一社会信用代码也叫做纳税人识别号
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
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 网址
        /// </summary>
        public string? WebSite { get; set; }

        /// <summary>
        /// 公司地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 营业执照图片链接
        /// </summary>
        public string? LicenseLink { get; set; }


    }
}