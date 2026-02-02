using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// 垂直排列的 ToolBar 溢出面板
    /// </summary>
    public class RSToolBarOverflowPanel : ToolBarOverflowPanel
    {
        /// <summary>
        /// 重写测量方法，实现垂直排列
        /// </summary>
        protected override Size MeasureOverride(Size constraint)
        {
            // 先调用基类方法，确保子元素被正确收集
            base.MeasureOverride(constraint);

            // 如果用户设置了 WrapWidth，使用它作为宽度
            double panelWidth = this.WrapWidth > 0 && !double.IsNaN(this.WrapWidth) && !double.IsInfinity(this.WrapWidth)
                ? this.WrapWidth
                : 0;

            double maxWidth = panelWidth;
            double totalHeight = 0;

            UIElementCollection children = this.InternalChildren;
            int count = children.Count;

            for (int i = 0; i < count; i++)
            {
                UIElement child = children[i];
                child.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
                Size desiredSize = child.DesiredSize;

                // 垂直堆叠：累加高度，取最大宽度
                maxWidth = Math.Max(maxWidth, desiredSize.Width);
                totalHeight += desiredSize.Height;
            }

            return new Size(maxWidth, totalHeight);
        }

        /// <summary>
        /// 重写排列方法，实现垂直排列
        /// </summary>
        protected override Size ArrangeOverride(Size arrangeBounds)
        {
            double currentY = 0;
            double maxWidth = 0;

            // 如果用户设置了 WrapWidth，使用它作为宽度
            double panelWidth = this.WrapWidth > 0 && !double.IsNaN(this.WrapWidth) && !double.IsInfinity(this.WrapWidth)
                ? this.WrapWidth
                : arrangeBounds.Width;

            UIElementCollection children = this.Children;
            int count = children.Count;

            for (int i = 0; i < count; i++)
            {
                UIElement child = children[i];
                Size desiredSize = child.DesiredSize;

                // 垂直排列：每个元素占满宽度，依次向下排列
                child.Arrange(new Rect(0, currentY, panelWidth, desiredSize.Height));

                currentY += desiredSize.Height;
                maxWidth = Math.Max(maxWidth, desiredSize.Width);
            }

            return new Size(Math.Max(maxWidth, panelWidth), currentY);
        }
    }
}
