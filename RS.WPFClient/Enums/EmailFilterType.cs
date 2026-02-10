using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Enums
{
    /// <summary>
    /// 收件筛选类型枚举
    /// 用于对收件箱邮件进行不同维度的筛选
    /// </summary>
    public enum EmailFilterType
    {

        /// <summary>
        /// 全读邮件
        /// </summary>
        AllRead,

        /// <summary>
        /// 未读邮件
        /// </summary>
        Unread,

        /// <summary>
        /// 包含附件的邮件
        /// </summary>
        WithAttachment,

        /// <summary>
        /// 来自联系人的邮件
        /// </summary>
        FromContact
    }
}
