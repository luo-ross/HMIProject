using Microsoft.Extensions.Logging;
using NPOI.POIFS.Properties;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Controls;
using RS.Widgets.Enums;
using RS.Widgets.Extensions;
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
        private RSNavItem RSListBoxItemRelative;
        private RSNavList RSNavListRelative;
        private RSNavListSortInfo RSNavListSortInfo { get; set; }

        public RSNavListSortAdorner(RSNavItem rsListBoxItem) : base(rsListBoxItem)
        {
            this.RSNavListSortInfo = new RSNavListSortInfo()
            {
                RSListBoxItem = rsListBoxItem,
                RSNavList = rsListBoxItem.TryFindParent<RSNavList>()
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

            var rsNavListRelative = this.ParentWin.GetUIElementUnderMouse<RSNavList>(this.CurrentMouseWinPoint);
            if (rsNavListRelative == null)
            {
                return;
            }
            this.RSNavListRelative = rsNavListRelative;

            var position = e.GetPosition(this.RSNavListRelative);
            var rSRSListBoxItem = this.RSNavListRelative.GetUIElementUnderMouse<RSNavItem>(position);
            if (rSRSListBoxItem != null)
            {
                this.RSListBoxItemRelative = rSRSListBoxItem;
            }


        }

        public override void OnParentWin_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dragRSListBoxItemDataContext = this.RSNavListSortInfo.RSListBoxItem.DataContext;
            if (dragRSListBoxItemDataContext == null)
            {
                return;
            }

            if (this.RSListBoxItemRelative == null)
            {
                return;
            }

            if (this.RSNavListRelative == null)
            {
                return;
            }

            if (this.RSListBoxItemRelative.Equals(this.RSNavListSortInfo.RSListBoxItem))
            {
                return;
            }

            object? relativeDataContext = this.RSListBoxItemRelative.DataContext;

            if (relativeDataContext == null)
            {
                return;
            }


            var rsNavListRelativeActualWidth = this.RSNavListRelative.ActualWidth;
            var rsNavListRelativePoint = this.RSNavListRelative.TransformToVisual(this).Transform(new Point(0, 0));

            var rsListBoxItemRelativeActualWidth = this.RSListBoxItemRelative.ActualWidth;
            var rsListBoxItemRelativeActualHeight = this.RSListBoxItemRelative.ActualHeight;
            var rsListBoxItemRelativePoint = this.RSListBoxItemRelative.TransformToVisual(this).Transform(new Point(0, 0));

            //获取相对矩形框
            var rsListBoxItemRelativeRect = new Rect(
                rsListBoxItemRelativePoint.X,
                rsListBoxItemRelativePoint.Y,
                rsListBoxItemRelativeActualWidth,
                rsListBoxItemRelativeActualHeight);

            var mousePositionCurrent = Mouse.GetPosition(this);
            var rectArea = RSAdorner.GetRectArea(rsListBoxItemRelativeRect, mousePositionCurrent);

            var dataList = this.RSNavListSortInfo.RSNavList.ItemsSource as IList;
            dataList.Remove(dragRSListBoxItemDataContext);

            IList relativeDataList = this.RSNavListRelative.ItemsSource as IList;
            int indexInsertShould = relativeDataList.IndexOf(relativeDataContext);


            var relativeNavigateModel = relativeDataContext as NavigateModel;
            var dragNavigateModel = dragRSListBoxItemDataContext as NavigateModel;
            IList<NavigateModel>? sourceList = default;
            List<NavigateModel>? childList = default;
            switch (rectArea)
            {
                case RectArea.TopLeft:
                case RectArea.TopCenter:
                case RectArea.TopRight:
                    indexInsertShould = Math.Max(indexInsertShould, 0);
                    indexInsertShould = Math.Min(indexInsertShould, relativeDataList.Count);
                    relativeDataList.Insert(indexInsertShould, dragRSListBoxItemDataContext);
                    break;
                case RectArea.MiddleLeft:
                case RectArea.MiddleCenter:
                case RectArea.MiddleRight:

                    sourceList = this.RSNavListSortInfo.RSNavList.ItemsSource as IList<NavigateModel>;
                    if (sourceList == null
                        || relativeNavigateModel == null
                        || dragNavigateModel == null)
                    {
                        goto case RectArea.TopCenter;
                    }

                    //获取到所有子树
                    childList = GetChild(sourceList, dragNavigateModel);

                    //如果是自己的子集
                    if (childList.Contains(relativeNavigateModel))
                    {
                        var dragChildList = childList.Where(t => t.ParentId == dragNavigateModel.Id)
                              .ToList();
                        foreach (var item in dragChildList)
                        {
                            item.ParentId = dragNavigateModel.ParentId;
                            item.Level = dragNavigateModel.Level;
                            this.UpdateLevel(childList, item);
                        }

                        dragNavigateModel.HasChildren = false;
                        dragNavigateModel.IsExpand = false;
                    }


                    dragNavigateModel.ParentId = relativeNavigateModel.Id;
                    dragNavigateModel.Level = relativeNavigateModel.Level + 1;

                    relativeNavigateModel.HasChildren = true;

                    //如果展开
                    if (relativeNavigateModel.IsExpand)
                    {
                        goto case RectArea.BottomCenter;
                    }
                    break;
                case RectArea.BottomLeft:
                case RectArea.BottomCenter:
                case RectArea.BottomRight:
                    indexInsertShould = indexInsertShould + 1;
                    indexInsertShould = Math.Max(indexInsertShould, 0);
                    indexInsertShould = Math.Min(indexInsertShould, relativeDataList.Count);
                    relativeDataList.Insert(indexInsertShould, dragRSListBoxItemDataContext);
                    break;
            }


            if (relativeNavigateModel == null
                || dragNavigateModel == null
                || sourceList == default
                || childList == default)
            {
                return;
            }


            childList = GetChild(sourceList, dragNavigateModel);

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

            base.OnParentWin_PreviewMouseLeftButtonUp(sender, e);
        }

        private void UpdateLevel(List<NavigateModel> source, NavigateModel parent)
        {
            var childList = source.Where(t => t.ParentId == parent.Id)
                              .ToList();
            foreach (var item in childList)
            {
                item.Level = parent.Level + 1;
                this.UpdateLevel(childList, item);
            }
        }

        private List<NavigateModel> GetChild(IList<NavigateModel> sourceList, NavigateModel parent)
        {
            var childList = sourceList.Where(t => t.ParentId == parent.Id).ToList();
            for (int i = 0; i < childList.Count; i++)
            {
                var child = childList[i];
                //更新Level
                child.Level = parent.Level + 1;
                var dataList = GetChild(sourceList, child);
                if (dataList.Count > 0)
                {
                    childList.InsertRange(i, dataList);
                }
            }
            return childList;
        }


        protected override void OnRender(DrawingContext drawingContext)
        {
            var adornedElement = this.AdornedElement as FrameworkElement;
            var actualWidth = adornedElement.ActualWidth;
            var actualHeight = adornedElement.ActualHeight;
            VisualBrush visualBrush = new VisualBrush(adornedElement);

            var mousePositionCurrent = Mouse.GetPosition(this);
            var x = mousePositionCurrent.X + 20;
            var y = mousePositionCurrent.Y - 15;
            Rect rect = new Rect(x, y, actualWidth, actualHeight);
            drawingContext.DrawRectangle(visualBrush, new Pen(), rect);

            if (this.RSListBoxItemRelative == null || this.RSNavListRelative == null)
            {
                return;
            }


            var rsNavListRelativeActualWidth = this.RSNavListRelative.ActualWidth;
            var rsNavListRelativePoint = this.RSNavListRelative.TransformToVisual(this).Transform(new Point(0, 0));

            var rsListBoxItemRelativeActualWidth = this.RSListBoxItemRelative.ActualWidth;
            var rsListBoxItemRelativeActualHeight = this.RSListBoxItemRelative.ActualHeight;
            var rsListBoxItemRelativePoint = this.RSListBoxItemRelative.TransformToVisual(this).Transform(new Point(0, 0));

            //获取相对矩形框
            var rsListBoxItemRelativeRect = new Rect(
                rsListBoxItemRelativePoint.X,
                rsListBoxItemRelativePoint.Y,
                rsListBoxItemRelativeActualWidth,
                rsListBoxItemRelativeActualHeight);
            var rectArea = RSAdorner.GetRectArea(rsListBoxItemRelativeRect, mousePositionCurrent);

            // 计算起始点和结束点
            Point startPoint;
            Point endPoint;

            switch (rectArea)
            {
                case RectArea.TopLeft:
                case RectArea.TopCenter:
                case RectArea.TopRight:
                    startPoint = new Point((int)rsNavListRelativePoint.X + 4, (int)rsListBoxItemRelativePoint.Y - 2);
                    endPoint = startPoint + new Vector((int)rsNavListRelativeActualWidth - 4, 0);
                    this.DrawGuideLine(drawingContext, startPoint, endPoint);
                    break;
                case RectArea.MiddleLeft:
                case RectArea.MiddleCenter:
                case RectArea.MiddleRight:

                    object? relativeDataContext = this.RSListBoxItemRelative.DataContext;
                    var dragRSListBoxItemDataContext = this.RSNavListSortInfo.RSListBoxItem.DataContext;
                    if (dragRSListBoxItemDataContext == null || relativeDataContext == null)
                    {
                        return;
                    }

                    if (relativeDataContext is NavigateModel relativeNavigateModel
                        && dragRSListBoxItemDataContext is NavigateModel dragNavigateModel)
                    {
                        startPoint = new Point((int)rsNavListRelativePoint.X + 4, (int)rsListBoxItemRelativePoint.Y + rsListBoxItemRelativeActualHeight / 2);
                        drawingContext.DrawRectangle(null, DrawPen, rsListBoxItemRelativeRect);
                    }
                    else
                    {
                        goto case RectArea.TopCenter;
                    }
                    break;
                case RectArea.BottomLeft:
                case RectArea.BottomCenter:
                case RectArea.BottomRight:
                    startPoint = new Point((int)rsNavListRelativePoint.X + 4, (int)rsListBoxItemRelativePoint.Y + rsListBoxItemRelativeActualHeight + 2);
                    endPoint = startPoint + new Vector((int)rsNavListRelativeActualWidth - 4, 0);
                    this.DrawGuideLine(drawingContext, startPoint, endPoint);
                    break;
            }
        }


        private void DrawGuideLine(DrawingContext drawingContext, Point startPoint, Point endPoint)
        {
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
