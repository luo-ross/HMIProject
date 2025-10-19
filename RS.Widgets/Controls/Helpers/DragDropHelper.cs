using NPOI.OpenXmlFormats.Wordprocessing;
using RS.Widgets.Adorners;
using RS.Widgets.Commons;
using RS.Widgets.Enums;
using RS.Widgets.Extensions;
using RS.Widgets.Interfaces;
using RS.Win32API;
using RS.Win32API.Structs;
using System.Collections;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public static class DragDropHelper
    {
        public static readonly DependencyProperty DragDropStrategyProperty =
       DependencyProperty.RegisterAttached(
           "DragDropStrategy",
           typeof(IDragDropStrategy),
           typeof(DragDropHelper),
           new PropertyMetadata(null));

        public static void SetDragDropStrategy(UIElement element, IDragDropStrategy value)
        {
            element.SetValue(DragDropStrategyProperty, value);
        }

        public static IDragDropStrategy GetDragDropStrategy(UIElement element)
        {
            return (IDragDropStrategy)element.GetValue(DragDropStrategyProperty);
        }

        public static readonly DependencyProperty DragDropEffectsProperty =
           DependencyProperty.RegisterAttached(
               "DragDropEffects",
               typeof(DragDropEffects),
               typeof(DragDropHelper),
               new PropertyMetadata(DragDropEffects.Move));

        public static DragDropEffects GetDragDropEffects(DependencyObject obj)
        {
            return (DragDropEffects)obj.GetValue(DragDropEffectsProperty);
        }

        public static void SetDragDropEffects(DependencyObject obj, DragDropEffects value)
        {
            obj.SetValue(DragDropEffectsProperty, value);
        }



        public static readonly DependencyProperty IsDragTargetProperty =
            DependencyProperty.RegisterAttached(
                "IsDragTarget",
                typeof(bool),
                typeof(DragDropHelper),
                new PropertyMetadata(false, OnIsDragTargetPropertyChanged));

        public static bool GetIsDragTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDragTargetProperty);
        }

        public static void SetIsDragTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDragTargetProperty, value);
        }

        private static void OnIsDragTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dragTarget = d as UIElement;
            dragTarget.PreviewMouseLeftButtonDown -= DragTarget_PreviewMouseLeftButtonDown;
            dragTarget.PreviewMouseMove -= DragTarget_PreviewMouseMove;
            dragTarget.PreviewMouseLeftButtonUp -= DragTarget_PreviewMouseLeftButtonUp;
            dragTarget.GiveFeedback -= DragTarget_GiveFeedback;
            dragTarget.QueryContinueDrag -= DragTarget_QueryContinueDrag;
            if (e.NewValue is true)
            {
                dragTarget.PreviewMouseLeftButtonDown += DragTarget_PreviewMouseLeftButtonDown;
                dragTarget.PreviewMouseMove += DragTarget_PreviewMouseMove;
                dragTarget.PreviewMouseLeftButtonUp += DragTarget_PreviewMouseLeftButtonUp;
                dragTarget.GiveFeedback += DragTarget_GiveFeedback;
                dragTarget.QueryContinueDrag += DragTarget_QueryContinueDrag;
            }
        }


        public static DependencyObject DragTargetParent;
        public static FrameworkElement DragSource;
        private static Point? DragStartPoint;
        private static Point DragSourceTopLeftPoint;
        private static Vector DragSourceVector;
        public static RSDragPreview DragPreview;
        public static RSDragEffect DragDropEffect;
        public static bool IsDraging;
        public static FrameworkElement DropTarget;
        public static bool IsDragTarget_PreviewMouseLeftButtonDown;
        public static Window DragHostWindow;
        private static void DragTarget_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dragTarget = sender as FrameworkElement;
            DragHostWindow = dragTarget.TryFindParent<Window>();
            var point = e.GetPosition(dragTarget);


            if (dragTarget is RSDockGrid dockGrid)
            {
                (int index, UIElement? child) = dockGrid.GetMouseOverChildInfo();
                if (index == -1 || child == null)
                {
                    return;
                }
                var frameworkElement = (FrameworkElement)child;
                if (frameworkElement is RSDockPanel dockPanel && dockPanel.IsCanDragPanel)
                {
                    DragSource = frameworkElement;
                }
                else
                {
                    DragSource = null;
                }
            }
            else if (dragTarget is Button button)
            {
                DragSource = dragTarget;
            }
            else if (dragTarget is ItemsControl itemsControl)
            {
                var itemType = GetItemType(itemsControl);
                if (itemType == null)
                {
                    return;
                }
                var elementAtPoint = (FrameworkElement)itemsControl.GetElementAtPointOfType(point, itemType);
                if (elementAtPoint == null)
                {
                    return;
                }
                DragSource = elementAtPoint;
            }


            if (DragSource == null)
            {
                return;
            }

            DragStartPoint = e.GetPosition(DragHostWindow);
            DragSourceTopLeftPoint = DragSource.TranslatePoint(new Point(0, 0), null);
            DragSourceVector = DragStartPoint.Value - DragSourceTopLeftPoint;

            IsDragTarget_PreviewMouseLeftButtonDown = true;

            //dragTarget?.CaptureMouse();
        }


        private static Type GetItemType(ItemsControl itemsControl)
        {
            foreach (var item in itemsControl.Items)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromItem(item);
                if (container != null)
                {
                    return container.GetType();
                }
            }
            return null;
        }

        private static Dictionary<FrameworkElement, Window> DragDropDockWindowDic = new Dictionary<FrameworkElement, Window>();
        private static bool IsDragPreviewShouldClose = true;
        private static void DragTarget_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (DragSource == null)
            {
                return;
            }

            var dragSourceActualWidth = DragSource.ActualWidth;
            var dragSourceActualHeight = DragSource.ActualHeight;


            var dragTarget = sender as UIElement;
            if (e.LeftButton == MouseButtonState.Pressed
                && IsDragTarget_PreviewMouseLeftButtonDown
                && DragStartPoint != null)
            {
                Point currentPosition = e.GetPosition(DragHostWindow);
                if (Math.Abs(currentPosition.X - DragStartPoint.Value.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - DragStartPoint.Value.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (IsDraging)
                    {
                        return;
                    }
                    IsDraging = true;

                    var dependencyObject = (DependencyObject)sender;
                    var dragDropEffects = DragDropHelper.GetDragDropEffects(dependencyObject);

                    IsDragPreviewShouldClose = true;

                    if (dragTarget is RSDockGrid dockGrid && DragSource is RSDockPanel dockPanel)
                    {

                        IsDragPreviewShouldClose = false;
                     
                        var parentWin = DragSource.TryFindParent<Window>();

                      
                        //然后再宽带连接
                        RemoveDragSource(DragSource);

                        if (!DragDropDockWindowDic.ContainsKey(DragSource))
                        {
                            DragSource.Width = double.NaN;
                            DragSource.Height = double.NaN;
                            DragPreview = new RSDragPreview(dockPanel,
                                dragSourceActualWidth,
                                dragSourceActualHeight);
                            DragPreview.Owner = parentWin;
                            DragPreview.Show();
                         

                            DragPreview.Title = DragSource.Name;
                            DragDropDockWindowDic[DragSource] = DragPreview;

                            DragPreview.OnWindowDragStarted();
                        }
                    }
                    else
                    {
                        if (DragPreview == null)
                        {
                            DragPreview = new RSDragPreview(DragSource,
                                dragSourceActualWidth,
                                dragSourceActualHeight);
                            DragPreview.Show();
                        }

                        if (DragDropEffect == null)
                        {
                            DragDropEffect = new RSDragEffect();
                            DragDropEffect.DragDropEffects = dragDropEffects;
                            DragDropEffect.Show();
                        }
                    }

                    dragDropEffects = DragDrop.DoDragDrop(dependencyObject, DragSource, dragDropEffects);


                    DragPreview.OnWindowDragCompleted();

                    if (IsDragPreviewShouldClose)
                    {
                        DragPreview?.Close();
                        DragPreview = null;
                    }


                    DragDropEffect?.Close();
                    DragDropEffect = null;

                    RemoveGuidAdorner();

                    DragSource = null;
                    IsDraging = false;
                }
            }
        }


        private static void DragTarget_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Console.WriteLine("DragTarget_PreviewMouseLeftButtonUp");
            var dragTarget = sender as UIElement;
            dragTarget?.ReleaseMouseCapture();
            IsDragTarget_PreviewMouseLeftButtonDown = false;
            DragStartPoint = null;
        }

        private static void DragTarget_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            DragDropEffect?.UpdateDragDropEffects(e.Effects);
            e.Handled = true;
        }

        private static void DragTarget_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            UpdateDragPosition(e);
        }

        private static void UpdateDragPosition(QueryContinueDragEventArgs e)
        {
            POINT cursorPosition = new POINT();
            NativeMethods.GetCursorPos(cursorPosition);

            if (DragDropEffect != null)
            {
                var dragDropEffectWidth = DragDropEffect.ActualWidth;
                var dragDropEffectHeight = DragDropEffect.ActualHeight;
                double dragDropEffectLeft = cursorPosition.X - dragDropEffectWidth / 2D;
                double dragDropEffectTop = cursorPosition.Y - dragDropEffectHeight;
                DragDropEffect.Left = dragDropEffectLeft;
                DragDropEffect.Top = dragDropEffectTop;
            }

            if (DragPreview != null)
            {
                var dragPreviewWidth = DragPreview.ActualWidth;
                var dragPreviewHeight = DragPreview.ActualHeight;

                double dragPreviewLeft = cursorPosition.X - DragSourceVector.X;
                double dragPreviewTop = cursorPosition.Y - DragSourceVector.Y;
                DragPreview.Left = dragPreviewLeft;
                DragPreview.Top = dragPreviewTop;

                DragPreview.OnWindowDragMoving();
            }
        }

        /// <summary>
        /// 切换到目标窗口并在不松开鼠标的情况下触发其标题栏拖拽
        /// </summary>
        /// <param name="targetWindow">目标窗口</param>
        public static void TransferDragToWindow(Window targetWindow)
        {
            if (targetWindow == null)
            {
                return;
            }

            // 获取目标窗口句柄
            var hwndSource = PresentationSource.FromVisual(targetWindow) as HwndSource;
            if (hwndSource == null)
            {
                return;
            }

            IntPtr hWnd = hwndSource.Handle;
            var handleRef = new HandleRef(null, hWnd);
            // 将目标窗口置于前台并激活
            NativeMethods.SetForegroundWindow(handleRef);

            // 获取当前鼠标位置
            POINT cursorPosition = new POINT();
            NativeMethods.GetCursorPos(cursorPosition);

            // 计算相对于目标窗口的客户区坐标
            NativeMethods.ScreenToClient(handleRef, cursorPosition);

            //// 释放当前窗口的鼠标捕获
            NativeMethods.ReleaseCapture();

            //// 1. 发送非客户区鼠标按下消息（模拟在标题栏按下）
            //// wParam = HTCAPTION（标题栏），lParam = 鼠标坐标
            IntPtr lParam = (IntPtr)((cursorPosition.Y << 16) | (cursorPosition.X & 0xFFFF));
           NativeMethods.SendMessage(hWnd, NativeMethods.WM_NCLBUTTONDOWN, NativeMethods.HTCAPTION, lParam);

            // 2. 立即发送鼠标移动消息（触发拖拽）
            NativeMethods.SendMessage(hWnd, NativeMethods.WM_MOUSEMOVE, NativeMethods.HTCAPTION, lParam);
        }


        public static readonly DependencyProperty IsDropTargetProperty =
            DependencyProperty.RegisterAttached(
                "IsDropTarget",
                typeof(bool),
                typeof(DragDropHelper),
                new PropertyMetadata(false, OnIsDropTargetPropertyChanged));

        public static bool GetIsDropTarget(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDropTargetProperty);
        }

        public static void SetIsDropTarget(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDropTargetProperty, value);
        }



        private static void OnIsDropTargetPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dropTarget = d as UIElement;
            if (dropTarget == null)
            {
                return;
            }

            dropTarget.AllowDrop = (bool)e.NewValue;
            dropTarget.DragEnter -= DropTarget_DragEnter;
            dropTarget.DragLeave -= DropTarget_DragLeave;
            dropTarget.DragOver -= DropTarget_DragOver;
            dropTarget.Drop -= DropTarget_Drop;
            if (dropTarget.AllowDrop)
            {
                dropTarget.DragEnter += DropTarget_DragEnter;
                dropTarget.DragLeave += DropTarget_DragLeave;
                dropTarget.DragOver += DropTarget_DragOver;
                dropTarget.Drop += DropTarget_Drop;
            }

        }

        private static RSGuidAdorner GuidAdorner;

        private static void DropTarget_DragEnter(object sender, DragEventArgs e)
        {
            var dropTarget = sender as UIElement;
            if (DragSource == null)
            {
                return;
            }

            if (dropTarget is Panel dropPanel)
            {
                (int index, UIElement? child) = dropPanel.GetMouseOverChildInfo();
                if (index == -1 || child == null)
                {
                    return;
                }
                Window activeWindow = Window.GetWindow(dropTarget);
                var adornerDecorator = activeWindow.FindChild<AdornerDecorator>();
                var adornerLayer = adornerDecorator.AdornerLayer;
                if (GuidAdorner == null)
                {
                    GuidAdorner = new RSGuidAdorner(dropTarget);
                    adornerLayer.Add(GuidAdorner);
                }
            }
            else if (dropTarget is Decorator dropDecorator)
            {
                dropDecorator.Child = dropTarget;
            }
            else if (dropTarget is ListBox selector)
            {
                if (DragSource is not ListBoxItem)
                {
                    return;
                }
            }
            else if (dropTarget is TreeView treeView)
            {
                if (DragSource is not TreeViewItem)
                {
                    return;
                }
            }
            else if (dropTarget is ListView listView)
            {
                if (DragSource is not ListBoxItem)
                {
                    return;
                }
            }
            else if (dropTarget is DataGrid dataGrid)
            {
                if (DragSource is not DataGridRow)
                {
                    return;
                }
            }
            else if (dropTarget is ComboBox comboBox)
            {
                if (DragSource is not ComboBoxItem)
                {
                    return;
                }
            }
        }

        private static void DropTarget_DragOver(object sender, DragEventArgs e)
        {
            var dropTarget = sender as UIElement;
            if (DragSource == null)
            {
                return;
            }
            var mousePositionCurrent = e.GetPosition(dropTarget);
            if (dropTarget is StackPanel dropStackPanel)
            {
                PanelGuidStrategy(dropStackPanel, dropStackPanel.Orientation, mousePositionCurrent, false);
            }
            else if (dropTarget is VirtualizingStackPanel dropVirtualizingStackPanel)
            {
                PanelGuidStrategy(dropVirtualizingStackPanel, dropVirtualizingStackPanel.Orientation, mousePositionCurrent, false);
            }
            else if (dropTarget is WrapPanel dropWrapPanel)
            {
                PanelGuidStrategy(dropWrapPanel, dropWrapPanel.Orientation, mousePositionCurrent, true);
            }
            else if (dropTarget is DockPanel dropDockPanel)
            {
                DockPanelGuidStrategy(dropDockPanel, mousePositionCurrent);
            }
            else if (dropTarget is Grid dropGrid)
            {
                GridGuidStrategy(dropGrid, mousePositionCurrent);
            }
            else if (dropTarget is Decorator dropDecorator)
            {
                dropDecorator.Child = dropTarget;
            }
            else if (dropTarget is ListBox selector)
            {
                if (DragSource is not ListBoxItem)
                {
                    return;
                }
            }
            else if (dropTarget is TreeView treeView)
            {
                if (DragSource is not TreeViewItem)
                {
                    return;
                }
            }
            else if (dropTarget is ListView listView)
            {
                if (DragSource is not ListBoxItem)
                {
                    return;
                }
            }
            else if (dropTarget is DataGrid dataGrid)
            {
                if (DragSource is not DataGridRow)
                {
                    return;
                }
            }
            else if (dropTarget is ComboBox comboBox)
            {
                if (DragSource is not ComboBoxItem)
                {
                    return;
                }
            }
        }


        private static void DockPanelGuidStrategy(DockPanel dropDockPanel, Point mousePositionCurrent)
        {
            (int index, UIElement? child) = dropDockPanel.GetMouseOverChildInfo();
            if (index == -1 || child == null)
            {
                return;
            }
            var elementRelative = child as FrameworkElement;
            if (elementRelative == null)
            {
                return;
            }
        }

        private static void GridGuidStrategy(Grid dropGrid, Point mousePositionCurrent)
        {
            (int index, UIElement? child) = dropGrid.GetMouseOverChildInfo();
            if (index == -1 || child == null)
            {
                return;
            }

            var elementRelative = child as FrameworkElement;
            if (elementRelative == null)
            {
                return;
            }

            var dropPanelActualWidth = dropGrid.ActualWidth;
            var elementRelativePosition = elementRelative.TransformToVisual(dropGrid).Transform(new Point(0, 0));
            var rectArea = GetRelativeRectArea(dropGrid, elementRelative, elementRelativePosition, mousePositionCurrent);

            Point startPoint;
            Point endPoint;
            switch (rectArea)
            {
                case RectArea.TopLeft:
                case RectArea.TopCenter:
                case RectArea.TopRight:
                    startPoint = new Point(0, (int)elementRelativePosition.Y);
                    endPoint = startPoint + new Vector((int)dropPanelActualWidth, 0);
                    break;
                case RectArea.MiddleLeft:
                case RectArea.MiddleCenter:
                case RectArea.MiddleRight:
                    break;
                case RectArea.BottomLeft:
                case RectArea.BottomCenter:
                case RectArea.BottomRight:
                    startPoint = new Point(0, (int)elementRelativePosition.Y + elementRelative.ActualHeight);
                    endPoint = startPoint + new Vector((int)dropPanelActualWidth, 0);
                    break;
            }

            GuidAdorner.UpdateGuideLine(startPoint, endPoint);
        }

        private static void PanelGuidStrategy(Panel dropPanel, Orientation orientation, Point mousePositionCurrent, bool isWrapPanel)
        {
            (int index, UIElement? child) = dropPanel.GetMouseOverChildInfo();
            if (index == -1 || child == null)
            {
                return;
            }
            var elementRelative = child as FrameworkElement;
            if (elementRelative == null)
            {
                return;
            }

            var elementRelativePosition = elementRelative.TransformToVisual(dropPanel).Transform(new Point(0, 0));

            var rectArea = GetRelativeRectArea(dropPanel, elementRelative, elementRelativePosition, mousePositionCurrent);

            Point startPoint;
            Point endPoint;
            if (orientation == Orientation.Horizontal)
            {
                switch (rectArea)
                {
                    case RectArea.TopLeft:
                    case RectArea.MiddleLeft:
                    case RectArea.BottomLeft:
                        if (isWrapPanel)
                        {
                            startPoint = new Point((int)elementRelativePosition.X, elementRelativePosition.Y);
                            endPoint = startPoint + new Vector(0, elementRelative.ActualHeight);
                        }
                        else
                        {
                            startPoint = new Point((int)elementRelativePosition.X, 0);
                            endPoint = startPoint + new Vector(0, dropPanel.ActualHeight);
                        }
                        break;
                    case RectArea.TopCenter:
                    case RectArea.MiddleCenter:
                    case RectArea.BottomCenter:
                        break;
                    case RectArea.TopRight:
                    case RectArea.MiddleRight:
                    case RectArea.BottomRight:
                        if (isWrapPanel)
                        {
                            startPoint = new Point((int)(elementRelativePosition.X + elementRelative.ActualWidth), elementRelativePosition.Y);
                            endPoint = startPoint + new Vector(0, elementRelative.ActualHeight);
                        }
                        else
                        {
                            startPoint = new Point((int)(elementRelativePosition.X + elementRelative.ActualWidth), 0);
                            endPoint = startPoint + new Vector(0, dropPanel.ActualHeight);
                        }
                        break;
                }
            }
            else
            {
                switch (rectArea)
                {
                    case RectArea.TopLeft:
                    case RectArea.TopCenter:
                    case RectArea.TopRight:
                        if (isWrapPanel)
                        {
                            startPoint = new Point(elementRelativePosition.X, (int)elementRelativePosition.Y);
                            endPoint = startPoint + new Vector((int)elementRelative.ActualWidth, 0);
                        }
                        else
                        {
                            startPoint = new Point(0, (int)elementRelativePosition.Y);
                            endPoint = startPoint + new Vector((int)dropPanel.ActualWidth, 0);
                        }
                        break;
                    case RectArea.MiddleLeft:
                    case RectArea.MiddleCenter:
                    case RectArea.MiddleRight:
                        break;
                    case RectArea.BottomLeft:
                    case RectArea.BottomCenter:
                    case RectArea.BottomRight:
                        if (isWrapPanel)
                        {
                            startPoint = new Point(elementRelativePosition.X, (int)elementRelativePosition.Y + elementRelative.ActualHeight);
                            endPoint = startPoint + new Vector((int)elementRelative.ActualWidth, 0);
                        }
                        else
                        {
                            startPoint = new Point(0, (int)elementRelativePosition.Y + elementRelative.ActualHeight);
                            endPoint = startPoint + new Vector((int)dropPanel.ActualWidth, 0);
                        }
                        break;
                }
            }

            GuidAdorner.UpdateGuideLine(startPoint, endPoint);
        }



        private static void DropTarget_DragLeave(object sender, DragEventArgs e)
        {
            RemoveGuidAdorner();
        }

        private static void RemoveGuidAdorner()
        {
            if (GuidAdorner != null)
            {
                var parent = GuidAdorner.Parent;
                var adornerLayer = GuidAdorner.Parent as AdornerLayer;
                adornerLayer?.Remove(GuidAdorner);
                GuidAdorner = null;
            }
        }

        private static RectArea GetRelativeRectArea(FrameworkElement visual,
            FrameworkElement elementRelative,
            Point elementRelativePosition,
            Point mousePositionCurrent)
        {
            var elementRelativeRect = new Rect(
                elementRelativePosition.X,
                elementRelativePosition.Y,
                elementRelative.ActualWidth,
                elementRelative.ActualHeight);
            var rectArea = PointHelper.GetRectArea(elementRelativeRect, mousePositionCurrent);
            return rectArea;
        }


        private static void DropTarget_Drop(object sender, DragEventArgs e)
        {
            var dropTarget = sender as UIElement;
            if (DragSource == null)
            {
                return;
            }
            var mousePositionCurrent = e.GetPosition(dropTarget);

            if (dropTarget is StackPanel dropStackPanel)
            {
                PanelDropStrategy(dropStackPanel, mousePositionCurrent);
            }
            else if (dropTarget is VirtualizingStackPanel dropVirtualizingStackPanel)
            {
                PanelDropStrategy(dropVirtualizingStackPanel, mousePositionCurrent);
            }
            else if (dropTarget is WrapPanel dropWrapPanel)
            {
                PanelDropStrategy(dropWrapPanel, mousePositionCurrent);
            }
            else if (dropTarget is Grid dropGrid)
            {
                PanelDropStrategy(dropGrid, mousePositionCurrent);
            }
            else if (dropTarget is DockPanel dropDockPanel)
            {
                DockPanelDropStrategy(dropDockPanel, mousePositionCurrent);
            }
            else if (dropTarget is Decorator dropDecorator)
            {
                dropDecorator.Child = dropTarget;
            }
            else if (dropTarget is ListBox selector)
            {
                if (DragSource is not ListBoxItem)
                {
                    return;
                }
            }
            else if (dropTarget is TreeView treeView)
            {
                if (DragSource is not TreeViewItem)
                {
                    return;
                }
            }
            else if (dropTarget is ListView listView)
            {
                if (DragSource is not ListBoxItem)
                {
                    return;
                }
            }
            else if (dropTarget is DataGrid dataGrid)
            {
                if (DragSource is not DataGridRow)
                {
                    return;
                }
            }
            else if (dropTarget is ComboBox comboBox)
            {
                if (DragSource is not ComboBoxItem)
                {
                    return;
                }
            }
        }

        private static void DockPanelDropStrategy(DockPanel dropDockPanel, Point mousePositionCurrent)
        {

        }

        private static void PanelDropStrategy(Panel dropPanel, Point mousePositionCurrent)
        {
            (int index, UIElement? child) = dropPanel.GetMouseOverChildInfo();
            int insertIndex = -1;
            if (child != null)
            {
                var elementRelative = child as FrameworkElement;
                if (elementRelative == null)
                {
                    return;
                }
                var elementRelativePosition = elementRelative.TransformToVisual(dropPanel).Transform(new Point(0, 0));

                var rectArea = GetRelativeRectArea(dropPanel, elementRelative, elementRelativePosition, mousePositionCurrent);

                Orientation? orientation = (Orientation?)dropPanel.ReflectionGetProperty("Orientation");
                if (orientation.HasValue && orientation.Value == Orientation.Horizontal)
                {
                    switch (rectArea)
                    {
                        case RectArea.TopLeft:
                        case RectArea.MiddleLeft:
                        case RectArea.BottomLeft:
                            insertIndex = index;
                            break;
                        case RectArea.TopCenter:
                        case RectArea.MiddleCenter:
                        case RectArea.BottomCenter:
                            break;
                        case RectArea.TopRight:
                        case RectArea.MiddleRight:
                        case RectArea.BottomRight:
                            insertIndex = index + 1;
                            break;
                    }
                }
                else
                {
                    switch (rectArea)
                    {
                        case RectArea.TopLeft:
                        case RectArea.TopCenter:
                        case RectArea.TopRight:
                            insertIndex = index;
                            break;
                        case RectArea.MiddleLeft:
                        case RectArea.MiddleCenter:
                        case RectArea.MiddleRight:
                            break;
                        case RectArea.BottomLeft:
                        case RectArea.BottomCenter:
                        case RectArea.BottomRight:
                            insertIndex = index + 1;
                            break;
                    }
                }
            }

            RemoveDragSource(DragSource);

            if (insertIndex > -1)
            {
                insertIndex = Math.Min(insertIndex, dropPanel.Children.Count);
                insertIndex = Math.Max(insertIndex, 0);
                dropPanel.Children.Insert(insertIndex, DragSource);
            }
            else
            {
                dropPanel.Children.Add(DragSource);
            }

            DragPreview?.Close();
            DragPreview = null;
        }

        private static void RemoveDragSource(UIElement dragSource)
        {
            if (dragSource == null)
            {
                return;
            }

            if (dragSource is ListBoxItem
                || dragSource is ListViewItem
                || dragSource is TreeViewItem
                || dragSource is DataGridRow)
            {
                var itemsControl = ItemsControl.ItemsControlFromItemContainer(dragSource);
                if (itemsControl != null)
                {
                    var item = itemsControl.ItemContainerGenerator.ItemFromContainer(dragSource);
                    if (item != DependencyProperty.UnsetValue)
                    {
                        var itemsSource = itemsControl.ItemsSource ?? itemsControl.Items;
                        if (itemsSource is IList list
                            && item != null
                            && list.IsFixedSize == false
                            && list.IsReadOnly == false)
                        {
                            list.Remove(item);
                            return;
                        }
                    }
                }
            }

            DependencyObject parent = VisualTreeHelper.GetParent(dragSource);
            if (parent == null)
            {
                return;
            }

            if (parent is Panel panel)
            {
                panel.Children.Remove(dragSource);
            }
            else if (parent is Decorator decorator)
            {
                if (decorator.Child == dragSource)
                {
                    decorator.Child = null;
                }
            }
        }

    }
}
