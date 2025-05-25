using System;
using System.Collections.Generic;

namespace RS.HMIServer.Entity
{

    /// <summary>
    /// 用户登录信息
    /// </summary>
    public sealed class LogOnEntity : BaseEntity
    {
        

        /// <summary>
        /// 密码
        /// </summary>
        public string? Password { get; set; }

        /// <summary>
        /// 密码加盐
        /// </summary>
        public string? Salt { get; set; }

        /// <summary>
        /// 是否禁用
        /// </summary>
        public bool IsEnable { get; set; }

        /// <summary>
        /// 用户主键
        /// </summary>
        public long? UserId { get; set; }

    }
}