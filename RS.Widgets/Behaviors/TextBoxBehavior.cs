using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Behaviors
{
    /// <summary>
    /// TextBox 焦点管理行为，支持通过绑定控制焦点状态
    /// </summary>
    public class TextBoxBehavior : Behavior<TextBox>
    {
        public static readonly DependencyProperty IsFocusedProperty =
            DependencyProperty.Register(
                nameof(IsFocused),
                typeof(bool),
                typeof(TextBoxBehavior),
                new PropertyMetadata(false, OnIsFocusedChanged));

        /// <summary>
        /// TextBox 是否获得焦点
        /// </summary>
        public bool IsFocused
        {
            get { return (bool)GetValue(IsFocusedProperty); }
            set { SetValue(IsFocusedProperty, value); }
        }

        private static void OnIsFocusedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            TextBoxBehavior behavior = (TextBoxBehavior)d;
            if (behavior.AssociatedObject == null || !(bool)e.NewValue)
            {
                return;
            }

            behavior.AssociatedObject.Focus();
            behavior.AssociatedObject.SelectAll();
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            AssociatedObject.LostFocus += AssociatedObject_LostFocus;
        }

        protected override void OnDetaching()
        {
            AssociatedObject.LostFocus -= AssociatedObject_LostFocus;
            base.OnDetaching();
        }

        private void AssociatedObject_LostFocus(object sender, RoutedEventArgs e)
        {
            IsFocused = false;
        }
    }
}
