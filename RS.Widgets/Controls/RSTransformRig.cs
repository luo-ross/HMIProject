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
        private Thumb? PART_MoveThumb;
        #endregion

        private Grid? PART_Root;

        private double InitialRotationOffset = 0;

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


        public event EventHandler<double>? RotationRequested;
        public event EventHandler<double>? RotationCompleted;
        public event EventHandler<Vector>? TranslationRequested;
        public event EventHandler<ResizeEventArgs>? ResizeRequested;
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
            this.PART_Root = this.GetTemplateChild(nameof(this.PART_Root)) as Grid;
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


            if (this.PART_BottomRightRotate != null)
            {
                this.PART_BottomRightRotate.MouseEnter += PART_BottomRightRotate_MouseEnter;
                this.PART_BottomRightRotate.DragStarted += Rotation_DragStarted;
                this.PART_BottomRightRotate.DragDelta += PART_BottomRightRotate_DragDelta;
                this.PART_BottomRightRotate.DragCompleted += Rotation_DragCompleted;
            }

            if (this.PART_BottomLeftRotate != null)
            {
                this.PART_BottomLeftRotate.MouseEnter += PART_BottomLeftRotate_MouseEnter;
                this.PART_BottomLeftRotate.DragStarted += Rotation_DragStarted;
                this.PART_BottomLeftRotate.DragDelta += PART_BottomLeftRotate_DragDelta;
                this.PART_BottomLeftRotate.DragCompleted += Rotation_DragCompleted;
            }

            if (this.PART_TopRightRotate != null)
            {
                this.PART_TopRightRotate.MouseEnter += PART_TopRightRotate_MouseEnter;
                this.PART_TopRightRotate.DragStarted += Rotation_DragStarted;
                this.PART_TopRightRotate.DragDelta += PART_TopRightRotate_DragDelta;
                this.PART_TopRightRotate.DragCompleted += Rotation_DragCompleted;
            }

            if (this.PART_TopLeftRotate != null)
            {
                this.PART_TopLeftRotate.MouseEnter += PART_TopLeftRotate_MouseEnter;
                this.PART_TopLeftRotate.DragStarted += Rotation_DragStarted;
                this.PART_TopLeftRotate.DragDelta += PART_TopLeftRotate_DragDelta;
                this.PART_TopLeftRotate.DragCompleted += Rotation_DragCompleted;
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
                this.PART_RectDirectionArrow.DragStarted += Rotation_DragStarted;
                this.PART_RectDirectionArrow.DragDelta += PART_RectDirectionArrow_DragDelta;
                this.PART_RectDirectionArrow.DragCompleted += Rotation_DragCompleted;
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
            Rotate_DragDelta(sender, e);
        }

        private void Rotation_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            RotationCompleted?.Invoke(this, this.RotationAngle);
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
            Rotate_DragDelta(sender, e);
        }
        private void PART_BottomRightRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomRightRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.BottomRight);
        }

        private void PART_BottomLeftRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Rotate_DragDelta(sender, e);
        }
        private void PART_BottomLeftRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomLeftRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.BottomLeft);
        }

        private void PART_TopRightRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Rotate_DragDelta(sender, e);
        }
        private void PART_TopRightRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopRightRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.TopRight);
        }

        private void PART_TopLeftRotate_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Rotate_DragDelta(sender, e);
        }
        private void PART_TopLeftRotate_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopLeftRotate.Cursor = this.GetRotationCursor(ResizeGripDirection.TopLeft);
        }


        private void PART_BottomRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.BottomRight, delta));
            ApplySelfResize(ResizeGripDirection.BottomRight, delta);
        }

        private void PART_BottomRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomRight.Cursor = this.GetResizeCursor(ResizeGripDirection.BottomRight);
        }

        private void PART_BottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.BottomLeft, delta));
            ApplySelfResize(ResizeGripDirection.BottomLeft, delta);
        }

        private void PART_BottomLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomLeft.Cursor = this.GetResizeCursor(ResizeGripDirection.BottomLeft);
        }

        private void PART_TopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.TopRight, delta));
            ApplySelfResize(ResizeGripDirection.TopRight, delta);
        }

        private void PART_TopRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopRight.Cursor = this.GetResizeCursor(ResizeGripDirection.TopRight);
        }

        private void PART_TopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.TopLeft, delta));
            ApplySelfResize(ResizeGripDirection.TopLeft, delta);
        }

        private void PART_TopLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopLeft.Cursor = this.GetResizeCursor(ResizeGripDirection.TopLeft);
        }

        private void PART_Right_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Right, delta));
            ApplySelfResize(ResizeGripDirection.Right, delta);
        }

        private void PART_Right_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Right.Cursor = this.GetResizeCursor(ResizeGripDirection.Right);
        }

        private void PART_Left_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Left, delta));
            ApplySelfResize(ResizeGripDirection.Left, delta);
        }

        private void PART_Left_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Left.Cursor = this.GetResizeCursor(ResizeGripDirection.Left);
        }

        private void PART_Bottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Bottom, delta));
            ApplySelfResize(ResizeGripDirection.Bottom, delta);
        }

        private void PART_Bottom_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Bottom.Cursor = this.GetResizeCursor(ResizeGripDirection.Bottom);
        }

        private void PART_Top_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = new Vector(e.HorizontalChange, e.VerticalChange);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Top, delta));
            ApplySelfResize(ResizeGripDirection.Top, delta);
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
            // 如果 Rig 旋转了，Thumb 的局部 X/Y 也会随之旋转。
            // 我们需要将这些增量转换回父级的（直立）空间，以便元素在屏幕上保持对齐移动。
            Vector screenDelta;
            if (this.PART_Root?.RenderTransform is RotateTransform rt)
            {
                // 根据当前角度旋转局部增量向量
                Vector localDelta = new Vector(e.HorizontalChange, e.VerticalChange);
                Matrix matrix = rt.Value;
                screenDelta = matrix.Transform(localDelta);
            }
            else
            {
                screenDelta = new Vector(e.HorizontalChange, e.VerticalChange);
            }

            TranslationRequested?.Invoke(this, screenDelta);

            // 如果在 Canvas 中，执行自主移动逻辑
            if (VisualTreeHelper.GetParent(this) is Canvas canvas)
            {
                double left = Canvas.GetLeft(this);
                double top = Canvas.GetTop(this);
                if (double.IsNaN(left))
                {
                    left = 0;
                }
                if (double.IsNaN(top))
                {
                    top = 0;
                }
                Canvas.SetLeft(this, left + screenDelta.X);
                Canvas.SetTop(this, top + screenDelta.Y);
            }
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


        private void ApplySelfResize(ResizeGripDirection direction, Vector delta)
        {
            if (!(VisualTreeHelper.GetParent(this) is Canvas canvas))
            {
                return;
            }

            // 1. 确定在本次缩放期间应保持固定的局部锚点。
            // 这些点是相对于尺寸更改 *之前* 的 Rig 左上角 (0,0) 的。
            Point anchorLocal;
            switch (direction)
            {
                case ResizeGripDirection.TopLeft:
                    anchorLocal = new Point(this.Width, this.Height);
                    break;
                case ResizeGripDirection.Top:
                    anchorLocal = new Point(this.Width / 2, this.Height);
                    break;
                case ResizeGripDirection.TopRight:
                    anchorLocal = new Point(0, this.Height);
                    break;
                case ResizeGripDirection.Left:
                    anchorLocal = new Point(this.Width, this.Height / 2);
                    break;
                case ResizeGripDirection.Right:
                    anchorLocal = new Point(0, this.Height / 2);
                    break;
                case ResizeGripDirection.BottomLeft:
                    anchorLocal = new Point(this.Width, 0);
                    break;
                case ResizeGripDirection.Bottom:
                    anchorLocal = new Point(this.Width / 2, 0);
                    break;
                case ResizeGripDirection.BottomRight:
                    anchorLocal = new Point(0, 0);
                    break;
                default:
                    return;
            }

            // 2. 捕获该锚点当前的屏幕位置。
            double left = Canvas.GetLeft(this);
            double top = Canvas.GetTop(this);
            if (double.IsNaN(left))
            {
                left = 0;
            }
            if (double.IsNaN(top))
            {
                top = 0;
            }

            Point center = new Point(left + this.Width / 2, top + this.Height / 2);
            Matrix rotMatrix = Matrix.Identity;
            rotMatrix.RotateAt(this.RotationAngle, center.X, center.Y);
            Point anchorScreen = rotMatrix.Transform(new Point(left + anchorLocal.X, top + anchorLocal.Y));

            // 3. 在本地更新 Rig 的尺寸。
            // 'delta' 已经在局部坐标空间中，因为 Thumb 是旋转后的 PART_Root 的子级。
            double dw = 0, dh = 0;
            switch (direction)
            {
                case ResizeGripDirection.TopLeft:
                    dw = -delta.X;
                    dh = -delta.Y;
                    break;
                case ResizeGripDirection.Top:
                    dh = -delta.Y;
                    break;
                case ResizeGripDirection.TopRight:
                    dw = delta.X;
                    dh = -delta.Y;
                    break;
                case ResizeGripDirection.Left:
                    dw = -delta.X;
                    break;
                case ResizeGripDirection.Right:
                    dw = delta.X;
                    break;
                case ResizeGripDirection.BottomLeft:
                    dw = -delta.X;
                    dh = delta.Y;
                    break;
                case ResizeGripDirection.Bottom:
                    dh = delta.Y;
                    break;
                case ResizeGripDirection.BottomRight:
                    dw = delta.X;
                    dh = delta.Y;
                    break;
            }

            if (this.Width + dw > 20)
            {
                this.Width += dw;
            }
            if (this.Height + dh > 20)
            {
                this.Height += dh;
            }

            // 4. 计算新的 Canvas 位置，使锚点保持在其捕获的屏幕位置。
            // 尺寸更改后的新局部锚点位置
            Point anchorLocalNew;
            switch (direction)
            {
                case ResizeGripDirection.TopLeft:
                    anchorLocalNew = new Point(this.Width, this.Height);
                    break;
                case ResizeGripDirection.Top:
                    anchorLocalNew = new Point(this.Width / 2, this.Height);
                    break;
                case ResizeGripDirection.TopRight:
                    anchorLocalNew = new Point(0, this.Height);
                    break;
                case ResizeGripDirection.Left:
                    anchorLocalNew = new Point(this.Width, this.Height / 2);
                    break;
                case ResizeGripDirection.Right:
                    anchorLocalNew = new Point(0, this.Height / 2);
                    break;
                case ResizeGripDirection.BottomLeft:
                    anchorLocalNew = new Point(this.Width, 0);
                    break;
                case ResizeGripDirection.Bottom:
                    anchorLocalNew = new Point(this.Width / 2, 0);
                    break;
                case ResizeGripDirection.BottomRight:
                    anchorLocalNew = new Point(0, 0);
                    break;
                default:
                    return;
            }

            // 新中心点到新锚点在局部空间中的偏移量
            Point centerRelAnchorNew = new Point(this.Width / 2 - anchorLocalNew.X, this.Height / 2 - anchorLocalNew.Y);

            // 将此偏移量旋转当前角度，以获得从面锚点到中心的屏幕空间偏移量
            Matrix rotOnly = Matrix.Identity;
            rotOnly.Rotate(this.RotationAngle);
            Vector screenOffsetToCenter = rotOnly.Transform(new Vector(centerRelAnchorNew.X, centerRelAnchorNew.Y));

            // Canvas 空间中的新中心点
            Point centerNew = new Point(anchorScreen.X + screenOffsetToCenter.X, anchorScreen.Y + screenOffsetToCenter.Y);

            // 根据新中心点设置新的左上角位置
            Canvas.SetLeft(this, centerNew.X - this.Width / 2);
            Canvas.SetTop(this, centerNew.Y - this.Height / 2);
        }


        private void Rotation_DragStarted(object sender, DragStartedEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this) as UIElement;
            if (parent == null)
            {
                return;
            }

            Point centerLocal = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
            Point centerInParent = this.TranslatePoint(centerLocal, parent);
            Point mousePos = Mouse.GetPosition(parent);

            double radians = Math.Atan2(mousePos.Y - centerInParent.Y, mousePos.X - centerInParent.X);
            double currentMouseAngle = radians * (180 / Math.PI);

            // 捕获绝对鼠标角度与当前 RotationAngle 之间的偏移量
            InitialRotationOffset = (currentMouseAngle + 90) - this.RotationAngle;
        }

        private void Rotate_DragDelta(object sender, DragDeltaEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(this) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 计算父坐标系中 Rig 的中心点
            Point centerLocal = new Point(this.ActualWidth / 2, this.ActualHeight / 2);
            Point centerInParent = this.TranslatePoint(centerLocal, parent);

            // 获取相对于父级的当前鼠标位置
            Point mousePos = Mouse.GetPosition(parent);

            // 计算当前鼠标角度
            double radians = Math.Atan2(mousePos.Y - centerInParent.Y, mousePos.X - centerInParent.X);
            double currentMouseAngle = radians * (180 / Math.PI);

            // 使用捕获的偏移量计算最终旋转角度
            double finalRotation = (currentMouseAngle + 90 - InitialRotationOffset) % 360;
            if (finalRotation < 0)
            {
                finalRotation += 360;
            }

            // 更新本地属性以进行视觉反馈
            this.RotationAngle = finalRotation;

            // 通知监听器 (TransformAdorner) 更新实际元素
            RotationRequested?.Invoke(this, finalRotation);
        }



    }
}

