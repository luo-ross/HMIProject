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
using System.Windows.Controls.Primitives;
using System.Windows.Data;
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
        private double _globalScaleX = 1;
        private double _globalScaleY = 1;
        private Size VisualPixelSize = new Size(0, 0);

        private static TransformSelectService TransformSelectService;
        static TransformAdorner()
        {
            TransformSelectService = new TransformSelectService();
        }

        public TransformAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            TransformRig = new RSTransformRig();
            AddVisualChild(TransformRig);

            // Sync Rotation
            Binding rotationBinding = new Binding();
            rotationBinding.Source = adornedElement;
            rotationBinding.Path = new PropertyPath(TransformHelper.RotationProperty);
            rotationBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(TransformRig, RSTransformRig.RotationAngleProperty, rotationBinding);

            // Sync ScaleX
            Binding scaleXBinding = new Binding();
            scaleXBinding.Source = adornedElement;
            scaleXBinding.Path = new PropertyPath(TransformHelper.ScaleXProperty);
            scaleXBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(TransformRig, RSTransformRig.ScaleXProperty, scaleXBinding);

            // Sync ScaleY
            Binding scaleYBinding = new Binding();
            scaleYBinding.Source = adornedElement;
            scaleYBinding.Path = new PropertyPath(TransformHelper.ScaleYProperty);
            scaleYBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(TransformRig, RSTransformRig.ScaleYProperty, scaleYBinding);

            this.TransformRig.TranslationRequested += TransformRig_TranslationRequested;
            this.TransformRig.ResizeRequested += TransformRig_ResizeRequested;
        }

        // Note: TransformRig_RotationRequested is no longer needed because the Binding handles it TwoWay.

        private void TransformRig_TranslationRequested(object sender, Vector delta)
        {
            UpdateVisualScale(); // Ensure _globalScaleX/Y are fresh
            
            double dx = delta.X / _globalScaleX;
            double dy = delta.Y / _globalScaleY;

            var parent = VisualTreeHelper.GetParent(AdornedElement);
            if (parent is Canvas)
            {
                var x = TransformHelper.GetCanvasX(AdornedElement);
                var y = TransformHelper.GetCanvasY(AdornedElement);
                TransformHelper.SetCanvasX(AdornedElement, x + dx);
                TransformHelper.SetCanvasY(AdornedElement, y + dy);
            }
            else
            {
                var x = TransformHelper.GetTransformX(AdornedElement);
                var y = TransformHelper.GetTransformY(AdornedElement);
                TransformHelper.SetTransformX(AdornedElement, x + dx);
                TransformHelper.SetTransformY(AdornedElement, y + dy);
            }
        }

        private void TransformRig_ResizeRequested(object sender, ResizeEventArgs e)
        {
            if (AdornedElement is FrameworkElement fe)
            {
                UpdateVisualScale(); // Ensure _globalScaleX/Y are fresh

                double dw_scaled = 0, dh_scaled = 0, dx_scaled = 0, dy_scaled = 0;
                double sdx = e.Delta.X;
                double sdy = e.Delta.Y;

                switch (e.Direction)
                {
                    case ResizeGripDirection.TopLeft:
                        dx_scaled = sdx / _globalScaleX; dy_scaled = sdy / _globalScaleY; 
                        dw_scaled = -sdx / _globalScaleX; dh_scaled = -sdy / _globalScaleY;
                        break;
                    case ResizeGripDirection.Top:
                        dy_scaled = sdy / _globalScaleY; dh_scaled = -sdy / _globalScaleY;
                        break;
                    case ResizeGripDirection.TopRight:
                        dy_scaled = sdy / _globalScaleY; dw_scaled = sdx / _globalScaleX; dh_scaled = -sdy / _globalScaleY;
                        break;
                    case ResizeGripDirection.Left:
                        dx_scaled = sdx / _globalScaleX; dw_scaled = -sdx / _globalScaleX;
                        break;
                    case ResizeGripDirection.Right:
                        dw_scaled = sdx / _globalScaleX;
                        break;
                    case ResizeGripDirection.BottomLeft:
                        dx_scaled = sdx / _globalScaleX; dw_scaled = -sdx / _globalScaleX; dh_scaled = sdy / _globalScaleY;
                        break;
                    case ResizeGripDirection.Bottom:
                        dh_scaled = sdy / _globalScaleY;
                        break;
                    case ResizeGripDirection.BottomRight:
                        dw_scaled = sdx / _globalScaleX; dh_scaled = sdy / _globalScaleY;
                        break;
                }

                if (fe.Width + dw_scaled > 0) fe.Width += dw_scaled;
                if (fe.Height + dh_scaled > 0) fe.Height += dh_scaled;

                if (dx_scaled != 0 || dy_scaled != 0)
                {
                    TransformRig_TranslationRequested(this, new Vector(dx_scaled * _globalScaleX, dy_scaled * _globalScaleY));
                }
            }
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

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            if (transform is Transform t)
            {
                Matrix matrix = t.Value;
                // Strip all except translation to keep Adorner upright and 1:1
                return new MatrixTransform(new Matrix(1, 0, 0, 1, matrix.OffsetX, matrix.OffsetY));
            }

            return transform;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UpdateVisualScale();
            this.TransformRig.Measure(VisualPixelSize);
            return AdornedElement.RenderSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateVisualScale();
            this.TransformRig.Arrange(new Rect(new Point(0, 0), VisualPixelSize));
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
        }



        private void TransformAdorner_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            TransformSelectService.SingleSelect(this);
            this.InvalidateVisual();
        }

        private void UpdateVisualScale()
        {
            if (AdornedElement == null)
            {
                return;
            }

            Visual? root = null;
            PresentationSource source = PresentationSource.FromVisual(AdornedElement);
            if (source != null)
            {
                root = source.RootVisual;
            }

            if (root == null)
            {
                root = AdornedElement.TryFindParent<Visual>();
            }

            if (root == null || root == AdornedElement)
            {
                return;
            }

            GeneralTransform elementTransform = AdornedElement.TransformToAncestor(root);
            if (elementTransform != null)
            {

                Point p0 = new Point(0, 0);
                Point pW = new Point(AdornedElement.RenderSize.Width, 0);
                Point pH = new Point(0, AdornedElement.RenderSize.Height);

                Point tp0 = elementTransform.Transform(p0);
                Point tpW = elementTransform.Transform(pW);
                Point tpH = elementTransform.Transform(pH);

                double pixelWidth = Math.Sqrt(Math.Pow(tpW.X - tp0.X, 2) + Math.Pow(tpW.Y - tp0.Y, 2));
                double pixelHeight = Math.Sqrt(Math.Pow(tpH.X - tp0.X, 2) + Math.Pow(tpH.Y - tp0.Y, 2));

                    if (pixelWidth > 0 && pixelHeight > 0)
                    {
                        if (Math.Abs(VisualPixelSize.Width - pixelWidth) > 1e-6 || Math.Abs(VisualPixelSize.Height - pixelHeight) > 1e-6)
                        {
                            VisualPixelSize = new Size(pixelWidth, pixelHeight);
                            
                            // Calculate global scale for interaction math
                            _globalScaleX = pixelWidth / AdornedElement.RenderSize.Width;
                            _globalScaleY = pixelHeight / AdornedElement.RenderSize.Height;

                            TransformRig.LayoutTransform = Transform.Identity;
                        }
                    }
            }


        }


    }
}
