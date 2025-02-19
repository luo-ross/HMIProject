using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMI.Models.Widgets
{
    public enum LoadingType
    {
        /// <summary>
        /// 默认加载进度条
        /// </summary>
        ProgressBar,
        /// <summary>
        /// 单Icon旋转加载动画
        /// </summary>
        RotatingAnimation,
        /// <summary>
        /// 四周环绕加载动画
        /// </summary>
        BorderSurroundingAnimation
    }
}
