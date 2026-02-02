using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace RS.Widgets.Commons
{
    public static class DataGridScrollHelper
    {
        /// <summary>
        /// 获取DataGrid内部的ScrollViewer
        /// </summary>
        /// <param name="dataGrid">目标DataGrid</param>
        /// <returns>内部ScrollViewer（null表示未找到）</returns>
        public static ScrollViewer GetScrollViewer(this DataGrid dataGrid)
        {
            if (dataGrid == null) throw new ArgumentNullException(nameof(dataGrid));

            // 等待模板加载完成
            if (!dataGrid.IsLoaded)
            {
                dataGrid.Dispatcher.Invoke(() => { }, System.Windows.Threading.DispatcherPriority.Loaded);
            }
            return dataGrid.FindChild<ScrollViewer>();
        }

        /// <summary>
        /// 获取DataGrid的垂直滚动偏移量
        /// </summary>
        /// <param name="dataGrid">目标DataGrid</param>
        /// <returns>垂直偏移量（double）</returns>
        public static double GetVerticalOffset(this DataGrid dataGrid)
        {
            var scrollViewer = dataGrid.GetScrollViewer();
            return scrollViewer?.VerticalOffset ?? 0;
        }

        /// <summary>
        /// 获取DataGrid的水平滚动偏移量
        /// </summary>
        /// <param name="dataGrid">目标DataGrid</param>
        /// <returns>水平偏移量（double）</returns>
        public static double GetHorizontalOffset(this DataGrid dataGrid)
        {
            var scrollViewer = dataGrid.GetScrollViewer();
            return scrollViewer?.HorizontalOffset ?? 0;
        }

        /// <summary>
        /// 监听DataGrid滚动偏移量变化
        /// </summary>
        /// <param name="dataGrid">目标DataGrid</param>
        /// <param name="onScrollChanged">偏移量变化回调</param>
        public static void ListenScrollOffset(this DataGrid dataGrid, Action<double, double> onScrollChanged)
        {
            if (dataGrid == null) throw new ArgumentNullException(nameof(dataGrid));
            if (onScrollChanged == null) throw new ArgumentNullException(nameof(onScrollChanged));

            var scrollViewer = dataGrid.GetScrollViewer();
            if (scrollViewer != null)
            {
                scrollViewer.ScrollChanged += (s, e) =>
                {
                    onScrollChanged.Invoke(scrollViewer.VerticalOffset, scrollViewer.HorizontalOffset);
                };
            }
        }
    }
}
