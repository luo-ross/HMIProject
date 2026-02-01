using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// 继承自 ToolBar 的自定义工具栏控件，支持自定义溢出按钮样式
    /// </summary>
    public class RSToolbar : ToolBar
    {
        static RSToolbar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSToolbar), new FrameworkPropertyMetadata(typeof(RSToolbar)));
        }

        /// <summary>
        /// 溢出按钮显示的内容
        /// </summary>
        public object OverflowButtonContent
        {
            get { return (object)GetValue(OverflowButtonContentProperty); }
            set { SetValue(OverflowButtonContentProperty, value); }
        }

        public static readonly DependencyProperty OverflowButtonContentProperty =
            DependencyProperty.Register(nameof(OverflowButtonContent), typeof(object), typeof(RSToolbar),
                new PropertyMetadata("更多"));

        /// <summary>
        /// 溢出面板的宽度
        /// </summary>
        public double OverflowPanelWidth
        {
            get { return (double)GetValue(OverflowPanelWidthProperty); }
            set { SetValue(OverflowPanelWidthProperty, value); }
        }

        public static readonly DependencyProperty OverflowPanelWidthProperty =
            DependencyProperty.Register(nameof(OverflowPanelWidth), typeof(double), typeof(RSToolbar),
                new PropertyMetadata(200.0));
    }
}
