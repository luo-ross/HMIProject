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
        private FrameworkElement AdornedFE
        {
            get { return AdornedElement as FrameworkElement; }
        }
        
        private double GlobalScaleX = 1;
        private double GlobalScaleY = 1;
        private Size VisualPixelSize = new Size(0, 0);


        
        // 缩放期间的快照状态，用于消除累积误差（Capture Once Strategy）
        private Point InitialAnchorInParent;
        private double InitialWidth;
        private double InitialHeight;
        private Matrix InitialTransformMatrix;

        // 缩放期间锁定 GlobalScale 防止抖动
        private bool IsResizing = false;
        
        // 累计拖动增量（因为 ResizeEventArgs 给的是增量，我们需要总和）
        private Vector AccResizeDelta;

        static TransformAdorner()
        {
        }

        public TransformAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            TransformRig = new RSTransformRig();
            TransformRig.IsAutonomous = false;
            AddVisualChild(TransformRig);
            this.Focusable = true; // 启用焦点以支持键盘输入

            // 使用 PreviewMouseLeftButtonDown 在 Thumb 吞掉事件之前捕获焦点
            this.PreviewMouseLeftButtonDown += TransformAdorner_PreviewMouseLeftButtonDown;

            // 同步旋转
            Binding rotationBinding = new Binding();
            rotationBinding.Source = adornedElement;
            rotationBinding.Path = new PropertyPath(TransformHelper.RotationProperty);
            rotationBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(TransformRig, RSTransformRig.RotationAngleProperty, rotationBinding);

            // 同步 ScaleX
            Binding scaleXBinding = new Binding();
            scaleXBinding.Source = adornedElement;
            scaleXBinding.Path = new PropertyPath(TransformHelper.ScaleXProperty);
            scaleXBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(TransformRig, RSTransformRig.ScaleXProperty, scaleXBinding);

            // 同步 ScaleY
            Binding scaleYBinding = new Binding();
            scaleYBinding.Source = adornedElement;
            scaleYBinding.Path = new PropertyPath(TransformHelper.ScaleYProperty);
            scaleYBinding.Mode = BindingMode.TwoWay;
            BindingOperations.SetBinding(TransformRig, RSTransformRig.ScaleYProperty, scaleYBinding);

            this.TransformRig.TranslationRequested += TransformRig_TranslationRequested;
            this.TransformRig.ResizeRequested += TransformRig_ResizeRequested;
            this.TransformRig.ResizeStarted += TransformRig_ResizeStarted;
            this.TransformRig.ResizeCompleted += TransformRig_ResizeCompleted;
        }

        // 注意：TransformRig_RotationRequested 不再需要，因为 Binding 已经处理了 TwoWay 同步。



        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);
            
            if (AdornedFE == null)
            {
                return;
            }



            double step = 10.0; // 粗调
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                step = 1.0; // 微调
            }

            Vector delta = new Vector(0, 0);
            switch (e.Key)
            {
                case Key.Left:
                    delta.X = -step;
                    break;
                case Key.Right:
                    delta.X = step;
                    break;
                case Key.Up:
                    delta.Y = -step;
                    break;
                case Key.Down:
                    delta.Y = step;
                    break;
                default:
                    return;
            }

            ApplyTranslation(delta);
            e.Handled = true;
        }

        private void ApplyTranslation(Vector delta)
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 为了平移手感灵敏，位移比例应基于“父容器相对于屏幕的缩放”
            PresentationSource source = PresentationSource.FromVisual(parent);
            Matrix matrixScreen = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
            
            // 叠加父容器到根节点的物理变换
            GeneralTransform transformToRoot = parent.TransformToAncestor(source?.RootVisual ?? parent);
            if (transformToRoot is Transform t)
            {
                matrixScreen.Append(t.Value);
            }

            double scaleX = Math.Sqrt(matrixScreen.M11 * matrixScreen.M11 + matrixScreen.M12 * matrixScreen.M12);
            double scaleY = Math.Sqrt(matrixScreen.M21 * matrixScreen.M21 + matrixScreen.M22 * matrixScreen.M22);

            double dx = delta.X / (scaleX > 0 ? scaleX : 1.0);
            double dy = delta.Y / (scaleY > 0 ? scaleY : 1.0);

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

        private void TransformRig_TranslationRequested(object sender, Vector delta)
        {
            ApplyTranslation(delta);
        }

        private void TransformRig_ResizeStarted(object sender, ResizeGripDirection direction)
        {
            if (AdornedFE == null)
            {
                return;
            }

            // 锁定缩放状态，防止 UpdateVisualScale 在拖动过程中重新计算 GlobalScale
            IsResizing = true;
            AccResizeDelta = new Vector(0, 0);
            UpdateVisualScale();

            // 1. 确定锚点（缩放期间保持不动的逻辑点）
            Point anchorLocal;
            switch (direction)
            {
                case ResizeGripDirection.TopLeft:
                    anchorLocal = new Point(AdornedFE.ActualWidth, AdornedFE.ActualHeight);
                    break;
                case ResizeGripDirection.Top:
                    anchorLocal = new Point(AdornedFE.ActualWidth / 2, AdornedFE.ActualHeight);
                    break;
                case ResizeGripDirection.TopRight:
                    anchorLocal = new Point(0, AdornedFE.ActualHeight);
                    break;
                case ResizeGripDirection.Left:
                    anchorLocal = new Point(AdornedFE.ActualWidth, AdornedFE.ActualHeight / 2);
                    break;
                case ResizeGripDirection.Right:
                    anchorLocal = new Point(0, AdornedFE.ActualHeight / 2);
                    break;
                case ResizeGripDirection.BottomLeft:
                    anchorLocal = new Point(AdornedFE.ActualWidth, 0);
                    break;
                case ResizeGripDirection.Bottom:
                    anchorLocal = new Point(AdornedFE.ActualWidth / 2, 0);
                    break;
                case ResizeGripDirection.BottomRight:
                    anchorLocal = new Point(0, 0);
                    break;
                default:
                    return;
            }

            // 2. 捕获初始快照
            var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
            if (parent == null)
            {
                return;
            }

            GeneralTransform transformToParent = AdornedFE.TransformToVisual(parent);
            InitialAnchorInParent = transformToParent.Transform(anchorLocal);
            InitialWidth = double.IsNaN(AdornedFE.Width) ? AdornedFE.ActualWidth : AdornedFE.Width;
            InitialHeight = double.IsNaN(AdornedFE.Height) ? AdornedFE.ActualHeight : AdornedFE.Height;
            
            // 捕获不含偏移的变换矩阵，用于后续推算中心点
            Matrix mat = (transformToParent as Transform)?.Value ?? Matrix.Identity;
            mat.OffsetX = 0;
            mat.OffsetY = 0;
            InitialTransformMatrix = mat;
        }

        private void TransformRig_ResizeCompleted(object sender, ResizeGripDirection e)
        {
            IsResizing = false;
            UpdateVisualScale();
        }

        private void TransformRig_ResizeRequested(object sender, ResizeEventArgs e)
        {
            if (AdornedFE == null)
            {
                return;
            }

            var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 累加 Delta
            AccResizeDelta += e.Delta;

            // 1. 根据总 Delta 计算新尺寸（相对于初始快照）
            double dw = 0, dh = 0;
            switch (e.Direction)
            {
                case ResizeGripDirection.TopLeft:
                    dw = -AccResizeDelta.X;
                    dh = -AccResizeDelta.Y;
                    break;
                case ResizeGripDirection.Top:
                    dh = -AccResizeDelta.Y;
                    break;
                case ResizeGripDirection.TopRight:
                    dw = AccResizeDelta.X;
                    dh = -AccResizeDelta.Y;
                    break;
                case ResizeGripDirection.Left:
                    dw = -AccResizeDelta.X;
                    break;
                case ResizeGripDirection.Right:
                    dw = AccResizeDelta.X;
                    break;
                case ResizeGripDirection.BottomLeft:
                    dw = -AccResizeDelta.X;
                    dh = AccResizeDelta.Y;
                    break;
                case ResizeGripDirection.Bottom:
                    dh = AccResizeDelta.Y;
                    break;
                case ResizeGripDirection.BottomRight:
                    dw = AccResizeDelta.X;
                    dh = AccResizeDelta.Y;
                    break;
            }

            double dw_logical = dw / GlobalScaleX;
            double dh_logical = dh / GlobalScaleY;

            double newW = InitialWidth + dw_logical;
            double newH = InitialHeight + dh_logical;

            if (newW < 1)
            {
                newW = 1;
            }
            if (newH < 1)
            {
                newH = 1;
            }

            if (newW < AdornedFE.MinWidth)
            {
                newW = AdornedFE.MinWidth;
            }
            if (newW > AdornedFE.MaxWidth)
            {
                newW = AdornedFE.MaxWidth;
            }
            if (newH < AdornedFE.MinHeight)
            {
                newH = AdornedFE.MinHeight;
            }
            if (newH > AdornedFE.MaxHeight)
            {
                newH = AdornedFE.MaxHeight;
            }

            AdornedFE.Width = newW;
            AdornedFE.Height = newH;

            // 修正 AccResizeDelta 以匹配受限后的尺寸
            // 这样当用户反向拖动时，会立即响应，而不是先“走完”超出的虚空距离
            double clamped_dw = (newW - InitialWidth) * GlobalScaleX;
            double clamped_dh = (newH - InitialHeight) * GlobalScaleY;

            switch (e.Direction)
            {
                case ResizeGripDirection.TopLeft:
                    AccResizeDelta.X = -clamped_dw;
                    AccResizeDelta.Y = -clamped_dh;
                    break;
                case ResizeGripDirection.Top:
                    AccResizeDelta.Y = -clamped_dh;
                    break;
                case ResizeGripDirection.TopRight:
                    AccResizeDelta.X = clamped_dw;
                    AccResizeDelta.Y = -clamped_dh;
                    break;
                case ResizeGripDirection.Left:
                    AccResizeDelta.X = -clamped_dw;
                    break;
                case ResizeGripDirection.Right:
                    AccResizeDelta.X = clamped_dw;
                    break;
                case ResizeGripDirection.BottomLeft:
                    AccResizeDelta.X = -clamped_dw;
                    AccResizeDelta.Y = clamped_dh;
                    break;
                case ResizeGripDirection.Bottom:
                    AccResizeDelta.Y = clamped_dh;
                    break;
                case ResizeGripDirection.BottomRight:
                    AccResizeDelta.X = clamped_dw;
                    AccResizeDelta.Y = clamped_dh;
                    break;
            }

            // 2. 重新计算布局中心，确保锚点绝对不动
            Point anchorLocalNew;
            switch (e.Direction)
            {
                case ResizeGripDirection.TopLeft:
                    anchorLocalNew = new Point(newW, newH);
                    break;
                case ResizeGripDirection.Top:
                    anchorLocalNew = new Point(newW / 2, newH);
                    break;
                case ResizeGripDirection.TopRight:
                    anchorLocalNew = new Point(0, newH);
                    break;
                case ResizeGripDirection.Left:
                    anchorLocalNew = new Point(newW, newH / 2);
                    break;
                case ResizeGripDirection.Right:
                    anchorLocalNew = new Point(0, newH / 2);
                    break;
                case ResizeGripDirection.BottomLeft:
                    anchorLocalNew = new Point(newW, 0);
                    break;
                case ResizeGripDirection.Bottom:
                    anchorLocalNew = new Point(newW / 2, 0);
                    break;
                case ResizeGripDirection.BottomRight:
                    anchorLocalNew = new Point(0, 0);
                    break;
                default:
                    return;
            }

            // 基于初始矩阵推导新中心点
            Point centerRelAnchorLocalNew = new Point(newW / 2 - anchorLocalNew.X, newH / 2 - anchorLocalNew.Y);
            Vector offsetToCenterInParent = InitialTransformMatrix.Transform(new Vector(centerRelAnchorLocalNew.X, centerRelAnchorLocalNew.Y));

            Point centerInParentNew = new Point(InitialAnchorInParent.X + offsetToCenterInParent.X, InitialAnchorInParent.Y + offsetToCenterInParent.Y);

            // 更新布局
            if (parent is Canvas)
            {
                TransformHelper.SetCanvasX(AdornedFE, centerInParentNew.X - newW / 2 - AdornedFE.Margin.Left);
                TransformHelper.SetCanvasY(AdornedFE, centerInParentNew.Y - newH / 2 - AdornedFE.Margin.Top);
            }
            else
            {
                TransformHelper.SetTransformX(AdornedFE, centerInParentNew.X - newW / 2 - AdornedFE.Margin.Left);
                TransformHelper.SetTransformY(AdornedFE, centerInParentNew.Y - newH / 2 - AdornedFE.Margin.Top);
            }
        }

        protected override int VisualChildrenCount
        {
            get { return 1; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return this.TransformRig;
        }

        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            if (AdornedFE == null)
            {
                return transform;
            }

            // 获取元素的中心点坐标（相对于元素自身）
            // 在 Resize 过程中，RenderSize 可能会滞后于 actual Width/Height，导致中心点计算偏差，
            // Adorner 的位置出现漂移。如果 Width/Height 已设置，优先使用它们。
            double w = AdornedFE.RenderSize.Width;
            double h = AdornedFE.RenderSize.Height;

            if (!double.IsNaN(AdornedFE.Width) && AdornedFE.Width > 0)
            {
                w = AdornedFE.Width;
            }
            if (!double.IsNaN(AdornedFE.Height) && AdornedFE.Height > 0)
            {
                h = AdornedFE.Height;
            }
            
            Point centerLocal = new Point(w / 2, h / 2);

            // transform (通常是 MatrixTransform) 包含了从被装饰元素到装饰层的完整变换（含旋转、缩放等）
            if (transform is Transform t)
            {
                // 获取元素中心在装饰层坐标系（直立空间）下的位置
                Point centerInParent = t.Transform(centerLocal);

                // 我们希望 RSTransformRig 的中心对准 centerInParent。
                // 由于 RSTransformRig 的排列尺寸 is VisualPixelSize，它的中心偏移应该是 VisualPixelSize / 2。
                double offsetX = centerInParent.X - VisualPixelSize.Width / 2;
                double offsetY = centerInParent.Y - VisualPixelSize.Height / 2;

                return new MatrixTransform(new Matrix(1, 0, 0, 1, offsetX, offsetY));
            }

            return transform;
        }

        protected override Size MeasureOverride(Size constraint)
        {
            UpdateVisualScale();
            this.TransformRig.Measure(VisualPixelSize);
            return AdornedFE?.RenderSize ?? Size.Empty;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            UpdateVisualScale();
            this.TransformRig.Arrange(new Rect(new Point(0, 0), VisualPixelSize));
            return finalSize;
        }




        private void TransformAdorner_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.TransformRig.Select(e);
            this.Focus();
            this.InvalidateVisual();
        }

        private void UpdateVisualScale()
        {
            if (AdornedFE == null)
            {
                return;
            }

            Visual? root = null;
            PresentationSource source = PresentationSource.FromVisual(AdornedFE);
            if (source != null)
            {
                root = source.RootVisual;
            }

            if (root == null)
            {
                root = AdornedFE.TryFindParent<Visual>();
            }

            if (root == null || root == AdornedFE)
            {
                return;
            }

            // 如果正在拖动缩放，则锁定 Scale 不变，只更新 PixelSize
            // 这样可以避免布局浮点数引起的抖动
            if (IsResizing)
            {
                double logicalW = double.IsNaN(AdornedFE.Width) ? AdornedFE.ActualWidth : AdornedFE.Width;
                double logicalH = double.IsNaN(AdornedFE.Height) ? AdornedFE.ActualHeight : AdornedFE.Height;
                if (logicalW > 0 && logicalH > 0)
                {
                    VisualPixelSize = new Size(logicalW * GlobalScaleX, logicalH * GlobalScaleY);
                }
                return;
            }

            GeneralTransform elementTransform = AdornedFE.TransformToAncestor(root);
            if (elementTransform != null)
            {
                // 计算 GlobalScale 时避开 RenderSize 陷阱。
                // 我们直接计算 (0,0) 和 (Width, Height) 在物理屏幕上的真实跨度。
                double logicalW = double.IsNaN(AdornedFE.Width) ? AdornedFE.ActualWidth : AdornedFE.Width;
                double logicalH = double.IsNaN(AdornedFE.Height) ? AdornedFE.ActualHeight : AdornedFE.Height;
                if (logicalW <= 0 || logicalH <= 0)
                {
                    return;
                }

                Point p0 = new Point(0, 0);
                Point pW = new Point(logicalW, 0);
                Point pH = new Point(0, logicalH);

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
                        
                        // GlobalScale 定义：每一逻辑单位 Width/Height 对应多少物理像素。
                        GlobalScaleX = pixelWidth / logicalW;
                        GlobalScaleY = pixelHeight / logicalH;

                        TransformRig.LayoutTransform = Transform.Identity;
                    }
                }
            }
        }
    }
}
