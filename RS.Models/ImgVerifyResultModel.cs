using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Models
{
    public class ImgVerifyResultModel
    {
        /// <summary>
        /// 验证矩形框
        /// </summary>
        public RectModel Verify { get; set; }

        /// <summary>
        /// 验证会话Id
        /// </summary>
        public string VerifySessionId { get; set; }

    }
}
