﻿using RS.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Models
{
    public class ImgVerifyInitModel : ImgVerifyModel
    {
        /// <summary>
        /// 验证矩形框信息
        /// </summary>
        public RectModel Rect { get; set; }
    }
}
