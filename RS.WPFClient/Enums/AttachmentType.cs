using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WPFClient.Enums
{

    public enum AttachmentType
    {
        /// <summary>
        /// 任何附件状态（不限制附件是否存在，包含有附件和无附件的全部内容）
        /// </summary>
        Any,

        /// <summary>
        /// 包含附件（仅包含带有附件的内容）
        /// </summary>
        IncludeAttachment,

        /// <summary>
        /// 不包含附件（仅包含未带有附件的内容）
        /// </summary>
        NotIncludeAttachment,
    }
}
