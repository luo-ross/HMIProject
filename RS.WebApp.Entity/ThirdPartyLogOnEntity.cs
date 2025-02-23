using System;
using System.Collections.Generic;

namespace RS.WebApp.Entity
{

    public sealed class ThirdPartyLogOnEntity : BaseEntity
    {

        /// <summary>
        /// 平台类别
        /// </summary>
        public int? PlatformType { get; set; }

        /// <summary>
        /// 平台唯一标识
        /// </summary>
        public string OpenId { get; set; }

        /// <summary>
        /// 第三方平台昵称
        /// </summary>
        public string? NickName { get; set; }

        /// <summary>
        /// 第三方平台头像
        /// </summary>
        public string? Pic { get; set; }

        /// <summary>
        /// 是否禁用 默认启用
        /// </summary>
        public bool? IsEnable { get; set; }

        /// <summary>
        /// 绑定用户主键
        /// </summary>
        public long? UserId { get; set; }

    }
}