using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    public class ImgVerifyModel
    {
        /// <summary>
        /// 验证会话Id
        /// </summary>
        public string VerifySessionId { get; set; }

        /// <summary>
        /// 验证图像数据二合一
        /// </summary>
        public byte[] ImgBuffer { get; set; }

        /// <summary>
        /// 背景图宽度
        /// </summary>
        public int ImgWidth { get; set; }

        /// <summary>
        /// 背景图高度
        /// </summary>
        public int ImgHeight { get; set; }

        /// <summary>
        /// 拖拽背景图片宽度
        /// </summary>
        public int IconWidth { get; set; }

        /// <summary>
        /// 拖拽背景图片高度
        /// </summary>
        public int IconHeight { get; set; }

        /// <summary>
        /// 拖拽按钮默认坐标位置X
        /// </summary>
        public int ImgBtnPositionX { get; set; }

        /// <summary>
        /// 拖拽按钮默认坐标位置Y
        /// </summary>
        public int ImgBtnPositionY { get; set; }

    }
}
