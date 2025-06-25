using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

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
    }

}