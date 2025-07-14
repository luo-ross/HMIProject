using Microsoft.Extensions.Logging;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Controls;
using RS.Widgets.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
        private RSNavItem RSNavItemRelative;
        private RSNavList RSNavListRelative;
        private RSNavListSortInfo RSNavListSortInfo { get; set; }

        public RSNavListSortAdorner(RSNavItem rsNavItem) : base(rsNavItem)
        {
            this.RSNavListSortInfo = new RSNavListSortInfo()
            {
                RSNavItem = rsNavItem,
                RSNavList = rsNavItem.TryFindParent<RSNavList>()
            };
            this.DrawPen = new Pen()
            {
                Brush = Brushes.DimGray,
                Thickness = 2,
                //DashStyle = new DashStyle(new double[] { 4, 4 }, 0)
            };
        }

        public Pen DrawPen
        {
            get { return (Pen)GetValue(DrawPenProperty); }
            set { SetValue(DrawPenProperty, value); }
        }

        public static readonly DependencyProperty DrawPenProperty =
            DependencyProperty.Register("DrawPen", typeof(Pen), typeof(RSNavListSortAdorner), new PropertyMetadata(null));


        public override void OnParentWin_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            base.OnParentWin_PreviewMouseMove(sender, e);

            this.RSNavListRelative = GetUIElementUnderMouse<RSNavList>(this.ParentWin, this.CurrentMouseWinPoint);
            if (this.RSNavListRelative == null)
            {
                return;
            }

            var position = e.GetPosition(this.RSNavListRelative);
            var rSNavItem = GetUIElementUnderMouse<RSNavItem>(this.RSNavListRelative, position);
            if (rSNavItem != null)
            {
                this.RSNavItemRelative = rSNavItem;
            }
        }

        public override void OnParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dragNavItemDataContext = this.RSNavListSortInfo.RSNavItem.DataContext;
            if (dragNavItemDataContext == null)
            {
                return;
            }

            var dataList = this.RSNavListSortInfo.RSNavList.ItemsSource as IList;

            var dragNavItemIndex = dataList.IndexOf(dragNavItemDataContext);

            dataList.Remove(dragNavItemDataContext);

            int indexInsertShould = dragNavItemIndex;

            IList relativeDataList = dataList;
            if (this.RSNavListRelative != null)
            {
                relativeDataList = this.RSNavListRelative.ItemsSource as IList;
            }

            object? relativeDataContext = null;

            if (this.RSNavItemRelative != null && !this.RSNavItemRelative.Equals(this.RSNavListSortInfo.RSNavItem))
            {
                relativeDataContext = this.RSNavItemRelative.DataContext;
                indexInsertShould = relativeDataList.IndexOf(relativeDataContext);
            }

            indexInsertShould = Math.Max(indexInsertShould, 0);

            indexInsertShould = Math.Min(indexInsertShould, relativeDataList.Count);

            relativeDataList.Insert(indexInsertShould, dragNavItemDataContext);


            //如果是NavigateModel 则说明这是个树导航
            if (relativeDataContext != null
                && relativeDataContext is NavigateModel relativeNavigateModel
                && dragNavItemDataContext is NavigateModel dragNavigateModel)
            {
                // 则需要递归获取树
                var sourceList = this.RSNavListSortInfo.RSNavList.ItemsSource as IList<NavigateModel>;

                List<NavigateModel> childList = new List<NavigateModel>();

                List<NavigateModel> GetChild(NavigateModel parent)
                {
                    var childList = sourceList.Where(t => t.ParentId == parent.Id).ToList();
                    for (int i = 0; i < childList.Count; i++)
                    {
                        var child = childList[i];
                        //更新Level
                        child.Level = parent.Level + 1;
                        var dataList = GetChild(child);
                        if (dataList.Count > 0)
                        {
                            childList.InsertRange(i, dataList);
                        }
                    }
                    return childList;
                }

                //获取到所有子树
                var children = GetChild(dragNavigateModel);
                if (children.Count > 0)
                {
                    childList = childList.Concat(children).ToList();
                }

                //如果相对移动是自己的孩子
                if (childList.Contains(relativeNavigateModel))
                {
                    childList.Where(t => t.ParentId == dragNavigateModel.Id)
                        .ToList()
                        .ForEach(t =>
                        {
                            t.ParentId = dragNavigateModel.ParentId;
                            t.Level = dragNavigateModel.Level;
                        });

                    dragNavigateModel.HasChildren = false;
                    dragNavigateModel.IsExpand = false;
                }
                else
                {
                    //如果不是自己的孩子
                    dragNavigateModel.ParentId = relativeNavigateModel.ParentId;
                    dragNavigateModel.Level = relativeNavigateModel.Level;
                }

                  
                foreach (var item in childList)
                {
                    //先要移除
                    dataList.Remove(item);
                }
                indexInsertShould = dataList.IndexOf(dragNavigateModel) + 1;

                //循环插入所有子树
                for (int i = 0; i < childList.Count; i++)
                {
                    var insertShould = childList[i];

                    indexInsertShould = Math.Max(indexInsertShould, 0);
                    indexInsertShould = Math.Min(indexInsertShould, dataList.Count);
                    dataList.Insert(indexInsertShould, insertShould);
                    indexInsertShould++;
                }
            }



            base.OnParentWin_PreviewMouseLeftButtonUp(sender, e);
        }



        protected override void OnRender(DrawingContext drawingContext)
        {

            var adornedElement = this.AdornedElement as FrameworkElement;
            var actualWidth = adornedElement.ActualWidth;
            var actualHeight = adornedElement.ActualHeight;
            VisualBrush visualBrush = new VisualBrush(adornedElement);

            var mousePosition = Mouse.GetPosition(this);
            var x = mousePosition.X + 20;
            var y = mousePosition.Y - 15;
            Rect rect = new Rect(x, y, actualWidth, actualHeight);
            drawingContext.DrawRectangle(visualBrush, new Pen(), rect);

            if (this.RSNavItemRelative == null || this.RSNavListRelative == null)
            {
                return;
            }


            var rsNavListActualWidth = this.RSNavListRelative.ActualWidth;
            var rsNavListPoint = this.RSNavListRelative.TransformToVisual(this).Transform(new Point(0, 0));
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
