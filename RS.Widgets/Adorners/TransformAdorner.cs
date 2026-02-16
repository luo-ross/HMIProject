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
                double scaleX = Math.Sqrt(matrix.M11 * matrix.M11 + matrix.M12 * matrix.M12);
                double scaleY = Math.Sqrt(matrix.M21 * matrix.M21 + matrix.M22 * matrix.M22);
                if (scaleX > 0)
                {
                    matrix.M11 /= scaleX;
                    matrix.M12 /= scaleX;
                }

                if (scaleY > 0)
                {
                    matrix.M21 /= scaleY;
                    matrix.M22 /= scaleY;
                }
                return new MatrixTransform(matrix);
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
                        TransformRig.LayoutTransform = Transform.Identity;
                    }
                }
            }


        }


    }
}
