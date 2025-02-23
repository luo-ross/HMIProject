using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 公司房价配置
    /// </summary>
    public sealed class CompanyPriceConfigEntity : BaseEntity
    {

        /// <summary>
        /// 房价日期
        /// </summary>
        public long? Date { get; set; }

        /// <summary>
        /// 房型代码
        /// </summary>
        public string? RoomType { get; set; }

        /// <summary>
        /// 入账代码 就是房价
        /// </summary>
        public string? PostCode { get; set; }

        /// <summary>
        /// 公司资料
        /// </summary>
        public long? CompanyId { get; set; }



    }
}