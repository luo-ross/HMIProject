using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 散客房价配置
    /// </summary>
    public sealed class IndividualPriceConfigEntity : BaseEntity
    {

        /// <summary>
        /// 日期
        /// </summary>
        public long? Date { get; set; }

        /// <summary>
        /// 房型 关联房型Code
        /// </summary>
        public string? RoomType { get; set; }

        /// <summary>
        /// 入账代码 就是房价
        /// </summary>
        public string? PostCode { get; set; }


    }
}