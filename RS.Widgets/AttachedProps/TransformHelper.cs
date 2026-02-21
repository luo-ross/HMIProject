using RS.Widgets.Adorners;
using RS.Widgets.Converters;
using RS.Widgets.Models;
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
using System.Windows.Input;
using RS.Widgets.Services;

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
        }

        public static readonly DependencyProperty IsDirectionEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsDirectionEnabled",
                typeof(bool),
                typeof(TransformHelper),
                new FrameworkPropertyMetadata(false, OnTransformBoolPropertyChanged));

        public static bool GetIsDirectionEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsDirectionEnabledProperty);
        }

        public static void SetIsDirectionEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsDirectionEnabledProperty, value);
        }

        public static readonly DependencyProperty IsRotationEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsRotationEnabled",
                typeof(bool),
                typeof(TransformHelper),
                new FrameworkPropertyMetadata(true, OnTransformBoolPropertyChanged));

        public static bool GetIsRotationEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsRotationEnabledProperty);
        }

        public static void SetIsRotationEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsRotationEnabledProperty, value);
        }

        private static void OnTransformBoolPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element != null && element.IsLoaded && GetIsEditable(element))
            {
                UpdateTransformAdorner(element, true);
            }
        }
        public static readonly DependencyProperty TransformDataProperty =
            DependencyProperty.RegisterAttached(
                "TransformData",
                typeof(TransformData),
                typeof(TransformHelper),
                new FrameworkPropertyMetadata(null, OnTransformDataPropertyChanged));

        private static void OnTransformDataPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var element = d as FrameworkElement;
            if (element != null && element.IsLoaded && GetIsEditable(element))
            {
                var adornerLayer = AdornerLayer.GetAdornerLayer(element);
                if (adornerLayer != null)
                {
                    var adorners = adornerLayer.GetAdorners(element);
                    if (adorners != null)
                    {
                        foreach (var adorner in adorners)
                        {
                            if (adorner is TransformAdorner ta)
                            {
                                ta.DataModel = e.NewValue as TransformData;
                                ta.UpdateDataModel();
                            }
                        }
                    }
                }
            }
        }

        public static TransformData GetTransformData(DependencyObject obj)
        {
            return (TransformData)obj.GetValue(TransformDataProperty);
        }

        public static void SetTransformData(DependencyObject obj, TransformData value)
        {
            obj.SetValue(TransformDataProperty, value);
        }


        // ── 命令支持 ──

        public static readonly DependencyProperty MoveStartedCommandProperty =
            DependencyProperty.RegisterAttached("MoveStartedCommand", typeof(ICommand), typeof(TransformHelper), new PropertyMetadata(null));
        public static ICommand GetMoveStartedCommand(DependencyObject obj) => (ICommand)obj.GetValue(MoveStartedCommandProperty);
        public static void SetMoveStartedCommand(DependencyObject obj, ICommand value) => obj.SetValue(MoveStartedCommandProperty, value);

        public static readonly DependencyProperty MoveCompletedCommandProperty =
            DependencyProperty.RegisterAttached("MoveCompletedCommand", typeof(ICommand), typeof(TransformHelper), new PropertyMetadata(null));
        public static ICommand GetMoveCompletedCommand(DependencyObject obj) => (ICommand)obj.GetValue(MoveCompletedCommandProperty);
        public static void SetMoveCompletedCommand(DependencyObject obj, ICommand value) => obj.SetValue(MoveCompletedCommandProperty, value);

        public static readonly DependencyProperty ResizeStartedCommandProperty =
            DependencyProperty.RegisterAttached("ResizeStartedCommand", typeof(ICommand), typeof(TransformHelper), new PropertyMetadata(null));
        public static ICommand GetResizeStartedCommand(DependencyObject obj) => (ICommand)obj.GetValue(ResizeStartedCommandProperty);
        public static void SetResizeStartedCommand(DependencyObject obj, ICommand value) => obj.SetValue(ResizeStartedCommandProperty, value);

        public static readonly DependencyProperty ResizeCompletedCommandProperty =
            DependencyProperty.RegisterAttached("ResizeCompletedCommand", typeof(ICommand), typeof(TransformHelper), new PropertyMetadata(null));
        public static ICommand GetResizeCompletedCommand(DependencyObject obj) => (ICommand)obj.GetValue(ResizeCompletedCommandProperty);
        public static void SetResizeCompletedCommand(DependencyObject obj, ICommand value) => obj.SetValue(ResizeCompletedCommandProperty, value);

        public static readonly DependencyProperty RotationStartedCommandProperty =
            DependencyProperty.RegisterAttached("RotationStartedCommand", typeof(ICommand), typeof(TransformHelper), new PropertyMetadata(null));
        public static ICommand GetRotationStartedCommand(DependencyObject obj) => (ICommand)obj.GetValue(RotationStartedCommandProperty);
        public static void SetRotationStartedCommand(DependencyObject obj, ICommand value) => obj.SetValue(RotationStartedCommandProperty, value);

        public static readonly DependencyProperty RotationCompletedCommandProperty =
            DependencyProperty.RegisterAttached("RotationCompletedCommand", typeof(ICommand), typeof(TransformHelper), new PropertyMetadata(null));
        public static ICommand GetRotationCompletedCommand(DependencyObject obj) => (ICommand)obj.GetValue(RotationCompletedCommandProperty);
        public static void SetRotationCompletedCommand(DependencyObject obj, ICommand value) => obj.SetValue(RotationCompletedCommandProperty, value);

        // ── 全局撤销/重做命令 ──

        private static RelayCommand _undoCommand;
        public static ICommand UndoCommand
        {
            get
            {
                if (_undoCommand == null)
                {
                    _undoCommand = new RelayCommand(
                        _ => TransformAdorner.UndoService.Undo(),
                        _ => TransformAdorner.UndoService.CanUndo
                    );
                    TransformAdorner.UndoService.StateChanged += (s, e) => _undoCommand.RaiseCanExecuteChanged();
                }
                return _undoCommand;
            }
        }

        private static RelayCommand _redoCommand;
        public static ICommand RedoCommand
        {
            get
            {
                if (_redoCommand == null)
                {
                    _redoCommand = new RelayCommand(
                        _ => TransformAdorner.UndoService.Redo(),
                        _ => TransformAdorner.UndoService.CanRedo
                    );
                    TransformAdorner.UndoService.StateChanged += (s, e) => _redoCommand.RaiseCanExecuteChanged();
                }
                return _redoCommand;
            }
        }

        /// <summary>
        /// 简易 RelayCommand 实现，避免库依赖过多
        /// </summary>
        private class RelayCommand : ICommand
        {
            private readonly Action<object> _execute;
            private readonly Func<object, bool> _canExecute;

            public RelayCommand(Action<object> execute, Func<object, bool> canExecute = null)
            {
                _execute = execute ?? throw new ArgumentNullException(nameof(execute));
                _canExecute = canExecute;
            }

            public bool CanExecute(object parameter) => _canExecute == null || _canExecute(parameter);

            public void Execute(object parameter) => _execute(parameter);

            public event EventHandler CanExecuteChanged;

            public void RaiseCanExecuteChanged() => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
        }

        public static void UpdateTransformAdorner(FrameworkElement element, bool isEditable)
        {
            if (element == null)
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
                var transformAdorner = new TransformAdorner(element)
                {
                    IsDirectionEnabled = GetIsDirectionEnabled(element),
                    IsRotationEnabled = GetIsRotationEnabled(element),
                    DataModel = GetTransformData(element)
                };
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

            // 强制更新装饰器层，以应对 RenderTransform 变化导致的装饰器不同步
            AdornerLayer.GetAdornerLayer(element)?.Update(element);
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
    }
}
