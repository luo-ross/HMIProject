using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 实名认证
    /// </summary>
    public sealed class RealNameEntity : BaseEntity
    {

        /// <summary>
        /// 姓名
        /// </summary>
        public string? Name { get; set; }

        /// <summary>
        /// 性别
        /// </summary>
        public bool? Gender { get; set; }

        /// <summary>
        /// 出身日期
        /// </summary>
        public long? BirthDay { get; set; }

        /// <summary>
        /// 地址
        /// </summary>
        public string? Address { get; set; }

        /// <summary>
        /// 身份证
        /// </summary>
        public string? IDNo { get; set; }

        /// <summary>
        /// 身份证正面地址
        /// </summary>
        public string? FrontLink { get; set; }

        /// <summary>
        /// 身份证反面地址
        /// </summary>
        public string? BackLink { get; set; }


    }
}