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
        public event EventHandler<ResizeGripDirection>? ResizeStarted;
        public event EventHandler<ResizeEventArgs>? ResizeRequested;
        public event EventHandler<ResizeGripDirection>? ResizeCompleted;
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
            this.Focusable = true; // 启用焦点以支持键盘输入
            this.PreviewMouseLeftButtonDown += RSTransformRig_PreviewMouseLeftButtonDown;
        }

        private void RSTransformRig_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.Focus();
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

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

            ProcessMove(delta);
            e.Handled = true;
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
            get
            {
                return (RectDirection)GetValue(RectDirectionProperty);
            }
            set
            {
                SetValue(RectDirectionProperty, value);
            }
        }

        public static readonly DependencyProperty RectDirectionProperty =
            DependencyProperty.Register(nameof(RectDirection), typeof(RectDirection), typeof(RSTransformRig), new PropertyMetadata(RectDirection.Top));




        public double ScaleX
        {
            get
            {
                return (double)GetValue(ScaleXProperty);
            }
            set
            {
                SetValue(ScaleXProperty, value);
            }
        }

        public static readonly DependencyProperty ScaleXProperty =
            DependencyProperty.Register(nameof(ScaleX), typeof(double), typeof(RSTransformRig), new PropertyMetadata(1D));





        public double ScaleY
        {
            get
            {
                return (double)GetValue(ScaleYProperty);
            }
            set
            {
                SetValue(ScaleYProperty, value);
            }
        }

        public static readonly DependencyProperty ScaleYProperty =
            DependencyProperty.Register(nameof(ScaleY), typeof(double), typeof(RSTransformRig), new PropertyMetadata(1D));

        public bool IsAutonomous
        {
            get
            {
                return (bool)GetValue(IsAutonomousProperty);
            }
            set
            {
                SetValue(IsAutonomousProperty, value);
            }
        }

        public static readonly DependencyProperty IsAutonomousProperty =
            DependencyProperty.Register(nameof(IsAutonomous), typeof(bool), typeof(RSTransformRig), new PropertyMetadata(true));






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
                this.PART_Top.DragStarted += Resize_DragStarted;
                this.PART_Top.DragDelta += PART_Top_DragDelta;
                this.PART_Top.DragCompleted += Resize_DragCompleted;
            }


            if (this.PART_Bottom != null)
            {
                this.PART_Bottom.MouseEnter += PART_Bottom_MouseEnter;
                this.PART_Bottom.DragStarted += Resize_DragStarted;
                this.PART_Bottom.DragDelta += PART_Bottom_DragDelta;
                this.PART_Bottom.DragCompleted += Resize_DragCompleted;
            }


            if (this.PART_Left != null)
            {
                this.PART_Left.MouseEnter += PART_Left_MouseEnter;
                this.PART_Left.DragStarted += Resize_DragStarted;
                this.PART_Left.DragDelta += PART_Left_DragDelta;
                this.PART_Left.DragCompleted += Resize_DragCompleted;
            }



            if (this.PART_Right != null)
            {
                this.PART_Right.MouseEnter += PART_Right_MouseEnter;
                this.PART_Right.DragStarted += Resize_DragStarted;
                this.PART_Right.DragDelta += PART_Right_DragDelta;
                this.PART_Right.DragCompleted += Resize_DragCompleted;
            }

            if (this.PART_TopLeft != null)
            {
                this.PART_TopLeft.MouseEnter += PART_TopLeft_MouseEnter;
                this.PART_TopLeft.DragStarted += Resize_DragStarted;
                this.PART_TopLeft.DragDelta += PART_TopLeft_DragDelta;
                this.PART_TopLeft.DragCompleted += Resize_DragCompleted;
            }

            if (this.PART_TopRight != null)
            {
                this.PART_TopRight.MouseEnter += PART_TopRight_MouseEnter;
                this.PART_TopRight.DragStarted += Resize_DragStarted;
                this.PART_TopRight.DragDelta += PART_TopRight_DragDelta;
                this.PART_TopRight.DragCompleted += Resize_DragCompleted;
            }

            if (this.PART_BottomLeft != null)
            {
                this.PART_BottomLeft.MouseEnter += PART_BottomLeft_MouseEnter;
                this.PART_BottomLeft.DragStarted += Resize_DragStarted;
                this.PART_BottomLeft.DragDelta += PART_BottomLeft_DragDelta;
                this.PART_BottomLeft.DragCompleted += Resize_DragCompleted;
            }

            if (this.PART_BottomRight != null)
            {
                this.PART_BottomRight.MouseEnter += PART_BottomRight_MouseEnter;
                this.PART_BottomRight.DragStarted += Resize_DragStarted;
                this.PART_BottomRight.DragDelta += PART_BottomRight_DragDelta;
                this.PART_BottomRight.DragCompleted += Resize_DragCompleted;
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
            Vector delta = GetLocalDelta(e);
            RotationRequested?.Invoke(this, this.RotationAngle);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.BottomRight, delta));
            ApplySelfResize(ResizeGripDirection.BottomRight, delta);
        }

        private void PART_BottomRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomRight.Cursor = this.GetResizeCursor(ResizeGripDirection.BottomRight);
        }

        private void PART_BottomLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.BottomLeft, delta));
            ApplySelfResize(ResizeGripDirection.BottomLeft, delta);
        }

        private void PART_BottomLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_BottomLeft.Cursor = this.GetResizeCursor(ResizeGripDirection.BottomLeft);
        }

        private void PART_TopRight_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.TopRight, delta));
            ApplySelfResize(ResizeGripDirection.TopRight, delta);
        }

        private void PART_TopRight_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopRight.Cursor = this.GetResizeCursor(ResizeGripDirection.TopRight);
        }

        private void PART_TopLeft_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.TopLeft, delta));
            ApplySelfResize(ResizeGripDirection.TopLeft, delta);
        }

        private void PART_TopLeft_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_TopLeft.Cursor = this.GetResizeCursor(ResizeGripDirection.TopLeft);
        }

        private void PART_Right_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Right, delta));
            ApplySelfResize(ResizeGripDirection.Right, delta);
        }

        private void PART_Right_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Right.Cursor = this.GetResizeCursor(ResizeGripDirection.Right);
        }

        private void PART_Left_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Left, delta));
            ApplySelfResize(ResizeGripDirection.Left, delta);
        }

        private void PART_Left_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Left.Cursor = this.GetResizeCursor(ResizeGripDirection.Left);
        }

        private void PART_Bottom_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeGripDirection.Bottom, delta));
            ApplySelfResize(ResizeGripDirection.Bottom, delta);
        }

        private void PART_Bottom_MouseEnter(object sender, MouseEventArgs e)
        {
            this.PART_Bottom.Cursor = this.GetResizeCursor(ResizeGripDirection.Bottom);
        }

        private void PART_Top_DragDelta(object sender, DragDeltaEventArgs e)
        {
            Vector delta = GetLocalDelta(e);
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
            // 对于平移，我们希望它是屏幕对齐的（鼠标往右滑，元素往右走）。
            // 由于 MoveThumb 是 PART_Root（已旋转环境）的子级，其 DragDelta 是局部坐标。
            // 我们需要将其转回父级（直立）空间。
            Vector screenDelta;
            if (this.PART_Root?.RenderTransform is RotateTransform rt)
            {
                Vector localDelta = new Vector(e.HorizontalChange, e.VerticalChange);
                screenDelta = rt.Value.Transform(localDelta);
            }
            else
            {
                screenDelta = new Vector(e.HorizontalChange, e.VerticalChange);
            }

            ProcessMove(screenDelta);
        }

        private void ProcessMove(Vector screenDelta)
        {
            TranslationRequested?.Invoke(this, screenDelta);

            // 如果处于自主模式且在 Canvas 中，则执行自我位移
            if (this.IsAutonomous && VisualTreeHelper.GetParent(this) is Canvas canvas)
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


        /// <summary>
        /// 获取 Rig 局部空间（旋转后空间）的位移。
        /// 由于 Thumbs 是 PART_Root（已旋转）的子级，WPF 的 DragDelta 已经提供了局部偏移。
        /// </summary>
        private Vector GetLocalDelta(DragDeltaEventArgs e)
        {
            return new Vector(e.HorizontalChange, e.VerticalChange);
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
            // 1. 确定在本次缩放期间应保持固定的局部锚点。
            // 这些点是相对于尺寸更改 *之前* 的 Rig 左上角 (0,0) 的。
            Point anchorLocal = new Point(0, 0);
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
            // 这里我们需要 Rig 在其父容器中的位置。
            var parentElement = VisualTreeHelper.GetParent(this) as FrameworkElement;
            if (parentElement == null)
            {
                return;
            }

            // 获取相对于父容器的左上角位置
            double left = 0, top = 0;
            if (parentElement is Canvas)
            {
                left = Canvas.GetLeft(this);
                top = Canvas.GetTop(this);
            }
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
            // 'delta' 已经在局部坐标空间中。
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

            if (this.IsAutonomous)
            {
                if (this.Width + dw > 20)
                {
                    this.Width += dw;
                }
                if (this.Height + dh > 20)
                {
                    this.Height += dh;
                }
            }

            // 4. 计算新的位置（仅当处于自主模式且父容器是 Canvas 时）
            if (this.IsAutonomous && parentElement is Canvas)
            {
                // 尺寸更改后的新局部锚点位置
                Point anchorLocalNew = new Point(0, 0);
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

                // 将此偏移量旋转当前角度，获得屏幕空间（直立空间）下的向量
                Matrix rotOnly = Matrix.Identity;
                rotOnly.Rotate(this.RotationAngle);
                Vector screenOffsetToCenter = rotOnly.Transform(new Vector(centerRelAnchorNew.X, centerRelAnchorNew.Y));

                // 计算新的位置
                Point centerNew = new Point(anchorScreen.X + screenOffsetToCenter.X, anchorScreen.Y + screenOffsetToCenter.Y);

                Canvas.SetLeft(this, centerNew.X - this.Width / 2);
                Canvas.SetTop(this, centerNew.Y - this.Height / 2);
            }
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


        private void Rotation_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            RotationCompleted?.Invoke(this, this.RotationAngle);
        }

        private void Resize_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var direction = GetDirection(thumb);
                ResizeStarted?.Invoke(this, direction);
            }
        }

        private void Resize_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (sender is Thumb thumb)
            {
                var direction = GetDirection(thumb);
                ResizeCompleted?.Invoke(this, direction);
            }
        }

        private ResizeGripDirection GetDirection(Thumb thumb)
        {
            if (thumb == PART_Top)
            {
                return ResizeGripDirection.Top;
            }
            if (thumb == PART_Bottom)
            {
                return ResizeGripDirection.Bottom;
            }
            if (thumb == PART_Left)
            {
                return ResizeGripDirection.Left;
            }
            if (thumb == PART_Right)
            {
                return ResizeGripDirection.Right;
            }
            if (thumb == PART_TopLeft)
            {
                return ResizeGripDirection.TopLeft;
            }
            if (thumb == PART_TopRight)
            {
                return ResizeGripDirection.TopRight;
            }
            if (thumb == PART_BottomLeft)
            {
                return ResizeGripDirection.BottomLeft;
            }
            if (thumb == PART_BottomRight)
            {
                return ResizeGripDirection.BottomRight;
            }
            return ResizeGripDirection.None;
        }
    }
}

