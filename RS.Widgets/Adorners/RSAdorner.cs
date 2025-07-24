using RS.Widgets.Controls;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Adorners
{
    public class RSAdorner : Adorner
    {
        public Window ParentWin;
        public Point CurrentMouseWinPoint;
        public RSAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            Loaded += RSAdorner_Loaded;
            Unloaded += RSAdorner_Unloaded;
        }

        private void RSAdorner_Unloaded(object sender, RoutedEventArgs e)
        {
            if (ParentWin != null)
            {
                ParentWin.PreviewMouseLeftButtonUp -= ParentWin_PreviewMouseLeftButtonUp;
                ParentWin.PreviewMouseMove -= ParentWin_PreviewMouseMove;
            }
        }

        private void RSAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentWin = Window.GetWindow(this);
            this.CurrentMouseWinPoint = Mouse.GetPosition(this.ParentWin);
            if (this.ParentWin != null)
            {
                this.ParentWin.PreviewMouseLeftButtonUp += ParentWin_PreviewMouseLeftButtonUp;
                this.ParentWin.PreviewMouseMove += ParentWin_PreviewMouseMove;
            }
        }


        private void ParentWin_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            this.CurrentMouseWinPoint = e.GetPosition(this.ParentWin);
            this.OnParentWin_PreviewMouseMove(sender, e);
            this.InvalidateVisual();
        }

        private void ParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            this.OnParentWin_PreviewMouseLeftButtonUp(sender, e);
            var adornerLayer = Parent as AdornerLayer;
            adornerLayer?.Remove(this);
        }

        public virtual void OnParentWin_PreviewMouseMove(object sender, MouseEventArgs e)
        {

        }
        public virtual void OnParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {

        }


        public static RectArea GetRectArea(Rect rect, Point point)
        {
            // 计算矩形的三分之一宽度和高度
            double thirdWidth = rect.Width / 3;
            double thirdHeight = rect.Height / 3;

            // 确定点在水平方向的位置（0=左，1=中，2=右）
            int horizontalPos = point.X < rect.Left ? 0 :
                               point.X > rect.Right ? 2 :
                               point.X < rect.Left + thirdWidth ? 0 :
                               point.X < rect.Left + 2 * thirdWidth ? 1 : 2;

            // 确定点在垂直方向的位置（0=上，1=中，2=下）
            int verticalPos = point.Y < rect.Top ? 0 :
                             point.Y > rect.Bottom ? 2 :
                             point.Y < rect.Top + thirdHeight ? 0 :
                             point.Y < rect.Top + 2 * thirdHeight ? 1 : 2;

            // 根据水平和垂直位置确定区域
            return (RectArea)(verticalPos * 3 + horizontalPos);
        }


    }
}
