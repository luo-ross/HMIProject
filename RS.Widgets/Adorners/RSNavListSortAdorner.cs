using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Adorners
{
    public class RSNavListSortAdorner : RSDragAdorner
    {
        private RSNavItem RSNavItem { get; set; }
        private RSNavigate RSNavigate { get; set; }

        private int DragNavItemIndex;
        private NavigateModel DragNavigateModel;
        private RSNavItem RSNavItemRelative;

        public RSNavListSortAdorner(RSNavigate rsNavigate, RSNavItem rsNavItem) : base(rsNavItem)
        {
            this.RSNavItem = rsNavItem;
            this.RSNavigate = rsNavigate;
            //获取基础数据
            this.DragNavigateModel = this.RSNavItem.DataContext as NavigateModel;
            this.DragNavItemIndex = this.RSNavigate.NavigateModelList.IndexOf(this.DragNavigateModel);

            this.DrawPen = new Pen()
            {
                Brush = Brushes.DimGray,
                Thickness = 2,
                //DashStyle = new DashStyle(new double[] { 4, 4 }, 0)
            };
        }


        public override void OnParentWin_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            base.OnParentWin_PreviewMouseMove(sender, e);
            var rsNavList = RSNavigate.GetRSNavList();

            var position = e.GetPosition(rsNavList);
            var rSNavItem = GetUIElementUnderMouse<RSNavItem>(rsNavList, position);
            if (rSNavItem != null)
            {
                this.RSNavItemRelative = rSNavItem;
            }
        }

        public override void OnParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (this.DragNavigateModel == null)
            {
                return;
            }

            this.CurrentMousePoint = e.GetPosition(this);
            RSNavigate.NavigateModelList.Remove(this.DragNavigateModel);
            int indexInsertShould = this.DragNavItemIndex;
            if (this.RSNavItem != null
                && this.RSNavItemRelative != null
                && !this.RSNavItem.Equals(this.RSNavItemRelative))
            {

                var navigateModelRelative = this.RSNavItemRelative.DataContext as NavigateModel;
                indexInsertShould = RSNavigate.NavigateModelList.IndexOf(navigateModelRelative);
            }

            indexInsertShould = Math.Max(indexInsertShould, 0);
            indexInsertShould = Math.Min(indexInsertShould, RSNavigate.NavigateModelList.Count);
            RSNavigate.NavigateModelList.Insert(indexInsertShould, this.DragNavigateModel);
            this.DragNavigateModel = null;
            this.RSNavItemRelative = null;

            base.OnParentWin_PreviewMouseLeftButtonUp(sender, e);
        }




        public Pen DrawPen
        {
            get { return (Pen)GetValue(DrawPenProperty); }
            set { SetValue(DrawPenProperty, value); }
        }

        public static readonly DependencyProperty DrawPenProperty =
            DependencyProperty.Register("DrawPen", typeof(Pen), typeof(RSNavListSortAdorner), new PropertyMetadata(null));



        protected override void OnRender(DrawingContext drawingContext)
        {

            var adornedElement = this.AdornedElement as FrameworkElement;
            var actualWidth = adornedElement.ActualWidth;
            var actualHeight = adornedElement.ActualHeight;
            VisualBrush visualBrush = new VisualBrush(adornedElement);
            Rect rect = new Rect(0, 0, actualWidth, actualHeight);
            if (this.CurrentMousePoint != default)
            {
                rect.X = this.CurrentMousePoint.X + 20;
                rect.Y = this.CurrentMousePoint.Y - 15;
            }
            drawingContext.DrawRectangle(visualBrush, new Pen(), rect);


            if (this.RSNavItemRelative == null)
            {
                return;
            }
            var rsNavItemRelative = this.RSNavItemRelative;


            //var rsNavList = RSNavigate.GetRSNavList();
            //var rsNavListActualWidth = rsNavList.ActualWidth;
            //var rsNavListPoint = rsNavList.TransformToVisual(this).Transform(new Point(0, 0));
            //var rsNavItemRelativePoint = this.RSNavItemRelative.TransformToVisual(this).Transform(new Point(0, 0));
            //Point startPoint = new Point((int)rsNavListPoint.X + 4, (int)rsNavItemRelativePoint.Y - 2);
            //Point endPoint = startPoint + new Vector((int)rsNavListActualWidth - 4, 0);

            //var guidelines = new GuidelineSet();
            //guidelines.GuidelinesX.Add(Math.Round(startPoint.X));
            //guidelines.GuidelinesY.Add(Math.Round(startPoint.Y));
            //guidelines.GuidelinesX.Add(Math.Round(endPoint.X));
            //guidelines.GuidelinesY.Add(Math.Round(endPoint.Y));

            //drawingContext.PushGuidelineSet(guidelines);
            //// 画参考线
            //drawingContext.DrawLine(this.DrawPen, startPoint, endPoint);
           
            //drawingContext.Pop();



            var rsNavList = RSNavigate.GetRSNavList();
            var rsNavListActualWidth = rsNavList.ActualWidth;
            var rsNavListPoint = rsNavList.TransformToVisual(this).Transform(new Point(0, 0));
            var rsNavItemRelativePoint = this.RSNavItemRelative.TransformToVisual(this).Transform(new Point(0, 0));

            // 计算起始点和结束点
            Point startPoint = new Point((int)rsNavListPoint.X + 4, (int)rsNavItemRelativePoint.Y - 2);
            Point endPoint = startPoint + new Vector((int)rsNavListActualWidth - 4, 0);

            // 计算垂直线的高度
            double verticalLineHeight = 8; // 可以根据需要调整

            // 定义三条线的起点和终点
            Point leftVerticalStart = new Point(startPoint.X, startPoint.Y - verticalLineHeight / 2);
            Point leftVerticalEnd = new Point(startPoint.X, startPoint.Y + verticalLineHeight / 2);

            Point rightVerticalStart = new Point(endPoint.X, endPoint.Y - verticalLineHeight / 2);
            Point rightVerticalEnd = new Point(endPoint.X, endPoint.Y + verticalLineHeight / 2);

            Point horizontalStart = startPoint;
            Point horizontalEnd = endPoint;

            // 创建 GuidelineSet 进行像素对齐
            var guidelines = new GuidelineSet();
            guidelines.GuidelinesX.Add(Math.Round(leftVerticalStart.X));
            guidelines.GuidelinesY.Add(Math.Round(leftVerticalStart.Y));
            guidelines.GuidelinesX.Add(Math.Round(leftVerticalEnd.X));
            guidelines.GuidelinesY.Add(Math.Round(leftVerticalEnd.Y));
            guidelines.GuidelinesX.Add(Math.Round(rightVerticalStart.X));
            guidelines.GuidelinesY.Add(Math.Round(rightVerticalStart.Y));
            guidelines.GuidelinesX.Add(Math.Round(rightVerticalEnd.X));
            guidelines.GuidelinesY.Add(Math.Round(rightVerticalEnd.Y));
            guidelines.GuidelinesX.Add(Math.Round(horizontalStart.X));
            guidelines.GuidelinesY.Add(Math.Round(horizontalStart.Y));
            guidelines.GuidelinesX.Add(Math.Round(horizontalEnd.X));
            guidelines.GuidelinesY.Add(Math.Round(horizontalEnd.Y));

            drawingContext.PushGuidelineSet(guidelines);

            // 绘制三条线：|---------------|
            drawingContext.DrawLine(this.DrawPen, leftVerticalStart, leftVerticalEnd);    // 左垂直线
            drawingContext.DrawLine(this.DrawPen, horizontalStart, horizontalEnd);        // 水平线
            drawingContext.DrawLine(this.DrawPen, rightVerticalStart, rightVerticalEnd);  // 右垂直线

            drawingContext.Pop();
        }

    }
}
