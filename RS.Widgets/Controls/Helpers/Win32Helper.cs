using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace RS.Widgets.Controls.Helpers
{
    public class Win32Helper
    {

        /// <summary>
        /// 获取指定窗口在屏幕坐标系中的位置。
        /// 支持根据FlowDirection处理从右到左的布局。
        /// </summary>
        /// <param name="windowIntPtr">窗口句柄</param>
        /// <param name="flowDirection">文本流向，影响窗口位置计算方式</param>
        /// <returns>窗口在屏幕上的坐标点</returns>
        public static Point GetWindowScreenLocation(IntPtr windowIntPtr, FlowDirection flowDirection)
        {
            System.Drawing.Point pOINT = default;
            HWND hWND = new HWND(windowIntPtr);
            if (flowDirection == FlowDirection.RightToLeft)
            {
                Ross.GetClientRect(hWND, out RECT rect);
                pOINT = new System.Drawing.Point(rect.right, rect.top);
            }
            Ross.ClientToScreen(hWND, ref pOINT);
            return new Point(pOINT.X, pOINT.Y);
        }

        /// <summary>
        /// 将屏幕坐标转换为指定窗口的相对坐标。
        /// 先调用GetWindowScreenLocation获取窗口位置，再进行坐标转换。
        /// </summary>
        /// <param name="windowIntPtr">窗口句柄</param>
        /// <param name="x">屏幕坐标X值</param>
        /// <param name="y">屏幕坐标Y值</param>
        /// <param name="flowDirection">文本流向，影响窗口位置计算方式</param>
        /// <returns>相对于窗口客户区的坐标点</returns>
        public static Point GetPointRelativeToWindow(IntPtr windowIntPtr, int x, int y, FlowDirection flowDirection)
        {
            var windowScreenLocation = GetWindowScreenLocation(windowIntPtr, flowDirection);
            return new Point(x - windowScreenLocation.X, y - windowScreenLocation.Y);
        }
    }
}
