using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RS.WPFApp.Models
{
    /// <summary>
    /// 矩形缩放调整事件枚举
    /// </summary>
    public enum RectResizeEvents
    {
        /// <summary>
        /// 调整左边框线
        /// </summary>
        ResizeLeftBorder,
        /// <summary>
        /// 调整右边框线
        /// </summary>
        ResizeRightBorder,
        /// <summary>
        /// 调整上边框线
        /// </summary>
        ResizeTopBorder,
        /// <summary>
        /// 调整下边框线
        /// </summary>
        ResizeBottomBorder,
        /// <summary>
        /// 调整左上角
        /// </summary>
        ResizeLeftTopCorner,
        /// <summary>
        /// 调整右上角
        /// </summary>
        ResizeRightTopCorner,
        /// <summary>
        /// 调整右下角
        /// </summary>
        ResizeRightBottomCorner,
        /// <summary>
        /// 调整左下角
        /// </summary>
        ResizeLeftBottomCorner,
        /// <summary>
        /// 移动整个矩形
        /// </summary>
        ResizeAll,
    }
}
