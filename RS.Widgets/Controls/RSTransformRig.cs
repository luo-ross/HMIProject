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

        #region RotateDirectionArrow
        private Thumb? PART_RectDirectionArrow;
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
            
            if (this.GetTemplateChild(nameof(this.PART_MoveThumb)) is Thumb moveThumb)
            {
                this.PART_MoveThumb = moveThumb;
                this.PART_MoveThumb.DragDelta += PART_MoveThumb_DragDelta;
            }

            // Initialize Resize Grips
            InitializeResizeGrip(nameof(PART_Top), ResizeGripDirection.Top);
            InitializeResizeGrip(nameof(PART_Bottom), ResizeGripDirection.Bottom);
            InitializeResizeGrip(nameof(PART_Left), ResizeGripDirection.Left);
            InitializeResizeGrip(nameof(PART_Right), ResizeGripDirection.Right);
            InitializeResizeGrip(nameof(PART_TopLeft), ResizeGripDirection.TopLeft);
            InitializeResizeGrip(nameof(PART_TopRight), ResizeGripDirection.TopRight);
            InitializeResizeGrip(nameof(PART_BottomLeft), ResizeGripDirection.BottomLeft);
            InitializeResizeGrip(nameof(PART_BottomRight), ResizeGripDirection.BottomRight);

            // Initialize Rotation Grips
            InitializeRotationGrip(nameof(PART_TopLeftRotate), ResizeGripDirection.TopLeft);
            InitializeRotationGrip(nameof(PART_TopRightRotate), ResizeGripDirection.TopRight);
            InitializeRotationGrip(nameof(PART_BottomLeftRotate), ResizeGripDirection.BottomLeft);
            InitializeRotationGrip(nameof(PART_BottomRightRotate), ResizeGripDirection.BottomRight);

            // Initialize Direction Buttons
            InitializeDirectionButton("PART_RectDirectionTop", RectDirection.Top);
            InitializeDirectionButton("PART_RectDirectionLeft", RectDirection.Left);
            InitializeDirectionButton("PART_RectDirectionRight", RectDirection.Right);
            InitializeDirectionButton("PART_RectDirectionBottom", RectDirection.Bottom);

            if (this.GetTemplateChild(nameof(this.PART_RectDirectionArrow)) is Thumb arrowThumb)
            {
                this.PART_RectDirectionArrow = arrowThumb;
                this.PART_RectDirectionArrow.MouseEnter += PART_RectDirectionArrow_MouseEnter;
                this.PART_RectDirectionArrow.DragStarted += Rotation_DragStarted;
                this.PART_RectDirectionArrow.DragDelta += Rotation_DragDelta;
                this.PART_RectDirectionArrow.DragCompleted += Rotation_DragCompleted;
            }
        }

        private void InitializeResizeGrip(string partName, ResizeGripDirection direction)
        {
            if (GetTemplateChild(partName) is Thumb thumb)
            {
                thumb.Tag = direction;
                thumb.MouseEnter += ResizeGrip_MouseEnter;
                thumb.DragStarted += Resize_DragStarted;
                thumb.DragDelta += ResizeGrip_DragDelta;
                thumb.DragCompleted += Resize_DragCompleted;
            }
        }

        private void InitializeRotationGrip(string partName, ResizeGripDirection direction)
        {
            if (GetTemplateChild(partName) is Thumb thumb)
            {
                thumb.Tag = direction;
                thumb.MouseEnter += RotationGrip_MouseEnter;
                thumb.DragStarted += Rotation_DragStarted;
                thumb.DragDelta += Rotation_DragDelta;
                thumb.DragCompleted += Rotation_DragCompleted;
            }
        }

        private void InitializeDirectionButton(string partName, RectDirection direction)
        {
            if (GetTemplateChild(partName) is Button button)
            {
                button.Tag = direction;
                button.Click += DirectionButton_Click;
            }
        }

        private void DirectionButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button button && button.Tag is RectDirection direction)
            {
                this.RectDirection = direction;
            }
        }
        }

        private void PART_RectDirectionArrow_MouseEnter(object sender, MouseEventArgs e)
        {
            if (this.PART_RectDirectionArrow == null) return;

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
            if (!this.IsAutonomous)
            {
                return;
            }

            // 确定在本次缩放期间应保持固定的局部锚点。
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

            // 捕获该锚点当前的屏幕位置。
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

            // 计算新的位置（仅当处于自主模式且父容器是 Canvas 时）
            if (parentElement is Canvas)
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

        private void Rotation_DragDelta(object sender, DragDeltaEventArgs e)
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
            if (thumb.Tag is ResizeGripDirection direction)
            {
                return direction;
            }
            return ResizeGripDirection.None;
        }
    }
}

