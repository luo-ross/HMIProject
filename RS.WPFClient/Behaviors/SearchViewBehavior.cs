using Microsoft.Xaml.Behaviors;
using RS.WPFClient.Views;
using System;
using System.Windows;
using System.Windows.Input;

namespace RS.WPFClient.Behaviors
{
    /// <summary>
    /// 搜索视图行为，处理点击外部区域和窗口失焦时隐藏搜索
    /// </summary>
    public class SearchViewBehavior : Behavior<SearchView>
    {
        private Window parentWindow;

        public static readonly DependencyProperty HideSearchCommandProperty =
            DependencyProperty.Register(
                nameof(HideSearchCommand),
                typeof(ICommand),
                typeof(SearchViewBehavior),
                new PropertyMetadata(null));

        /// <summary>
        /// 隐藏搜索的命令
        /// </summary>
        public ICommand HideSearchCommand
        {
            get { return (ICommand)GetValue(HideSearchCommandProperty); }
            set { SetValue(HideSearchCommandProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.Loaded += AssociatedObject_Loaded;
            AssociatedObject.Unloaded += AssociatedObject_Unloaded;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.Loaded -= AssociatedObject_Loaded;
            AssociatedObject.Unloaded -= AssociatedObject_Unloaded;
            UnsubscribeWindowEvents();
            base.OnDetaching();
        }

        private void AssociatedObject_Loaded(object sender, RoutedEventArgs e)
        {
            parentWindow = Window.GetWindow(AssociatedObject);
            if (parentWindow != null)
            {
                parentWindow.PreviewMouseLeftButtonUp += Window_MouseLeftButtonUp;
                parentWindow.Deactivated += Window_Deactivated;
            }
        }

        private void AssociatedObject_Unloaded(object sender, RoutedEventArgs e)
        {
            UnsubscribeWindowEvents();
        }

        private void UnsubscribeWindowEvents()
        {
            if (parentWindow != null)
            {
                parentWindow.PreviewMouseLeftButtonUp -= Window_MouseLeftButtonUp;
                parentWindow.Deactivated -= Window_Deactivated;
                parentWindow = null;
            }
        }

        private void Window_Deactivated(object sender, EventArgs e)
        {
            HideSearchCommand?.Execute(null);
        }

        private void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.Source == AssociatedObject)
            {
                return;
            }
            HideSearchCommand?.Execute(null);
        }
    }
}
