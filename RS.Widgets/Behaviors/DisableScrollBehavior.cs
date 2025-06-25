using Microsoft.Xaml.Behaviors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows;
using RS.Widgets.Controls;

namespace RS.Widgets.Behaviors
{
    public class DisableScrollBehavior : Behavior<DataGrid>
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(
                "IsEnabled",
                typeof(bool),
                typeof(DisableScrollBehavior),
                new PropertyMetadata(false, OnIsEnabledChanged));

        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var behavior = (DisableScrollBehavior)d;
            if ((bool)e.NewValue)
            {
                behavior.EnableScrollBlocking();
            }
            else
            {
                behavior.DisableScrollBlocking();
            }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            if (IsEnabled)
            {
                EnableScrollBlocking();
            }
        }

        protected override void OnDetaching()
        {
            DisableScrollBlocking();
            base.OnDetaching();
        }

        private void EnableScrollBlocking()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.PreviewMouseWheel += DataGrid_PreviewMouseWheel;
                AssociatedObject.PreviewKeyDown += DataGrid_PreviewKeyDown;
            }
        }

        private void DisableScrollBlocking()
        {
            if (AssociatedObject != null)
            {
                AssociatedObject.PreviewMouseWheel -= DataGrid_PreviewMouseWheel;
                AssociatedObject.PreviewKeyDown -= DataGrid_PreviewKeyDown;
            }
        }

        private void DataGrid_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
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

        private void DataGrid_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Up || e.Key == Key.Down ||
                e.Key == Key.PageUp || e.Key == Key.PageDown)
            {
                e.Handled = true;
            }
        }
    }
}
