using RS.Widgets.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSWrapPanel : WrapPanel
    {
        [Description("一行显示几个")]
        public int Cols
        {
            get { return (int)GetValue(ColsProperty); }
            set { SetValue(ColsProperty, value); }
        }

        public static readonly DependencyProperty ColsProperty =
            DependencyProperty.Register("Cols", typeof(int), typeof(RSWrapPanel), new PropertyMetadata(0, OnColsChanged));

        private static void OnColsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var panel = d as RSWrapPanel;
            panel?.InvalidateMeasure();
        }

        protected override Size MeasureOverride(Size constraint)
        {
            double itemWidth = this.ItemWidth;
            bool hasItemWidth = !Double.IsNaN(itemWidth);

            if (Orientation == Orientation.Horizontal && hasItemWidth && constraint.Width != double.PositiveInfinity)
            {
                double availableWidth = constraint.Width;
                int n = Math.Max(1, (int)Math.Floor(availableWidth / itemWidth));
                double actualWidth = availableWidth / n;

                double totalHeight = 0;
                double maxHeight = 0;
                int colIndex = 0;

                foreach (UIElement child in InternalChildren)
                {
                    if (child == null) continue;
                    child.Measure(new Size(actualWidth, constraint.Height));
                    maxHeight = Math.Max(maxHeight, child.DesiredSize.Height);
                    colIndex++;
                    if (colIndex >= n)
                    {
                        totalHeight += maxHeight;
                        maxHeight = 0;
                        colIndex = 0;
                    }
                }
                if (colIndex > 0) { 
                    totalHeight += maxHeight;
                }
                return new Size(availableWidth, totalHeight);
            }
            else
            {
                return base.MeasureOverride(constraint);
            }
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double itemWidth = this.ItemWidth;
            bool hasItemWidth = !Double.IsNaN(itemWidth);

            if (Orientation == Orientation.Horizontal && hasItemWidth && finalSize.Width != double.PositiveInfinity)
            {
                double availableWidth = finalSize.Width;
                int n = Math.Max(1, (int)Math.Floor(availableWidth / itemWidth));
                double actualWidth = availableWidth / n;

                double x = 0, y = 0;
                double maxHeight = 0;
                int colIndex = 0;

                foreach (UIElement child in InternalChildren)
                {
                    if (child == null) continue;
                    double height = child.DesiredSize.Height;
                    child.Arrange(new Rect(x, y, actualWidth, height));
                    maxHeight = Math.Max(maxHeight, height);
                    colIndex++;
                    x += actualWidth;

                    if (colIndex >= n)
                    {
                        y += maxHeight;
                        x = 0;
                        colIndex = 0;
                        maxHeight = 0;
                    }
                }
                return finalSize;
            }
            else
            {
                return base.ArrangeOverride(finalSize);
            }
        }
      
    }
}
