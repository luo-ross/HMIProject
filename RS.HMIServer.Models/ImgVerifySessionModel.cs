using RS.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    /// <summary>
    /// 验证会话实体
    /// </summary>
    public class ImgVerifySessionModel
    {
        /// <summary>
        /// 验证会话Id
        /// </summary>
        public string VerifySessionId { get; set; }

        /// <summary>
        /// 矩形框验证信息
        /// </summary>
        public RectModel Rect { get; set; }

        /// <summary>
        /// 拖拽按钮默认坐标位置X
        /// </summary>
        public int ImgBtnPositionX { get; set; }

        /// <summary>
        /// 拖拽按钮默认坐标位置Y
        /// </summary>
        public int ImgBtnPositionY { get; set; }

        /// <summary>
        /// 创建会话次数
        /// </summary>
        public int CreateCount { get; set; }

    }
}
