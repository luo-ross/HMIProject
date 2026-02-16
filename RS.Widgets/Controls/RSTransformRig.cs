using CommunityToolkit.Mvvm.Input;
using RS.Widgets.CustomEventArgs;
using RS.Widgets.Enums;
using RS.Widgets.Structs;
using RS.Widgets.Utilities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Resources;
using System.Windows.Shell;

namespace RS.Widgets.Controls
{
    public class RSTransformRig : Control
    {

        #region Move
        private Thumb PART_MoveThumb;
        #endregion

        #region Resize
        private Thumb PART_Top;
        private Thumb PART_Bottom;
        private Thumb PART_Left;
        private Thumb PART_Right;
        private Thumb PART_TopLeft;
        private Thumb PART_TopRight;
        private Thumb PART_BottomLeft;
        private Thumb PART_BottomRight;
        #endregion

        #region Rotation
        private Thumb PART_TopLeftRotate;
        private Thumb PART_TopRightRotate;
        private Thumb PART_BottomLeftRotate;
        private Thumb PART_BottomRightRotate;
        #endregion


        #region Direction
        private Button PART_RectDirectionTop;
        private Button PART_RectDirectionLeft;
        private Button PART_RectDirectionRight;
        private Button PART_RectDirectionBottom;
        #endregion

        #region RotateDirectionArrow
        private Thumb PART_RectDirectionArrow;
        #endregion


        private static readonly CursorData BaseRotationCursorData;
        private static readonly CursorData BaseResizeCursorData;


        public event EventHandler<double> RotationRequested;
        public event EventHandler<Vector> TranslationRequested;
        public event EventHandler<ResizeEventArgs> ResizeRequested;
        static RSTransformRig()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSTransformRig), new FrameworkPropertyMetadata(typeof(RSTransformRig)));

            // 加载自定义旋转光标
            var resourceStream = Application.GetResourceStream(new Uri("pack://application:,,,/RS.Widgets;component/Assets/Rotation.cur"));
            if (resourceStream != null)
            {
                BaseRotationCursorData = CursorHelper.GetCursorData(new Cursor(resourceStream.Stream));
                BaseRotationCursorData.HotspotX = (int)(BaseRotationCursorData.Bitmap.Width / 2);
                BaseRotationCursorData.HotspotY = (int)(BaseRotationCursorData.Bitmap.Height / 2);
            }

            // 使用系统原生的 SizeNS (上下缩放) 光标作为所有旋转缩放的基础
            BaseResizeCursorData = CursorHelper.GetCursorData(Cursors.SizeNS);
        }

        public RSTransformRig()
        {
        }

        public double RotationAngle
        {
            get
            {
                return (double)GetValue(RotationAngleProperty);
            }
            set
            {
                SetValue(RotationAngleProperty, value);
            }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register(nameof(RotationAngle), typeof(double), typeof(RSTransformRig), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnRotationAnglePropertyChanged));

        private static void OnRotationAnglePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {

        }



        /// <summary>
        /// 矩形开口方向
        /// </summary>
        public RectDirection RectDirection
        {
            get { return (RectDirection)GetValue(RectDirectionProperty); }
            set { SetValue(RectDirectionProperty, value); }
        }

        public static readonly DependencyProperty RectDirectionProperty =
            DependencyProperty.Register(nameof(RectDirection), typeof(RectDirection), typeof(RSTransformRig), new PropertyMetadata(RectDirection.Top));




        public double ScaleX
        {
            get { return (double)GetValue(ScaleXProperty); }
            set { SetValue(ScaleXProperty, value); }
        }

        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register(nameof(ScaleX), typeof(double), typeof(RSTransformRig), new PropertyMetadata(1D));





        public double ScaleY
        {
            get { return (double)GetValue(ScaleYProperty); }
            set { SetValue(ScaleYProperty, value); }
        }

        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register(nameof(ScaleY), typeof(double), typeof(RSTransformRig), new PropertyMetadata(1D));






        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_MoveThumb = this.GetTemplateChild(nameof(this.PART_MoveThumb)) as Thumb;
            this.PART_Top = this.GetTemplateChild(nameof(this.PART_Top)) as Thumb;
            this.PART_Bottom = this.GetTemplateChild(nameof(this.PART_Bottom)) as Thumb;
            this.PART_Left = this.GetTemplateChild(nameof(this.PART_Left)) as Thumb;
            this.PART_Right = this.GetTemplateChild(nameof(this.PART_Right)) as Thumb;
            this.PART_TopLeft = this.GetTemplateChild(nameof(this.PART_TopLeft)) as Thumb;
            this.PART_TopRight = this.GetTemplateChild(nameof(this.PART_TopRight)) as Thumb;
            this.PART_BottomLeft = this.GetTemplateChild(nameof(this.PART_BottomLeft)) as Thumb;
            this.PART_BottomRight = this.GetTemplateChild(nameof(this.PART_BottomRight)) as Thumb;

            this.PART_TopLeftRotate = this.GetTemplateChild(nameof(this.PART_TopLeftRotate)) as Thumb;
            this.PART_TopRightRotate = this.GetTemplateChild(nameof(this.PART_TopRightRotate)) as Thumb;
            this.PART_BottomLeftRotate = this.GetTemplateChild(nameof(this.PART_BottomLeftRotate)) as Thumb;
            this.PART_BottomRightRotate = this.GetTemplateChild(nameof(this.PART_BottomRightRotate)) as Thumb;

            this.PART_RectDirectionTop = this.GetTemplateChild(nameof(this.PART_RectDirectionTop)) as Button;
            this.PART_RectDirectionLeft = this.GetTemplateChild(nameof(this.PART_RectDirectionLeft)) as Button;
            this.PART_RectDirectionRight = this.GetTemplateChild(nameof(this.PART_RectDirectionRight)) as Button;
            this.PART_RectDirectionBottom = this.GetTemplateChild(nameof(this.PART_RectDirectionBottom)) as Button;

            this.PART_RectDirectionArrow = this.GetTemplateChild(nameof(this.PART_RectDirectionArrow)) as Thumb;




            if (this.PART_MoveThumb != null)
            {
                this.PART_MoveThumb.MouseEnter += PART_MoveThumb_MouseEnter;
                this.PART_MoveThumb.DragDelta += PART_MoveThumb_DragDelta;
            }

            if (this.PART_Top != null)
            {
                this.PART_Top.MouseEnter += PART_Top_MouseEnter;
                this.PART_Top.DragDelta += PART_Top_DragDelta;
            }


            if (this.PART_Bottom != null)
            {
                this.PART_Bottom.MouseEnter += PART_Bottom_MouseEnter;
                this.PART_Bottom.DragDelta += PART_Bottom_DragDelta;
            }


            if (this.PART_Left != null)
            {
                this.PART_Left.MouseEnter += PART_Left_MouseEnter;
                this.PART_Left.DragDelta += PART_Left_DragDelta;
            }



            if (this.PART_Right != null)
            {
                this.PART_Right.MouseEnter += PART_Right_MouseEnter;
                this.PART_Right.DragDelta += PART_Right_DragDelta;
            }

            if (this.PART_TopLeft != null)
            {
                this.PART_TopLeft.MouseEnter += PART_TopLeft_MouseEnter;
                this.PART_TopLeft.DragDelta += PART_TopLeft_DragDelta;
            }

            if (this.PART_TopRight != null)
            {
                this.PART_TopRight.MouseEnter += PART_TopRight_MouseEnter;
                this.PART_TopRight.DragDelta += PART_TopRight_DragDelta;
            }

            if (this.PART_BottomLeft != null)
            {
                this.PART_BottomLeft.MouseEnter += PART_BottomLeft_MouseEnter;
                this.PART_BottomLeft.DragDelta += PART_BottomLeft_DragDelta;
            }

            if (this.PART_BottomRight != null)
            {
                this.PART_BottomRight.MouseEnter += PART_BottomRight_MouseEnter;
                this.PART_BottomRight.DragDelta += PART_BottomRight_DragDelta;
            }


            if (this.PART_TopLeftRotate != null)
            {
                this.PART_TopLeftRotate.MouseEnter += PART_TopLeftRotate_MouseEnter;
                this.PART_TopLeftRotate.DragDelta += PART_TopLeftRotate_DragDelta;
            }

            if (this.PART_TopRightRotate != null)
            {
                this.PART_TopRightRotate.MouseEnter += PART_TopRightRotate_MouseEnter;
                this.PART_TopRightRotate.DragDelta += PART_TopRightRotate_DragDelta;
            }

            if (this.PART_BottomLeftRotate != null)
            {
                this.PART_BottomLeftRotate.MouseEnter += PART_BottomLeftRotate_MouseEnter;
                this.PART_BottomLeftRotate.DragDelta += PART_BottomLeftRotate_DragDelta;
            }

            if (this.PART_BottomRightRotate != null)
            {
                this.PART_BottomRightRotate.MouseEnter += PART_BottomRightRotate_MouseEnter;
                this.PART_BottomRightRotate.DragDelta += PART_BottomRightRotate_DragDelta;
            }

            if (this.PART_RectDirectionTop != null)
            {
                this.PART_RectDirectionTop.Click += PART_RectDirectionTop_Click;
            }

            if (this.PART_RectDirectionLeft != null)
            {
                this.PART_RectDirectionLeft.Click += PART_RectDirectionLeft_Click;
            }

            if (this.PART_RectDirectionRight != null)
            {
                this.PART_RectDirectionRight.Click += PART_RectDirectionRight_Click;
            }

            if (this.PART_RectDirectionBottom != null)
            {
                this.PART_RectDirectionBottom.Click += PART_RectDirectionBottom_Click;
            }

            if (this.PART_RectDirectionArrow != null)
            {
                this.PART_RectDirectionArrow.MouseEnter += PART_RectDirectionArrow_MouseEnter;
                this.PART_RectDirectionArrow.DragDelta += PART_RectDirectionArrow_DragDelta;
            }
        }

        private void PART_RectDirectionArrow_MouseEnter(object sender, MouseEventArgs e)
        {
            switch (this.RectDirection)
            {
                case RectDirection.Top:
                    this.PART_RectDirectionArrow.Cursor = this.GetRotationCursor(ResizeGripDirection.Top);
                    break;
                case RectDirection.Bottom:
                    this.PART_RectDirectionArrow.Cursor = this.GetRotationCursor(ResizeGripDirection.Bottom);
                    break;
                case RectDirection.Left:
                    this.PART_RectDirectionArrow.Cursor = this.GetRotationCursor(ResizeGripDirection.Left);
                    break;
                case RectDirection.Right:
                    this.PART_RectDirectionArrow.Cursor = this.GetRotationCursor(ResizeGripDirection.Right);
                    break;
            }
        }

        private void PART_RectDirectionArrow_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_RectDirectionBottom_Click(object sender, RoutedEventArgs e)
        {
            this.RectDirection = RectDirection.Bottom;
        }

        private void PART_RectDirectionRight_Click(object sender, RoutedEventArgs e)
        {
            this.RectDirection = RectDirection.Right;
        }

        private void PART_RectDirectionLeft_Click(object sender, RoutedEventArgs e)
        {
            this.RectDirection = RectDirection.Left;
        }

        private void PART_RectDirectionTop_Click(object sender, RoutedEventArgs e)
        {
            this.RectDirection = RectDirection.Top;
        }

        private void PART_BottomRightRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_BottomRightRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomRightRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.BottomRight);
        }

        private void PART_BottomLeftRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_BottomLeftRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomLeftRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.BottomLeft);
        }

        private void PART_TopRightRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_TopRightRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopRightRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.TopRight);
        }

        private void PART_TopLeftRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_TopLeftRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopLeftRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.TopLeft);
        }

        private void PART_BottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_BottomRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomRight.Cursor = this.GetResizeCursor(ResizeGripDirection.BottomRight);
        }

        private void PART_BottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_BottomLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomLeft.Cursor = this.GetResizeCursor(ResizeGripDirection.BottomLeft);
        }

        private void PART_TopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_TopRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopRight.Cursor = this.GetResizeCursor(ResizeGripDirection.TopRight);
        }

        private void PART_TopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_TopLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopLeft.Cursor = this.GetResizeCursor(ResizeGripDirection.TopLeft);
        }

        private void PART_Right_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_Right_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Right.Cursor = this.GetResizeCursor(ResizeGripDirection.Right);
        }

        private void PART_Left_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_Left_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Left.Cursor = this.GetResizeCursor(ResizeGripDirection.Left);
        }

        private void PART_Bottom_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_Bottom_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Bottom.Cursor = this.GetResizeCursor(ResizeGripDirection.Bottom);
        }

        private void PART_Top_DragDelta(object sender, DragDeltaEventArgs e)
        {

        }

        private void PART_Top_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Top.Cursor = this.GetResizeCursor(ResizeGripDirection.Top);
        }

        private void PART_MoveThumb_MouseEnter(object sender, MouseEventArgs e)
        {

        }

        private void PART_MoveThumb_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //double scale = ScaleHelper.GetScale(AdornedElement);
            //Canvas.SetLeft(AdornedElement, Canvas.GetLeft(AdornedElement) + e.HorizontalChange / this.ScaleX);
            //Canvas.SetTop(AdornedElement, Canvas.GetTop(AdornedElement) + e.VerticalChange / this.ScaleY);
        }


      
        private Cursor GetResizeCursor(ResizeGripDirection resizeGripDirection)
        {
            if (BaseResizeCursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }


            double vAngle = RotationAngle;
            CursorData cursorData = new CursorData();
            switch (resizeGripDirection)
            {
                case ResizeGripDirection.None:
                    break;
                case ResizeGripDirection.TopLeft:
                case ResizeGripDirection.BottomRight:
                    cursorData = CursorHelper.RotateCursor(BaseResizeCursorData, vAngle - 45);
                    break;
                case ResizeGripDirection.Top:
                case ResizeGripDirection.Bottom:
                    cursorData = CursorHelper.RotateCursor(BaseResizeCursorData, vAngle);
                    break;
                case ResizeGripDirection.TopRight:
                case ResizeGripDirection.BottomLeft:
                    cursorData = CursorHelper.RotateCursor(BaseResizeCursorData, vAngle + 45);
                    break;
                case ResizeGripDirection.Left:
                case ResizeGripDirection.Right:
                    cursorData = CursorHelper.RotateCursor(BaseResizeCursorData, vAngle + 90);
                    break;
            }

            if (cursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            return CursorHelper.CreateCursor(cursorData.Bitmap, cursorData.HotspotX, cursorData.HotspotY);
        }


        private Cursor GetRotationCursor(ResizeGripDirection resizeGripDirection)
        {
            if (BaseRotationCursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            double vAngle = RotationAngle;

            CursorData cursorData = new CursorData();
            switch (resizeGripDirection)
            {
                case ResizeGripDirection.None:
                    break;
                case ResizeGripDirection.TopLeft:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle - 90);
                    break;
                case ResizeGripDirection.BottomRight:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle + 90);
                    break;
                case ResizeGripDirection.Top:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle - 45);
                    break;
                case ResizeGripDirection.Bottom:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle + 135);
                    break;
                case ResizeGripDirection.TopRight:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle);
                    break;
                case ResizeGripDirection.BottomLeft:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle + 180);
                    break;
                case ResizeGripDirection.Left:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle + 235);
                    break;
                case ResizeGripDirection.Right:
                    cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, vAngle + 45);
                    break;
            }

            if (cursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            return CursorHelper.CreateCursor(cursorData.Bitmap, cursorData.HotspotX, cursorData.HotspotY);
        }


        private void ApplyResize(double dWidth, double dHeight, double dLeft, double dTop)
        {
            //TransformHelper.CalculateTotalScale(this, out double totalScaleX, out double totalScaleY);

            //if (dWidth != 0)
            //{
            //    double oldW = this.ActualWidth;
            //    double newW = Math.Max(20, oldW + dWidth / this.ScaleX);
            //    AdornedElement.Width = newW;
            //    if (dLeft != 0) Canvas.SetLeft(AdornedElement, Canvas.GetLeft(AdornedElement) - (newW - oldW));
            //}

            //if (dHeight != 0)
            //{
            //    double oldH = AdornedElement.ActualHeight;
            //    double newH = Math.Max(20, oldH + dHeight / scale);
            //    AdornedElement.Height = newH;
            //    if (dTop != 0) Canvas.SetTop(AdornedElement, Canvas.GetTop(AdornedElement) - (newH - oldH));
            //}
        }


        private void Rotate_DragDelta(object sender, DragDeltaEventArgs e)
        {
            //if (AdornedElement == null) return;

            //// 获取 AdornerLayer，这是计算旋转的“稳定”坐标系
            //var adornerLayer = AdornerLayer.GetAdornerLayer(AdornedElement);
            //if (adornerLayer == null) return;

            //// 1. 获取元素中心在 AdornerLayer 中的位置
            //Point centerInElement = new Point(AdornedElement.ActualWidth / 2, AdornedElement.ActualHeight / 2);
            //Point centerInAdorner = AdornedElement.TranslatePoint(centerInElement, adornerLayer);

            //// 2. 获取当前鼠标在 AdornerLayer 中的位置
            //Point mouseInAdorner = Mouse.GetPosition(adornerLayer);

            //// 3. 计算鼠标相对于中心点的角度 (弧度 -> 角度)
            //double angle = Math.Atan2(mouseInAdorner.Y - centerInAdorner.Y, mouseInAdorner.X - centerInAdorner.X) * 180 / Math.PI;

            //// 4. 更新依赖属性 (Atan2 = 0 是向右，我们需要映射 0 为向上，所以 + 90)
            //RotationAngle = angle + 90;
        }



    }
}

