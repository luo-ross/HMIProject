using NPOI.POIFS.Properties;
using RS.Widgets.Adorners;
using RS.Widgets.Controls;
using RS.Widgets.CustomEventArgs;
using RS.Widgets.Services;
using RS.Widgets.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Shell;
using System.Xml.Linq;

namespace RS.Widgets.Adorners
{
    public class TransformAdorner : Adorner
    {
        private readonly RSTransformRig TransformRig;
        private readonly ScaleTransform InverseScale = new ScaleTransform(1, 1);
        private static TransformSelectService TransformSelectService;
        static TransformAdorner()
        {
            TransformSelectService = new TransformSelectService();
        }

        public TransformAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            TransformRig = new RSTransformRig();
            TransformRig.LayoutTransform = InverseScale;
            AddVisualChild(TransformRig);

            //this.LayoutUpdated += TransformAdorner_LayoutUpdated;


            //adornedElement.SizeChanged += AdornedElement_SizeChanged;
            //adornedElement.LayoutUpdated += AdornedElement_LayoutUpdated;
            //adornedElement.RenderSizeChanged += AdornedElement_RenderSizeChanged;
            //this.LayoutUpdated += (s, e) =>
            //{
            //    Console.WriteLine("LayoutUpdated");
            //};


            //this.OriginalCursor = this.Cursor;
            //_transformControl.RotationRequested += TransformControl_RotationRequested;
            //_transformControl.TranslationRequested += TransformControl_TranslationRequested;
            //_transformControl.ResizeRequested += TransformControl_ResizeRequested;
            //var cursorImage = LoadCursorImage();
            //var rotated = CursorHelper.RotateBitmapSource(cursorImage, CurrentRotation);
            //this.Cursor = CursorHelper.CreateCursor(rotated) ?? OriginalCursor;
            //this.MouseLeftButtonDown += TransformAdorner_MouseLeftButtonDown;
            //var descriptor = DependencyPropertyDescriptor.FromProperty(ScaleTransform.ScaleXProperty, typeof(FrameworkElement));
            //descriptor?.AddValueChanged(adornedElement, (s, e) =>
            //{
            //    Console.WriteLine("AddValueChanged");
            //    UpdateInverseScale();
            //});
            //this.UpdateInverseScale();
        }

        private void TransformAdorner_LayoutUpdated(object? sender, EventArgs e)
        {
            TransformHelper.CalculateTotalScale(this.AdornedElement, out double totalScaleX, out double totalScaleY);
            UpdateInverseScale(totalScaleX, totalScaleY);
            
            //Console.WriteLine($"TransformAdorner_LayoutUpdated{totalScaleX}{totalScaleY}");
        }

        private void AdornedElement_LayoutUpdated(object? sender, EventArgs e)
        {
            //Console.WriteLine($"AdornedElement_LayoutUpdated{DateTime.Now}");
        }

        private void AdornedElement_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //Console.WriteLine("AdornedElement_SizeChanged");
        }

        private void UpdateInverseScale(double scaleX,  double scaleY)
        {
          
            //if (scaleX > 0)
            //{
            //    InverseScale.ScaleX = 1D / scaleX;
            //}
            //if (scaleY > 0)
            //{
            //    InverseScale.ScaleY = 1D / scaleY;
            //}

            Console.WriteLine(InverseScale.ScaleX);
            this.InvalidateMeasure();
            this.InvalidateArrange();
        }


        private void TransformControl_RotationRequested(object sender, double delta)
        {
            var currentAngle = TransformHelper.GetRotation(AdornedElement);
            TransformHelper.SetRotation(AdornedElement, (currentAngle + delta) % 360);
            UpdateControlState();
        }

        private void TransformControl_TranslationRequested(object sender, Vector delta)
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement);
            if (parent is Canvas)
            {
                var x = TransformHelper.GetCanvasX(AdornedElement);
                var y = TransformHelper.GetCanvasY(AdornedElement);
                TransformHelper.SetCanvasX(AdornedElement, x + delta.X);
                TransformHelper.SetCanvasY(AdornedElement, y + delta.Y);
            }
            else
            {
                var x = TransformHelper.GetTransformX(AdornedElement);
                var y = TransformHelper.GetTransformY(AdornedElement);
                TransformHelper.SetTransformX(AdornedElement, x + delta.X);
                TransformHelper.SetTransformY(AdornedElement, y + delta.Y);
            }
        }

        private void TransformControl_ResizeRequested(object sender, ResizeEventArgs e)
        {
            if (AdornedElement is FrameworkElement fe)
            {
                double dw = 0, dh = 0, dx = 0, dy = 0;

                switch (e.Direction)
                {
                    case ResizeGripDirection.TopLeft:
                        {
                            dx = e.Delta.X; dy = e.Delta.Y; dw = -e.Delta.X; dh = -e.Delta.Y;
                            break;
                        }
                    case ResizeGripDirection.Top:
                        {
                            dy = e.Delta.Y; dh = -e.Delta.Y;
                            break;
                        }
                    case ResizeGripDirection.TopRight:
                        {
                            dy = e.Delta.Y; dw = e.Delta.X; dh = -e.Delta.Y;
                            break;
                        }
                    case ResizeGripDirection.Left:
                        {
                            dx = e.Delta.X; dw = -e.Delta.X;
                            break;
                        }
                    case ResizeGripDirection.Right:
                        {
                            dw = e.Delta.X;
                            break;
                        }
                    case ResizeGripDirection.BottomLeft:
                        {
                            dx = e.Delta.X; dw = -e.Delta.X; dh = e.Delta.Y;
                            break;
                        }
                    case ResizeGripDirection.Bottom:
                        {
                            dh = e.Delta.Y;
                            break;
                        }
                    case ResizeGripDirection.BottomRight:
                        {
                            dw = e.Delta.X; dh = e.Delta.Y;
                            break;
                        }
                }

                // Update Size
                if (fe.Width + dw > 0)
                {
                    fe.Width += dw;
                }
                if (fe.Height + dh > 0)
                {
                    fe.Height += dh;
                }

                // Update Position if needed
                if (dx != 0 || dy != 0)
                {
                    TransformControl_TranslationRequested(this, new Vector(dx, dy));
                }
            }
        }

        private void UpdateControlState()
        {
            this.TransformRig.RotationAngle = TransformHelper.GetRotation(AdornedElement);
        }

        protected override int VisualChildrenCount
        {
            get
            {
                return 1;
            }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.TransformRig;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            TransformHelper.CalculateTotalScale(this.AdornedElement, out double totalScaleX, out double totalScaleY);
            this.TransformRig.Measure(new Size(AdornedElement.RenderSize.Width* totalScaleX, AdornedElement.RenderSize.Height* totalScaleY));
            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            TransformHelper.CalculateTotalScale(this.AdornedElement, out double totalScaleX, out double totalScaleY);
            this.TransformRig.Arrange(new Rect(0, 0, finalSize.Width* totalScaleX, finalSize.Height* totalScaleY));
            return finalSize;
        }


        public bool IsSelect
        {
            get
            {
                return (bool)GetValue(IsSelectProperty);
            }
            set
            {
                SetValue(IsSelectProperty, value);
            }
        }

        public static readonly DependencyProperty IsSelectProperty =
            DependencyProperty.Register(nameof(IsSelect), typeof(bool), typeof(TransformAdorner), new PropertyMetadata(false, OnIsSelectPropertyChanged));

        private static void OnIsSelectPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var transformAdorner = d as TransformAdorner;
            //if (transformAdorner != null)
            //{
            //    transformAdorner._transformControl.Visibility = (bool)e.NewValue ? Visibility.Visible : Visibility.Collapsed;
            //    if ((bool)e.NewValue)
            //    {
            //        transformAdorner.SyncFromElement();
            //    }
            //    transformAdorner.InvalidateVisual();
            //}
        }

        private void SyncFromElement()
        {
            UpdateControlState();
            // Initial sync of properties if they haven't been set yet
            if (AdornedElement is FrameworkElement fe)
            {
                // We should probably read actual values if attached ones are 0, 
                // but for now let's assume the attached properties are the source of truth.
            }
        }

        private void TransformAdorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TransformSelectService.SingleSelect(this);
            this.InvalidateVisual();
        }

        //protected override void OnRender(DrawingContext drawingContext)
        //{
        //    // We no longer draw handles here, TransformControl handles it
        //    if (!this.IsSelect)
        //    {
        //        return;
        //    }
        //    Rect renderRect = new Rect(AdornedElement.RenderSize);
        //    Pen pen = new Pen(new SolidColorBrush((Color)ColorConverter.ConvertFromString("#00E5FF")), 1.5);
        //    drawingContext.DrawRectangle(null, pen, renderRect);
        //}



    }
}
