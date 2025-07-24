using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Controls;
using RS.Widgets.Adorners;
using RS.Win32API;
using RS.Win32API.Structs;
using System.Windows.Input;
using MathNet.Numerics;
using RS.Win32API.Enums;
using System.Diagnostics;
using System.Runtime.InteropServices;
using NPOI.SS.UserModel;
using RS.Widgets.Interfaces;
using System.Windows.Data;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Collections;
using RS.Widgets.Extensions;

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
        public static UIElement DragSource;
        private static Point DragStartPoint;
        private static Point DragSourceTopLeftPoint;
        private static Vector DragSourceVector;
        public static RSDragDropPreview DragDropPreview;
        public static RSDragDropEffect DragDropEffect;
        public static bool IsDraging;
        public static UIElement DropTarget;
        private static void DragTarget_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var dragTarget = sender as UIElement;
            var point = e.GetPosition(dragTarget);
            if (dragTarget is Button button)
            {
                DragSource = dragTarget;
            }
            else if (dragTarget is ItemsControl itemsControl)
            {
                var itemType = GetItemType(itemsControl);
                if (itemType==null)
                {
                    return;
                }
                var elementAtPoint = (UIElement)itemsControl.GetElementAtPointOfType(point, itemType);
                if (elementAtPoint != null)
                {
                    DragSource = elementAtPoint;
                }
            }


            if (DragSource == null)
            {
                return;
            }


            //POINT cursorPosition = new POINT();
            //NativeMethods.GetCursorPos(cursorPosition);

            //var point = e.GetPosition(dragTarget);

            //var sdf = GetAllControlsAtPoint(dragTarget, point);




            DragStartPoint = e.GetPosition(null);
            DragSourceTopLeftPoint = DragSource.TranslatePoint(new Point(0, 0), null);
            DragSourceVector = DragStartPoint - DragSourceTopLeftPoint;
            dragTarget?.CaptureMouse();
        }


        private static Type GetItemType(ItemsControl itemsControl)
        {
            if (itemsControl.Items.Count > 0)
            {
                var container = itemsControl.ItemContainerGenerator.ContainerFromIndex(0);
                if (container != null)
                {
                    return container.GetType();
                }
            }

            return null;
        }


        private static void DragTarget_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            if (DragSource == null)
            {
                return;
            }
            var dragTarget = sender as UIElement;
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                Point currentPosition = e.GetPosition(null);
                if (Math.Abs(currentPosition.X - DragStartPoint.X) > SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(currentPosition.Y - DragStartPoint.Y) > SystemParameters.MinimumVerticalDragDistance)
                {
                    if (IsDraging)
                    {
                        return;
                    }
                    IsDraging = true;

                    if (DragDropPreview == null)
                    {
                        DragDropPreview = new RSDragDropPreview(DragSource);
                        DragDropPreview.Show();
                    }

                    var dependencyObject = (DependencyObject)sender;
                    var dragDropEffects = DragDropHelper.GetDragDropEffects((DependencyObject)sender);
                    if (DragDropEffect == null)
                    {
                        DragDropEffect = new RSDragDropEffect();
                        DragDropEffect.DragDropEffects = dragDropEffects;
                        DragDropEffect.Show();
                    }


                    dragDropEffects = DragDrop.DoDragDrop(dependencyObject, DragSource, dragDropEffects);


                    DragDropPreview?.Close();
                    DragDropEffect?.Close();
                    DragDropPreview = null;
                    DragDropEffect = null;
                    DragSource = null;
                    IsDraging = false;
                }
            }
        }


        private static void DragTarget_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var dragTarget = sender as UIElement;
            dragTarget?.ReleaseMouseCapture();
        }

        private static void DragTarget_GiveFeedback(object sender, GiveFeedbackEventArgs e)
        {
            e.UseDefaultCursors = false;
            DragDropEffect.UpdateDragDropEffects(e.Effects);
            e.Handled = true;
        }

        private static void DragTarget_QueryContinueDrag(object sender, QueryContinueDragEventArgs e)
        {
            Mouse.OverrideCursor = Cursors.Arrow;
            UpdateDragPosition();
        }

        private static void UpdateDragPosition()
        {
            POINT cursorPosition = new POINT();
            NativeMethods.GetCursorPos(cursorPosition);

            if (DragDropEffect != null)
            {
                var dragDropEffectWidth = DragDropEffect.ActualWidth;
                var dragDropEffectHeight = DragDropEffect.ActualHeight;
                double dragDropEffectLeft = cursorPosition.x - dragDropEffectWidth / 2D;
                double dragDropEffectTop = cursorPosition.y - dragDropEffectHeight;
                DragDropEffect.Left = dragDropEffectLeft;
                DragDropEffect.Top = dragDropEffectTop;
            }

            if (DragDropPreview != null)
            {
                var dragDropPreviewWidth = DragDropPreview.ActualWidth;
                var dragDropPreviewHeight = DragDropPreview.ActualHeight;
                double dragDropPreviewLeft = cursorPosition.x - DragSourceVector.X;
                double dragDropPreviewTop = cursorPosition.y - DragSourceVector.Y;
                DragDropPreview.Left = dragDropPreviewLeft;
                DragDropPreview.Top = dragDropPreviewTop;
            }
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

        private static void DropTarget_DragEnter(object sender, DragEventArgs e)
        {

        }

        private static void DropTarget_DragOver(object sender, DragEventArgs e)
        {

        }

        private static void DropTarget_DragLeave(object sender, DragEventArgs e)
        {

        }


        private static void DropTarget_Drop(object sender, DragEventArgs e)
        {
            if (DragSource == null)
            {
                return;
            }
            var dropTarget = sender as UIElement;
            DependencyObject directlyOver = Mouse.DirectlyOver as DependencyObject;
            //POINT cursorPosition = new POINT();
            //NativeMethods.GetCursorPos(cursorPosition);
            //var point = e.GetPosition(dragTarget);
            //var sdf = GetAllControlsAtPoint(dragTarget, point);
            //RSAdorner.GetUIElementUnderMouse<Button>(dragTarget, point);
            //var elementUnderMouse = GetElementUnderMouse(dropTarget);
            RemoveDragSource(DragSource);
            if (dropTarget is Panel dropPanel)
            {
             var sdf=   dropPanel.GetMouseOverChildInfo();
             
                dropPanel.Children.Insert(0, DragSource);
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
                        var itemsSource = itemsControl.ItemsSource??itemsControl.Items;
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
