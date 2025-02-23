using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    /// <summary>
    /// 用户表
    /// </summary>
    public sealed class UserEntity : BaseEntity
    {

        /// <summary>
        /// 用户昵称
        /// </summary>
        public string? NickName { get; set; }

        /// <summary>
        /// 用户头像
        /// </summary>
        public string? UserPic { get; set; }

        /// <summary>
        /// 邮箱
        /// </summary>
        public string? Email { get; set; }

        /// <summary>
        /// 电话
        /// </summary>
        public string? Phone { get; set; }

        /// <summary>
        /// 关联实名认证
        /// </summary>
        public long? RealNameId { get; set; }
    }
}