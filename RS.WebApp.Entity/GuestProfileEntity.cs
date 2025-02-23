using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 客人资料
    /// </summary>
    public sealed class GuestProfileEntity : BaseEntity
    {

        /// <summary>
        /// 中文姓名
        /// </summary>
        public string? ChName { get; set; }

        /// <summary>
        /// 英文姓名
        /// </summary>
        public string? EnName { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public bool? Gender { get; set; }

        /// <summary>
        /// 出身日期
        /// </summary>
        public long? BirthDay { get; set; }

        /// <summary>
        /// 创建人
        /// </summary>
        public long?  CreatorId { get; set; }

        /// <summary>
        /// 创建日期
        /// </summary>
        public long? CreateTime { get; set; }

        /// <summary>
        /// 客人头像
        /// </summary>
        public string? GuestPic { get; set; }











    }
}