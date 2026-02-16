using RS.Widgets.Adorners;
using RS.Widgets.Converters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Xml.Linq;

namespace RS.Widgets.Controls
{
    public class TransformHelper
    {
        public static readonly DependencyProperty IsEditableProperty =
            DependencyProperty.RegisterAttached(
                "IsEditable",
                typeof(bool),
                typeof(TransformHelper),
                new FrameworkPropertyMetadata(false, OnIsEditablePropertyChanged));

        private static void OnIsEditablePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element == null)
            {
                return;
            }
            if (element.IsLoaded)
            {
                UpdateTransformAdorner(element, (bool)e.NewValue);
            }
            else
            {
                element.Loaded += TargetElement_Loaded;
            }
        }

        private static void TargetElement_Unloaded(object sender, RoutedEventArgs e)
        {
        }

        private static void TargetElement_Loaded(object sender, RoutedEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            if (frameworkElement == null)
            {
                return;
            }
            var isEditable = GetIsEditable(frameworkElement);
            UpdateTransformAdorner(frameworkElement, isEditable);
            UpdateScaleIndependent(frameworkElement, isEditable);   
        }

        public static void UpdateScaleIndependent(FrameworkElement element, bool isEditable)
        {
            if (element == null)
            {
                return;
            }
            if (isEditable)
            {
                var transform = new ScaleTransform();
                var binding = new Binding
                {
                    Path = new PropertyPath(ScaleXProperty),
                    RelativeSource = new RelativeSource(RelativeSourceMode.Self),
                    Converter = new ScaleConverter() 
                };
                BindingOperations.SetBinding(transform, ScaleTransform.ScaleXProperty, binding);
                BindingOperations.SetBinding(transform, ScaleTransform.ScaleYProperty, binding);
                element.LayoutTransform = transform;
            }
            else
            {
                element.LayoutTransform = Transform.Identity;
            }
        }

        public static void UpdateTransformAdorner(FrameworkElement element, bool isEditable)
        {
            if (element==null)
            {
                return;
            }

            var adornerLayer = AdornerLayer.GetAdornerLayer(element);
            if (adornerLayer == null)
            {
                return;
            }
            RemoveTransformAdorner(adornerLayer, element);
            if (isEditable)
            {
                var transformAdorner = new TransformAdorner(element);
                adornerLayer.Add(transformAdorner);
            }
        }

        private static void RemoveTransformAdorner(AdornerLayer adornerLayer, UIElement element)
        {
            if (adornerLayer == null)
            {
                return;
            }

            var adorners = adornerLayer.GetAdorners(element);
            if (adorners != null)
            {
                foreach (var adorner in adorners)
                {
                    if (adorner is TransformAdorner)
                    {
                        adornerLayer.Remove(adorner);
                    }
                }
            }
        }

        public static bool GetIsEditable(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEditableProperty);
        }

        public static void SetIsEditable(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEditableProperty, value);
        }

        #region RenderTransform 旋转和平移   

        public static readonly DependencyProperty RotationProperty =
            DependencyProperty.RegisterAttached("Rotation", typeof(double), typeof(TransformHelper), new PropertyMetadata(0.0, OnTransformPropertyChanged));

        public static double GetRotation(DependencyObject obj)
        {
            return (double)obj.GetValue(RotationProperty);
        }

        public static void SetRotation(DependencyObject obj, double value)
        {
            obj.SetValue(RotationProperty, value);
        }


        public static readonly DependencyProperty TransformXProperty =
            DependencyProperty.RegisterAttached("TransformX", typeof(double), typeof(TransformHelper), new PropertyMetadata(0.0, OnTransformPropertyChanged));

        public static double GetTransformX(DependencyObject obj)
        {
            return (double)obj.GetValue(TransformXProperty);
        }

        public static void SetTransformX(DependencyObject obj, double value)
        {
            obj.SetValue(TransformXProperty, value);
        }

        public static readonly DependencyProperty TransformYProperty =
            DependencyProperty.RegisterAttached("TransformY", typeof(double), typeof(TransformHelper), new PropertyMetadata(0.0, OnTransformPropertyChanged));

        public static double GetTransformY(DependencyObject obj)
        {
            return (double)obj.GetValue(TransformYProperty);
        }

        public static void SetTransformY(DependencyObject obj, double value)
        {
            obj.SetValue(TransformYProperty, value);
        }

        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.RegisterAttached("ScaleX", typeof(double), typeof(TransformHelper), new PropertyMetadata(1.0, OnTransformPropertyChanged));

        public static double GetScaleX(DependencyObject obj)
        {
            return (double)obj.GetValue(ScaleXProperty);
        }

        public static void SetScaleX(DependencyObject obj, double value)
        {
            obj.SetValue(ScaleXProperty, value);
        }

        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.RegisterAttached("ScaleY", typeof(double), typeof(TransformHelper), new PropertyMetadata(1.0, OnTransformPropertyChanged));

        public static double GetScaleY(DependencyObject obj)
        {
            return (double)obj.GetValue(ScaleYProperty);
        }

        public static void SetScaleY(DependencyObject obj, double value)
        {
            obj.SetValue(ScaleYProperty, value);
        }


        private static void OnTransformPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                UpdateRenderTransform(element);
            }
        }

        private static void UpdateRenderTransform(FrameworkElement element)
        {
            var rotation = GetRotation(element);
            var x = GetTransformX(element);
            var y = GetTransformY(element);
            var sx = GetScaleX(element);
            var sy = GetScaleY(element);

            var group = element.RenderTransform as TransformGroup;
            if (group == null)
            {
                group = new TransformGroup();
                if (element.RenderTransform != null && element.RenderTransform != Transform.Identity)
                {
                    group.Children.Add(element.RenderTransform);
                }
                element.RenderTransform = group;
                element.RenderTransformOrigin = new Point(0.5, 0.5);
            }

            var rotateTransform = group.Children.OfType<RotateTransform>().FirstOrDefault();
            if (rotateTransform == null)
            {
                rotateTransform = new RotateTransform(0);
                group.Children.Add(rotateTransform);
            }
            rotateTransform.Angle = rotation;

            var scaleTransform = group.Children.OfType<ScaleTransform>().FirstOrDefault();
            if (scaleTransform == null)
            {
                scaleTransform = new ScaleTransform(1, 1);
                group.Children.Add(scaleTransform);
            }
            scaleTransform.ScaleX = sx;
            scaleTransform.ScaleY = sy;

            var translateTransform = group.Children.OfType<TranslateTransform>().FirstOrDefault();
            if (translateTransform == null)
            {
                translateTransform = new TranslateTransform(0, 0);
                group.Children.Add(translateTransform);
            }
            translateTransform.X = x;
            translateTransform.Y = y;
        }

        #endregion

        #region Canvas 位置

        public static readonly DependencyProperty CanvasXProperty =
            DependencyProperty.RegisterAttached("CanvasX", typeof(double), typeof(TransformHelper), new PropertyMetadata(0.0, OnCanvasPropertyChanged));

        public static double GetCanvasX(DependencyObject obj)
        {
            return (double)obj.GetValue(CanvasXProperty);
        }

        public static void SetCanvasX(DependencyObject obj, double value)
        {
            obj.SetValue(CanvasXProperty, value);
        }

        public static readonly DependencyProperty CanvasYProperty =
            DependencyProperty.RegisterAttached("CanvasY", typeof(double), typeof(TransformHelper), new PropertyMetadata(0.0, OnCanvasPropertyChanged));

        public static double GetCanvasY(DependencyObject obj)
        {
            return (double)obj.GetValue(CanvasYProperty);
        }

        public static void SetCanvasY(DependencyObject obj, double value)
        {
            obj.SetValue(CanvasYProperty, value);
        }

        private static void OnCanvasPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is FrameworkElement element)
            {
                if (e.Property == CanvasXProperty)
                {
                    Canvas.SetLeft(element, (double)e.NewValue);
                }
                else if (e.Property == CanvasYProperty)
                {
                    Canvas.SetTop(element, (double)e.NewValue);
                }
            }
        }


        #endregion


        /// <summary>
        /// 计算元素在视觉树中的综合缩放比例（包含所有父级的缩放）
        /// </summary>
        /// <param name="element">目标元素</param>
        /// <param name="totalScaleX">输出X轴综合缩放比例</param>
        /// <param name="totalScaleY">输出Y轴综合缩放比例</param>
        public static void CalculateTotalScale(DependencyObject element, out double totalScaleX, out double totalScaleY)
        {
            // 初始缩放比例为1（无缩放）
            totalScaleX = 1.0;
            totalScaleY = 1.0;
            if (element == null)
            {
                return;
            }
            // 向上遍历所有父级元素（包括自身）
            while (element != null)
            {
                if (element is FrameworkElement uiElement)
                {
                    GetElementScale(uiElement, out double scaleX, out double scaleY);
                    totalScaleX *= scaleX;
                    totalScaleY *= scaleY;
                }

                element = VisualTreeHelper.GetParent(element);
            }

            totalScaleX = Math.Abs(totalScaleX - 1) < 1e-6 ? 1 : totalScaleX;
            totalScaleY = Math.Abs(totalScaleY - 1) < 1e-6 ? 1 : totalScaleY;
        }

        /// <summary>
        /// 解析单个元素的RenderTransform中的缩放比例（仅提取ScaleTransform）
        /// </summary>
        /// <param name="element">单个元素</param>
        /// <param name="scaleX">X轴缩放</param>
        /// <param name="scaleY">Y轴缩放</param>
        public static void GetElementScale(FrameworkElement element, out double scaleX, out double scaleY)
        {
            scaleX = 1.0;
            scaleY = 1.0;

            if (element.LayoutTransform == null || element.LayoutTransform == Transform.Identity)
            {
                return;
            }

            if (element.LayoutTransform is ScaleTransform scale)
            {
                scaleX = scale.ScaleX;
                scaleY = scale.ScaleY;
            }
            else if (element.LayoutTransform is TransformGroup group)
            {
                foreach (var transform in group.Children)
                {
                    if (transform is ScaleTransform groupScale)
                    {
                        scaleX = groupScale.ScaleX;
                        scaleY = groupScale.ScaleY;
                        break;
                    }
                }
            }
            scaleX = scaleX == 0 ? 1 : scaleX;
            scaleY = scaleY == 0 ? 1 : scaleY;
        }

    }
}
