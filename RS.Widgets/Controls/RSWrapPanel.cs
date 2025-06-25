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
        public RSWrapPanel()
        {
            //this.SizeChanged += RSWrapPanel_SizeChanged;
        }


        private void RSWrapPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateViusalItemWidth();
        }


        [Description("最小内容宽度")]
        public GridLength MinItemWidth
        {
            get { return (GridLength)GetValue(MinItemWidthProperty); }
            set { SetValue(MinItemWidthProperty, value); }
        }

        public static readonly DependencyProperty MinItemWidthProperty =
            DependencyProperty.Register("MinItemWidth", typeof(GridLength), typeof(RSWrapPanel), new PropertyMetadata(new GridLength(200)));



        [Description("最大内容宽度")]
        public GridLength MaxItemWidth
        {
            get { return (GridLength)GetValue(MaxItemWidthProperty); }
            set { SetValue(MaxItemWidthProperty, value); }
        }
        public static readonly DependencyProperty MaxItemWidthProperty =
            DependencyProperty.Register("MaxItemWidth", typeof(GridLength), typeof(RSWrapPanel), new PropertyMetadata(GridLength.Auto));



        [Description("一行显示几个")]
        public int Cols
        {
            get { return (int)GetValue(ColsProperty); }
            set { SetValue(ColsProperty, value); }
        }

        public static readonly DependencyProperty ColsProperty =
            DependencyProperty.Register("Cols", typeof(int), typeof(RSWrapPanel), new PropertyMetadata(0));



        protected override void OnVisualChildrenChanged(DependencyObject visualAdded, DependencyObject visualRemoved)
        {
            base.OnVisualChildrenChanged(visualAdded, visualRemoved);
            //this.UpdateViusalItemWidth();
        }


        private void UpdateViusalItemWidth()
        {
            if (this.ActualWidth == 0)
            {
                return;
            }


            var count = this.Children.Count;
            if (count == 0)
            {
                return;
            }
            bool isAutoCaculateItemWidth = true;
            if (this.Cols != 0)
            {
                isAutoCaculateItemWidth = false;
            }
            double itemWidth = 0;
            var actualWidth = this.ActualWidth;
            if (isAutoCaculateItemWidth)
            {
                itemWidth = actualWidth / count;
                if (this.MaxItemWidth != GridLength.Auto)
                {
                    itemWidth = Math.Min(this.MaxItemWidth.Value, itemWidth);
                }
                itemWidth = Math.Max(this.MinItemWidth.Value, itemWidth);
            }
            else
            {
                itemWidth = actualWidth / this.Cols;
            }

            var actualCount = Math.Floor(actualWidth / itemWidth);
            itemWidth = actualWidth / actualCount;
            //这里需要减去一个像素，暂时解决宽度不能自适应的问题
            this.ItemWidth = Math.Min(this.ActualWidth, itemWidth) - 1;

        }
    }
}
