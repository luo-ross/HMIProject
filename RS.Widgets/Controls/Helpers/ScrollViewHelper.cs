using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using RS.Widgets.Commons;

namespace RS.Widgets.Controls
{
    public static class ScrollViewHelper
    {


        public static readonly DependencyProperty IsShowLineCommandProperty =
         DependencyProperty.RegisterAttached(
             "IsShowLineCommand",
             typeof(bool),
             typeof(ScrollViewHelper),
             new PropertyMetadata(false));

        public static bool GetIsShowLineCommand(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsShowLineCommandProperty);
        }

        public static void SetIsShowLineCommand(DependencyObject obj, bool value)
        {
            obj.SetValue(IsShowLineCommandProperty, value);
        }





        public static readonly DependencyProperty DisableScrollProperty =
         DependencyProperty.RegisterAttached(
             "DisableScroll",
             typeof(bool),
             typeof(ScrollViewHelper),
             new PropertyMetadata(false, OnDisableScrollChanged));

        public static bool GetDisableScroll(DependencyObject obj)
        {
            return (bool)obj.GetValue(DisableScrollProperty);
        }

        public static void SetDisableScroll(DependencyObject obj, bool value)
        {
            obj.SetValue(DisableScrollProperty, value);
        }

        private static void OnDisableScrollChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is DataGrid dataGrid)
            {
                if ((bool)e.NewValue)
                {
                    dataGrid.PreviewMouseWheel += DataGrid_PreviewMouseWheel;
                    dataGrid.PreviewKeyDown += DataGrid_PreviewKeyDown;
                }
                else
                {
                    dataGrid.PreviewMouseWheel -= DataGrid_PreviewMouseWheel;
                    dataGrid.PreviewKeyDown -= DataGrid_PreviewKeyDown;
                }
            }
        }

        private static void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            e.Handled = true;
            var scrollViewer = ((DependencyObject)sender).TryFindParent<ScrollViewer>();
            var newEventArgs = new MouseWheelEventArgs(e.MouseDevice, e.Timestamp, e.Delta)
            {
                RoutedEvent = UIElement.MouseWheelEvent,
                Source = scrollViewer
            };
            scrollViewer?.RaiseEvent(newEventArgs);
        }

        private static void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                e.Handled = true;
            }
        }


        #region 附加属性：VerticalScrollOffset
        public static readonly DependencyProperty VerticalScrollOffsetProperty =
            DependencyProperty.RegisterAttached(
                "VerticalScrollOffset",
                typeof(double),
                typeof(ScrollViewHelper),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    CoerceVerticalScrollOffset));

        public static double GetVerticalScrollOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(VerticalScrollOffsetProperty);
        }

        public static void SetVerticalScrollOffset(DependencyObject obj, double value)
        {
            obj.SetValue(VerticalScrollOffsetProperty, value);
        }

        private static object CoerceVerticalScrollOffset(DependencyObject d, object value)
        {
            if (d is not DataGrid dataGrid) return 0;
            var scrollViewer = dataGrid.GetScrollViewer();
            if (scrollViewer == null) return 0;

            // 限制偏移量范围（0 ~ 最大滚动值）
            double val = (double)value;
            return Math.Clamp(val, 0, scrollViewer.ScrollableHeight);
        }
        #endregion

        #region 附加属性：HorizontalScrollOffset
        public static readonly DependencyProperty HorizontalScrollOffsetProperty =
            DependencyProperty.RegisterAttached(
                "HorizontalScrollOffset",
                typeof(double),
                typeof(ScrollViewHelper),
                new FrameworkPropertyMetadata(
                    0.0,
                    FrameworkPropertyMetadataOptions.BindsTwoWayByDefault,
                    null,
                    CoerceHorizontalScrollOffset));

        public static double GetHorizontalScrollOffset(DependencyObject obj)
        {
            return (double)obj.GetValue(HorizontalScrollOffsetProperty);
        }

        public static void SetHorizontalScrollOffset(DependencyObject obj, double value)
        {
            obj.SetValue(HorizontalScrollOffsetProperty, value);
        }

        private static object CoerceHorizontalScrollOffset(DependencyObject d, object value)
        {
            if (d is not DataGrid dataGrid) return 0;
            var scrollViewer = dataGrid.GetScrollViewer();
            if (scrollViewer == null) return 0;

            double val = (double)value;
            return Math.Clamp(val, 0, scrollViewer.ScrollableWidth);
        }
        #endregion

        #region 内部逻辑：监听ScrollChanged并更新附加属性
        private static readonly DependencyProperty ScrollViewerTrackerProperty =
            DependencyProperty.RegisterAttached(
                "ScrollViewerTracker",
                typeof(ScrollViewer),
                typeof(ScrollViewHelper),
                new PropertyMetadata(null, OnScrollViewerTrackerChanged));

        private static void OnScrollViewerTrackerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ScrollViewer oldSv)
            {
                oldSv.ScrollChanged -= OnScrollChanged;
            }

            if (e.NewValue is ScrollViewer newSv)
            {
                newSv.ScrollChanged += OnScrollChanged;
                // 初始化偏移量
                UpdateOffsetProperties(d as DataGrid, newSv);
            }
        }

        private static void OnScrollChanged(object sender, ScrollChangedEventArgs e)
        {
            if (sender is not ScrollViewer sv || sv.TemplatedParent is not DataGrid dg) return;
            UpdateOffsetProperties(dg, sv);
        }

        private static void UpdateOffsetProperties(DataGrid dg, ScrollViewer sv)
        {
            // 更新附加属性（自动触发绑定通知）
            SetVerticalScrollOffset(dg, sv.VerticalOffset);
            SetHorizontalScrollOffset(dg, sv.HorizontalOffset);
        }
      
        #endregion

        #region  IsMonitoring 开关属性
        public static readonly DependencyProperty IsMonitoringProperty =
            DependencyProperty.RegisterAttached(
                "IsMonitoring",
                typeof(bool),
                typeof(ScrollViewHelper),
                new PropertyMetadata(false, OnIsMonitoringChanged));

        public static bool GetIsMonitoring(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsMonitoringProperty);
        }

        public static void SetIsMonitoring(DependencyObject obj, bool value)
        {
            obj.SetValue(IsMonitoringProperty, value);
        }

        private static void OnIsMonitoringChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not DataGrid dataGrid) return;
            bool isMonitoring = (bool)e.NewValue;
            if (isMonitoring)
            {
                dataGrid.Loaded += DataGrid_Loaded;
            }
            else
            {
                dataGrid.Loaded -= DataGrid_Loaded;
            }
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is DataGrid dataGrid)
            {
                var scrollViewer = dataGrid.GetScrollViewer();
                dataGrid.SetValue(ScrollViewerTrackerProperty, scrollViewer);
            }
        }
        #endregion





    }

}