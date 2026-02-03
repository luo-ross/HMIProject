using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Behaviors
{
    /// <summary>
    /// 验证失败时自动获取焦点的行为
    /// 当控件的数据验证失败时，自动让该控件获得焦点
    /// </summary>
    public class ValidationFocusBehavior : Behavior<Control>
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.Register(
                nameof(IsEnabled),
                typeof(bool),
                typeof(ValidationFocusBehavior),
                new PropertyMetadata(true));

        /// <summary>
        /// 是否启用验证焦点功能。默认为 true
        /// </summary>
        public bool IsEnabled
        {
            get { return (bool)GetValue(IsEnabledProperty); }
            set { SetValue(IsEnabledProperty, value); }
        }

        public static readonly DependencyProperty SelectAllOnFocusProperty =
            DependencyProperty.Register(
                nameof(SelectAllOnFocus),
                typeof(bool),
                typeof(ValidationFocusBehavior),
                new PropertyMetadata(true));

        /// <summary>
        /// 获取焦点时是否全选文本（仅对 TextBox 有效）。默认为 true
        /// </summary>
        public bool SelectAllOnFocus
        {
            get { return (bool)GetValue(SelectAllOnFocusProperty); }
            set { SetValue(SelectAllOnFocusProperty, value); }
        }

        protected override void OnAttached()
        {
            base.OnAttached();
            Validation.AddErrorHandler(AssociatedObject, OnValidationError);
        }

        protected override void OnDetaching()
        {
            Validation.RemoveErrorHandler(AssociatedObject, OnValidationError);
            base.OnDetaching();
        }

        private void OnValidationError(object sender, ValidationErrorEventArgs e)
        {
            if (!IsEnabled)
            {
                return;
            }

            // 只在新错误添加时获取焦点，移除错误时不处理
            if (e.Action != ValidationErrorEventAction.Added)
            {
                return;
            }

            AssociatedObject.Focus();

            // 如果是 TextBox，全选文本
            if (SelectAllOnFocus && AssociatedObject is TextBox textBox)
            {
                textBox.SelectAll();
            }
        }
    }
}
