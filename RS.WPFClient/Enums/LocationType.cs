using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Enums
{

    public enum LocationType
    {
        /// <summary>
        /// 任何位置（不限制具体位置，涵盖所有可能的位置范围）
        /// </summary>
        Any,

        /// <summary>
        /// 主题（如邮件主题、消息标题等内容的主题部分）
        /// </summary>
        Subject,

        /// <summary>
        /// 邮件正文（邮件内容的文本主体部分）
        /// </summary>
        EmailBody,

        /// <summary>
        /// 附件名称（附加文件的文件名部分）
        /// </summary>
        AttachmentName
    }
}
