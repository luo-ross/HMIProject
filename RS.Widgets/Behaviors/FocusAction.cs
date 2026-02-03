using Microsoft.Xaml.Behaviors;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Behaviors
{
    /// <summary>
    /// 一个可用于在 XAML 中设置焦点的 TriggerAction
    /// </summary>
    public class FocusAction : TriggerAction<UIElement>
    {
        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register(
                nameof(TargetElement),
                typeof(UIElement),
                typeof(FocusAction),
                new PropertyMetadata(null));

        /// <summary>
        /// 要获取焦点的目标元素。如果为空，则使用 AssociatedObject
        /// </summary>
        public UIElement TargetElement
        {
            get { return (UIElement)GetValue(TargetElementProperty); }
            set { SetValue(TargetElementProperty, value); }
        }

        public static readonly DependencyProperty MoveCaretToEndProperty =
            DependencyProperty.Register(
                nameof(MoveCaretToEnd),
                typeof(bool),
                typeof(FocusAction),
                new PropertyMetadata(true));

        /// <summary>
        /// 当目标为 TextBox 时，是否将光标移动到末尾。默认为 true
        /// </summary>
        public bool MoveCaretToEnd
        {
            get { return (bool)GetValue(MoveCaretToEndProperty); }
            set { SetValue(MoveCaretToEndProperty, value); }
        }

        protected override void Invoke(object parameter)
        {
            UIElement element = TargetElement ?? AssociatedObject;
            if (element == null)
            {
                return;
            }

            element.Focus();

            // 如果是 TextBox，将光标移动到末尾
            if (MoveCaretToEnd && element is TextBox textBox && !string.IsNullOrEmpty(textBox.Text))
            {
                textBox.CaretIndex = textBox.Text.Length;
            }
        }
    }
}
