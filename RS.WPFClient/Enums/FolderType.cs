using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Enums
{

    public enum FolderType
    {
        /// <summary>
        /// 任何 
        /// </summary>
        Any,
        /// <summary>
        /// 收件箱
        /// </summary>
        Inbox,
        /// <summary>
        /// 已发送
        /// </summary>
        Sent,
        /// <summary>
        /// 草稿箱
        /// </summary>
        Drafts,
        /// <summary>
        /// 已删除
        /// </summary>
        Deleted,
        /// <summary>
        /// 群邮件
        /// </summary>
        GroupMail
    }
}
