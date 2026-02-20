using RS.Widgets.Controls;
using RS.Widgets.CustomEventArgs;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Services;
using RS.Widgets.Structs;
using RS.Widgets.Utilities;
using RS.Widgets.Visuals;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using System.Windows.Shell;


namespace RS.Widgets.Adorners
{
    public class TransformAdorner : Adorner, ISelectable
    {
        private FrameworkElement AdornedFE => AdornedElement as FrameworkElement;

        // 全局选中服务（所有 TransformAdorner 实例共享）
        private static readonly RSSelectService<TransformAdorner> SelectionService = new RSSelectService<TransformAdorner>();


        private double GlobalScaleX = 1;
        private double GlobalScaleY = 1;
        private Size VisualPixelSize = new Size(0, 0);

        // 用于渲染的可复用 DrawingVisual
        private readonly RSTransformVisual TransformVisual;
        private readonly VisualCollection Visuals;

        // 布局边距 — 扩大 Adorner 的布局边界，
        // 以便在 AdornedElement (负坐标) 外部绘制的区域
        // 仍可接收来自 AdornerLayer 的鼠标命中测试事件。
        private const double HitPadding = 10.0;

        // 缓存的光标数据（加载一次，在实例间共享）
        private static readonly CursorData BaseRotationCursorData;
        private static readonly CursorData BaseResizeCursorData;

        // ── 鼠标 / 拖拽状态 ──
        private bool IsMouseCaptured;         // 鼠标已被捕获（MouseDown 后）
        private bool IsDragging;              // 已超过最小拖拽阈值，正式开始拖拽
        private TransformOperation PendingOperation;  // MouseDown 时 hit-test 结果（拖拽操作候选）
        private TransformOperation CurrentOperation;  // 实际拖拽中的操作
        private Point MouseDownPosition;      // MouseDown 时在父级坐标系中的位置
        private Point LastMouseScreen;        // 上一帧鼠标在父级坐标系中的位置

        // ── 方向按钮悬停状态 ──
        private RectDirection? HoveredDirectionButton;

        // ── 旋转状态 ──
        private double InitialRotationOffset;

        // ── 缩放状态 (一次性捕获策略，与老版本一致) ──
        private bool IsResizing;
        private Point ResizeAnchorInParent;
        private double ResizeInitialWidth;
        private double ResizeInitialHeight;
        private Matrix ResizeInitialTransformMatrix;
        private Vector ResizeAccDelta;
        private ResizeGripDirection ResizeDirection;


        // ── Events (与 RSTransformRig 的签名一致) ──
        public event EventHandler<double>? RotationRequested;
        public event EventHandler<double>? RotationCompleted;
        public event EventHandler<Vector>? TranslationRequested;
        public event EventHandler<ResizeGripDirection>? ResizeStarted;
        public event EventHandler<ResizeEventArgs>? ResizeRequested;
        public event EventHandler<ResizeGripDirection>? ResizeCompleted;


        static TransformAdorner()
        {
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




        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(TransformAdorner),
                new PropertyMetadata(null, OnVisualPropertyChanged));



        public TransformAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            var brush = new SolidColorBrush(ColorHelper.GetNextVibrantColor());
            brush.Freeze();
            this.SetCurrentValue(BorderBrushProperty, brush);

            TransformVisual = new RSTransformVisual();
            Visuals = new VisualCollection(this) { TransformVisual };

            this.Focusable = true;
            this.Loaded += TransformAdorner_Loaded;
        }

        private void TransformAdorner_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.IsSelect)
            {
                BringToFront();
            }

            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.MouseLeftButtonUp -= Window_MouseLeftButtonUp;
                window.MouseLeftButtonUp += Window_MouseLeftButtonUp;
            }
        }

        private static void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectionService.SelectedItems.Count > 0 && sender is Window window)
            {
                Point pt = e.GetPosition(window);
                if (VisualHelper.TryFindFromPoint<TransformAdorner>(window, pt) == null)
                {
                    UnselectAll();
                }
            }
        }



        #region Properties

        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register(nameof(RotationAngle), typeof(double), typeof(TransformAdorner),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));

        public RectDirection RectDirection
        {
            get { return (RectDirection)GetValue(RectDirectionProperty); }
            set { SetValue(RectDirectionProperty, value); }
        }

        public static readonly DependencyProperty RectDirectionProperty =
            DependencyProperty.Register(nameof(RectDirection), typeof(RectDirection), typeof(TransformAdorner),
                new FrameworkPropertyMetadata(RectDirection.Top, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsSelect
        {
            get { return (bool)GetValue(IsSelectProperty); }
            set { SetValue(IsSelectProperty, value); }
        }

        public static readonly DependencyProperty IsSelectProperty =
            DependencyProperty.Register(nameof(IsSelect), typeof(bool), typeof(TransformAdorner),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsSingleSelect
        {
            get { return (bool)GetValue(IsSingleSelectProperty); }
            set { SetValue(IsSingleSelectProperty, value); }
        }

        public static readonly DependencyProperty IsSingleSelectProperty =
            DependencyProperty.Register(nameof(IsSingleSelect), typeof(bool), typeof(TransformAdorner),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsDirectionEnabled
        {
            get { return (bool)GetValue(IsDirectionEnabledProperty); }
            set { SetValue(IsDirectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsDirectionEnabledProperty =
            DependencyProperty.Register(nameof(IsDirectionEnabled), typeof(bool), typeof(TransformAdorner),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsRotationEnabled
        {
            get { return (bool)GetValue(IsRotationEnabledProperty); }
            set { SetValue(IsRotationEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsRotationEnabledProperty =
            DependencyProperty.Register(nameof(IsRotationEnabled), typeof(bool), typeof(TransformAdorner),
                new FrameworkPropertyMetadata(true, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TransformAdorner adorner)
            {
                adorner.UpdateVisual();
            }
        }

        /// <summary>
        /// 无参数调用默认选中自身（如代码调用或键盘导航）
        /// </summary>
        public void Select()
        {
            Select(null);
        }

        /// <summary>
        /// 取消选中所有项
        /// </summary>
        public static void UnselectAll()
        {
            SelectionService.ClearSelect();
        }

        /// <summary>
        /// 处理选中逻辑：包含堆叠时的循环选择逻辑。无 Ctrl → 单选，Ctrl → 多选。
        /// 返回最终确定的交互目标，以便将由于 AdornerLayer 层级错乱而错误拦截的鼠标事件正确转发。
        /// </summary>
        private TransformAdorner Select(MouseButtonEventArgs e)
        {
            var isMulti = Keyboard.Modifiers.HasFlag(ModifierKeys.Control);
            var window = Window.GetWindow(this);
            var hitList = e != null ? VisualHelper.FindAllFromPoint<TransformAdorner>(window, e.GetPosition(window)) : new List<TransformAdorner>();

            // 保底确保至少包含当前操作项
            if (hitList.Count == 0)
            {
                hitList.Add(this);
            }

            if (isMulti)
            {
                // 多选堆叠模式：
                // 按 ZIndex 和 VisualTree 顺序排序 (Back -> Front)
                hitList.Sort((a, b) => 
                {
                    UIElement elA = a.AdornedElement as UIElement;
                    UIElement elB = b.AdornedElement as UIElement;
                    if (elA == null || elB == null)
                    {
                        return 0;
                    }
                    
                    Panel pA = VisualTreeHelper.GetParent(elA) as Panel;
                    Panel pB = VisualTreeHelper.GetParent(elB) as Panel;
                    if (pA != pB || pA == null)
                    {
                        return 0; 
                    }
                
                    int zA = Panel.GetZIndex(elA);
                    int zB = Panel.GetZIndex(elB);
                    if (zA != zB)
                    {
                        return zA.CompareTo(zB);
                    }
                
                    int idxA = pA.Children.IndexOf(elA);
                    int idxB = pB.Children.IndexOf(elB);
                    return idxA.CompareTo(idxB);
                });

                // 反转为 Front -> Back 顺序，这样找 FirstOrDefault(未选中) 时就是在从最上面往下找
                hitList.Reverse();

                if (hitList.Count > 1 && hitList.All(r => r.IsSelect))
                {
                    // 如果堆叠项全部已选，则执行“一键全反选”
                    hitList.ForEach(r => SelectionService.MultiSelect(r));
                    return this; // 多选全部取消，焦点留在此处
                }
                else
                {
                    // 按从上到下的层级，贪婪选中第一个未选中的项
                    var target = hitList.FirstOrDefault(r => !r.IsSelect) ?? this;
                    SelectionService.MultiSelect(target);
                    // 多选时不自动置顶，保持原层级
                    return target;
                }
            }
            else
            {
                // 单选模式
                // 按 ZIndex 和 VisualTree 顺序排序 (Back -> Front)
                hitList.Sort((a, b) => 
                {
                    UIElement elA = a.AdornedElement as UIElement;
                    UIElement elB = b.AdornedElement as UIElement;
                    if (elA == null || elB == null)
                    {
                        return 0;
                    }
                    
                    Panel pA = VisualTreeHelper.GetParent(elA) as Panel;
                    Panel pB = VisualTreeHelper.GetParent(elB) as Panel;
                    if (pA != pB || pA == null)
                    {
                        return 0; 
                    }
                
                    int zA = Panel.GetZIndex(elA);
                    int zB = Panel.GetZIndex(elB);
                    if (zA != zB)
                    {
                        return zA.CompareTo(zB);
                    }
                
                    int idxA = pA.Children.IndexOf(elA);
                    int idxB = pB.Children.IndexOf(elB);
                    return idxA.CompareTo(idxB);
                });

                var current = hitList.FirstOrDefault(r => r.IsSelect);
                
                // 找到当前选中项后，切换到其下一层；若无选中项，则选中最上层 (末尾)
                var nextIndex = (current != null && hitList.Count > 1) 
                    ? (hitList.IndexOf(current) + 1) % hitList.Count 
                    : hitList.Count - 1;

                var target = hitList[nextIndex];
                SelectionService.SingleSelect(target);
                target.BringToFront();
                return target;
            }
        }

        private void BringToFront()
        {
            // 仅提升 Adorner 本身在 AdornerLayer 中的视觉层级 (通过重新添加移到末尾)
            // 选择操作不应该去修改被装饰元素（原数据）的 ZIndex 顺序
            var adornerLayer = VisualTreeHelper.GetParent(this) as AdornerLayer;
            if (adornerLayer != null)
            {
                int count = VisualTreeHelper.GetChildrenCount(adornerLayer);
                if (count > 0 && VisualTreeHelper.GetChild(adornerLayer, count - 1) != this)
                {
                    adornerLayer.Remove(this);
                    adornerLayer.Add(this);
                }
            }
        }

        #endregion

        #region Rendering (Delegated to RSTransformVisual)

        /// <summary>
        /// 重绘具有当前状态的 TransformVisual。
        /// </summary>
        private void UpdateVisual()
        {
            if (TransformVisual == null)
            {
                return;
            }
            TransformVisual.Render(VisualPixelSize, BorderBrush, IsSelect, IsSingleSelect, RectDirection, IsDirectionEnabled, HoveredDirectionButton);
        }

        protected override int VisualChildrenCount
        {
            get { return Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visuals[index];
        }

        #endregion


        #region Hit Testing & Cursor

        /// <summary>
        /// 将命中测试委托给 TransformVisual。
        /// </summary>
        private TransformOperation HitTest(Point p)
        {
            return TransformVisual.HitTest(p, VisualPixelSize, RectDirection, IsDirectionEnabled, IsRotationEnabled);
        }

        /// <summary>
        /// 将 Adorner 坐标系的点补偿 HitPadding 后转为 Visual 本地坐标。
        /// </summary>
        private Point ToLocalPoint(Point adornerPoint)
        {
            return new Point(adornerPoint.X - HitPadding, adornerPoint.Y - HitPadding);
        }

        /// <summary>
        /// 重写 HitTestCore 以允许命中测试超出 Adorner 布局边界的地方（如旋转把手）。
        /// </summary>
        protected override HitTestResult HitTestCore(PointHitTestParameters hitTestParameters)
        {
            Point pt = ToLocalPoint(hitTestParameters.HitPoint);
            var op = HitTest(pt);
            if (op != TransformOperation.None)
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }
            return null;
        }

        /// <summary>
        /// Dynamically updates cursor based on which transform zone the mouse is hovering over.
        /// During a drag operation, the cursor stays locked to the current operation.
        /// </summary>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            // ── 已捕获但尚未开始拖拽：检查最小拖拽阈值 ──
            if (IsMouseCaptured && !IsDragging)
            {
                Point current = GetScreenPosition(e);
                Vector diff = current - MouseDownPosition;

                if (Math.Abs(diff.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    // 超过阈值 → 正式开始拖拽
                    CurrentOperation = PendingOperation;
                    IsDragging = true;
                    LastMouseScreen = current;

                    // 根据操作类型初始化
                    if (IsResizeOperation(CurrentOperation))
                    {
                        BeginResize(CurrentOperation);
                    }
                    else if (IsRotationOperation(CurrentOperation))
                    {
                        BeginRotation(e);
                    }
                }
                else
                {
                    // 如果在捕获阶段但尚未超过最小拖拽距离，锁定光标为即将进行的操作
                    this.Cursor = GetCursorForOperation(PendingOperation);
                }
                e.Handled = true;
                return;
            }

            // ── 拖拽进行中：计算增量并分发 ──
            if (IsDragging)
            {
                Point currentScreen = GetScreenPosition(e);
                Vector screenDelta = currentScreen - LastMouseScreen;
                LastMouseScreen = currentScreen;

                switch (CurrentOperation)
                {
                    case TransformOperation.Move:
                        PerformMoveDelta(screenDelta);
                        break;

                    case TransformOperation.ResizeTop:
                    case TransformOperation.ResizeBottom:
                    case TransformOperation.ResizeLeft:
                    case TransformOperation.ResizeRight:
                    case TransformOperation.ResizeTopLeft:
                    case TransformOperation.ResizeTopRight:
                    case TransformOperation.ResizeBottomLeft:
                    case TransformOperation.ResizeBottomRight:
                        PerformResizeDelta(screenDelta);
                        break;

                    case TransformOperation.Rotate:
                    case TransformOperation.RotateTopLeft:
                    case TransformOperation.RotateTopRight:
                    case TransformOperation.RotateBottomLeft:
                    case TransformOperation.RotateBottomRight:
                        PerformRotationDelta(e);
                        break;
                }
                e.Handled = true;
                return;
            }

            // ── 空闲悬停：更新光标 + 方向按钮 hover ──
            Point pt = ToLocalPoint(e.GetPosition(this));

            // 检测方向按钮悬停（方向按钮在 body 外面，必须独立检测）
            if (IsSingleSelect && IsDirectionEnabled)
            {
                var hitDir = TransformVisual.HitTestDirectionButton(pt, VisualPixelSize, RectDirection);
                RectDirection? newHover = (hitDir != RectDirection) ? hitDir : (RectDirection?)null;

                if (newHover != HoveredDirectionButton)
                {
                    HoveredDirectionButton = newHover;
                    UpdateVisual();
                }

                if (HoveredDirectionButton != null)
                {
                    this.Cursor = Cursors.Hand;
                    return;
                }
            }
            else if (HoveredDirectionButton != null)
            {
                HoveredDirectionButton = null;
                UpdateVisual();
            }

            var op = HitTest(pt);
            this.Cursor = GetCursorForOperation(op);
        }

        /// <summary>
        /// 当鼠标离开 Adorner 时重置光标。
        /// </summary>
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsDragging)
            {
                this.Cursor = Cursors.Arrow;
            }

            // 清除方向按钮 hover
            if (HoveredDirectionButton != null)
            {
                HoveredDirectionButton = null;
                UpdateVisual();
            }
        }

        /// <summary>
        /// 将 TransformOperation 映射到相应的光标。
        /// </summary>
        private Cursor GetCursorForOperation(TransformOperation op)
        {
            if (!IsSelect)
            {
                return Cursors.Arrow;
            }

            switch (op)
            {
                case TransformOperation.Move:
                    return Cursors.SizeAll;

                // ── Resize cursors (rotated by element angle) ──
                case TransformOperation.ResizeTop:
                case TransformOperation.ResizeBottom:
                    return GetResizeCursor(RotationAngle);

                case TransformOperation.ResizeLeft:
                case TransformOperation.ResizeRight:
                    return GetResizeCursor(RotationAngle + 90);

                case TransformOperation.ResizeTopLeft:
                case TransformOperation.ResizeBottomRight:
                    return GetResizeCursor(RotationAngle - 45);

                case TransformOperation.ResizeTopRight:
                case TransformOperation.ResizeBottomLeft:
                    return GetResizeCursor(RotationAngle + 45);

                // ── Rotation cursors (rotated by element angle + corner offset) ──
                case TransformOperation.Rotate:
                    switch (RectDirection)
                    {
                        case RectDirection.Top: 
                            return GetRotationCursor(RotationAngle - 45);
                        case RectDirection.Right: 
                            return GetRotationCursor(RotationAngle + 45);
                        case RectDirection.Bottom: 
                            return GetRotationCursor(RotationAngle + 135);
                        case RectDirection.Left: 
                            return GetRotationCursor(RotationAngle + 225);
                        default: 
                            return GetRotationCursor(RotationAngle - 45);
                    }

                case TransformOperation.RotateTopLeft:
                    return GetRotationCursor(RotationAngle - 90);

                case TransformOperation.RotateTopRight:
                    return GetRotationCursor(RotationAngle);

                case TransformOperation.RotateBottomRight:
                    return GetRotationCursor(RotationAngle + 90);

                case TransformOperation.RotateBottomLeft:
                    return GetRotationCursor(RotationAngle + 180);

                default:
                    return Cursors.Arrow;
            }
        }

        /// <summary>
        /// 创建一个动态旋转的缩放光标。
        /// </summary>
        private Cursor GetResizeCursor(double angle)
        {
            if (BaseResizeCursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            var cursorData = CursorHelper.RotateCursor(BaseResizeCursorData, angle);
            if (cursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            return CursorHelper.CreateCursor(cursorData.Bitmap, cursorData.HotspotX, cursorData.HotspotY);
        }

        /// <summary>
        /// 创建一个动态旋转的旋转光标。
        /// </summary>
        private Cursor GetRotationCursor(double angle)
        {
            if (BaseRotationCursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            var cursorData = CursorHelper.RotateCursor(BaseRotationCursorData, angle);
            if (cursorData.Bitmap == null)
            {
                return Cursors.Arrow;
            }

            return CursorHelper.CreateCursor(cursorData.Bitmap, cursorData.HotspotX, cursorData.HotspotY);
        }

        #endregion



        #region Layout & measure (Simplified)


        protected override Size MeasureOverride(Size constraint)
        {
            // 扩大布局边界，使命中测试能覆盖到边距区域
            var inflated = new Size(
                VisualPixelSize.Width + 2 * HitPadding,
                VisualPixelSize.Height + 2 * HitPadding);
            return base.MeasureOverride(inflated);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            var inflated = new Size(
                VisualPixelSize.Width + 2 * HitPadding,
                VisualPixelSize.Height + 2 * HitPadding);
            return base.ArrangeOverride(inflated);
        }

        /// <summary>
        /// 裁剪 adorner 使其不超出 ClipToBounds=true 的父容器边界。
        /// </summary>
        protected override Geometry GetLayoutClip(Size layoutSlotSize)
        {
            // 沿着被装饰元素的视觉树向上查找最近的 ClipToBounds 容器
            var clipParent = FindClippingAncestor(AdornedElement);
            if (clipParent == null)
            {
                return null; // 无裁剪
            }

            try
            {
                // 将裁剪容器的边界矩形转换到 Adorner 本地坐标系
                GeneralTransform toAdorner = clipParent.TransformToVisual(this);
                Rect clipRect = new Rect(clipParent.RenderSize);
                Rect localClip = toAdorner.TransformBounds(clipRect);
                return new RectangleGeometry(localClip);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// 沿视觉树向上查找第一个 ClipToBounds=true 的 FrameworkElement。
        /// </summary>
        private static FrameworkElement FindClippingAncestor(DependencyObject element)
        {
            DependencyObject current = VisualTreeHelper.GetParent(element);
            while (current != null)
            {
                if (current is FrameworkElement fe && fe.ClipToBounds)
                {
                    return fe;
                }
                current = VisualTreeHelper.GetParent(current);
            }
            return null;
        }

        #endregion

        #region Layout Infrastructure

        private Matrix AdornerMatrix = Matrix.Identity;
        public override GeneralTransform GetDesiredTransform(GeneralTransform transform)
        {
            var t = transform as MatrixTransform;
            if (t == null)
            {
                return transform;
            }
            Matrix matrix = t.Value;
            AdornerMatrix = GetMatrixWithoutScale(matrix, out double gsx, out double gsy);

            // 缩放期间锁定 GlobalScale 防止抖动
            if (!IsResizing)
            {
                GlobalScaleX = gsx;
                GlobalScaleY = gsy;
            }

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
            VisualPixelSize = new Size(w * GlobalScaleX, h * GlobalScaleY);

            // 在 Adorner 的局部空间中，将原点平移 -HitPadding。
            // 矩阵包含旋转因素 (M11/M12/M21/M22)，因此必须在应用偏移之前，将边距向量通过旋转进行变换。
            double paddingDx = AdornerMatrix.M11 * (-HitPadding) + AdornerMatrix.M21 * (-HitPadding);
            double paddingDy = AdornerMatrix.M12 * (-HitPadding) + AdornerMatrix.M22 * (-HitPadding);
            AdornerMatrix.OffsetX += paddingDx;
            AdornerMatrix.OffsetY += paddingDy;

            // 将 DrawingVisual 定位在放大后的 Adorner 的内部 (HitPadding, HitPadding) 处，
            // 以便它的 (0,0) 位置仍然与元素对齐。
            TransformVisual.Offset = new Vector(HitPadding, HitPadding);

            // 每当布局变化时更新 Visual
            UpdateVisual();

            return new MatrixTransform(AdornerMatrix);
        }

        public Matrix GetMatrixWithoutScale(Matrix originalMatrix, out double scaleX, out double scaleY)
        {
            scaleX = Math.Sqrt(originalMatrix.M11 * originalMatrix.M11 + originalMatrix.M12 * originalMatrix.M12);
            scaleY = Math.Sqrt(originalMatrix.M21 * originalMatrix.M21 + originalMatrix.M22 * originalMatrix.M22);

            scaleX = scaleX < 1e-6 ? 1 : scaleX;
            scaleY = scaleY < 1e-6 ? 1 : scaleY;

            double m11 = originalMatrix.M11 / scaleX;
            double m12 = originalMatrix.M12 / scaleX;
            double m21 = originalMatrix.M21 / scaleY;
            double m22 = originalMatrix.M22 / scaleY;

            double offsetX = originalMatrix.OffsetX;
            double offsetY = originalMatrix.OffsetY;

            return new Matrix(m11, m12, m21, m22, offsetX, offsetY);
        }


        #endregion

        #region Mouse Interaction (replaces Thumb DragStarted / DragDelta / DragCompleted)

        private bool WasAnySelectedInStack = false;

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var window = Window.GetWindow(this);
            var hitList = VisualHelper.FindAllFromPoint<TransformAdorner>(window, e.GetPosition(window));
            WasAnySelectedInStack = hitList.Any(r => r.IsSelect);

            TransformAdorner target = this;

            // 如果没有任何元素被选中，那么我们毫不犹豫地在 MouseDown 阶段进行选中
            if (!WasAnySelectedInStack)
            {
                target = Select(e);
            }
            else
            {
                // 如果已经有选中的元素了，那我们暂不循环切换，可能是想拖拽！
                // 这时我们要返回当前选中的那个，如果有多个就返回视觉树上最靠前的那个
                
                // 按 ZIndex 和 VisualTree 顺序排序 (Back -> Front)
                hitList.Sort((a, b) => 
                {
                    UIElement elA = a.AdornedElement as UIElement;
                    UIElement elB = b.AdornedElement as UIElement;
                    if (elA == null || elB == null) 
                    {
                        return 0;
                    }
                    Panel pA = VisualTreeHelper.GetParent(elA) as Panel;
                    Panel pB = VisualTreeHelper.GetParent(elB) as Panel;
                    if (pA != pB || pA == null) 
                    {
                        return 0; 
                    }
                    int zA = Panel.GetZIndex(elA);
                    int zB = Panel.GetZIndex(elB);
                    if (zA != zB) 
                    {
                        return zA.CompareTo(zB);
                    }
                    int idxA = pA.Children.IndexOf(elA);
                    int idxB = pB.Children.IndexOf(elB);
                    return idxA.CompareTo(idxB);
                });
                
                target = hitList.LastOrDefault(r => r.IsSelect) ?? this;
            }

            if (target != null && target != this)
            {
                // 如果决定交互的是底下一个元素（因为 AdornerLayer 会遮挡且层级可能错误拦截），
                // 我们直接将拖拽意图路由给目标，自身放弃处理
                target.ProcessMouseDown(e);
                e.Handled = true;
                return;
            }

            ProcessMouseDown(e);
        }

        internal void ProcessMouseDown(MouseButtonEventArgs e)
        {
            Point pt = ToLocalPoint(e.GetPosition(this));
            var operation = HitTest(pt);

            // 方向按钮点击 → 优先判断，即使 operation == None（按钮可能在 body 和箭头判定区外）也应响应
            if (IsSingleSelect && HandleDirectionButtonClick(pt))
            {
                e.Handled = true;
                return;
            }

            if (operation == TransformOperation.None)
            {
                return;
            }

            // 记录位置和候选操作，捕获鼠标，等待 MouseMove 判断是否为拖拽
            PendingOperation = operation;
            MouseDownPosition = GetScreenPosition(e);
            LastMouseScreen = MouseDownPosition;
            IsMouseCaptured = true;
            IsDragging = false;

            this.Cursor = GetCursorForOperation(PendingOperation);
            this.CaptureMouse();
            this.Focus();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsDragging)
            {
                // ── 拖拽结束 ──
                if (IsResizeOperation(CurrentOperation))
                {
                    EndResize();
                }
                else if (IsRotationOperation(CurrentOperation))
                {
                    EndRotation();
                }

                IsDragging = false;
                CurrentOperation = TransformOperation.None;
            }
            else if (IsMouseCaptured)
            {
                // ── 没有发生拖拽（纯点击） → 根据预存的状态决定是否执行选中（循环切或多选Toggle） ──
                if (WasAnySelectedInStack)
                {
                    Select(e);
                }
            }

            if (IsMouseCaptured)
            {
                IsMouseCaptured = false;
                PendingOperation = TransformOperation.None;
                this.ReleaseMouseCapture();
            }
            e.Handled = true;
        }

        /// <summary>
        /// 获取鼠标在屏幕坐标系中的位置（设备像素），
        /// 与 Thumb.DragDelta 的坐标基准一致。
        /// </summary>
        private Point GetScreenPosition(MouseEventArgs e)
        {
            return this.PointToScreen(e.GetPosition(this));
        }

        #endregion

        #region Move

        private void PerformMoveDelta(Vector screenDelta)
        {
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyMoveDelta(screenDelta);
            }
        }

        internal void ApplyMoveDelta(Vector screenDelta)
        {
            if (AdornedFE == null)
            {
                return;
            }

            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 与 TransformAdorner.ApplyTranslation 一致：
            // 计算父容器到屏幕的完整缩放，除以它使移动速度匹配鼠标
            PresentationSource source = PresentationSource.FromVisual(parent);
            Matrix matrixScreen = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;

            GeneralTransform transformToRoot = parent.TransformToAncestor(source?.RootVisual ?? parent);
            if (transformToRoot is Transform t)
            {
                matrixScreen.Append(t.Value);
            }

            double scaleX = Math.Sqrt(matrixScreen.M11 * matrixScreen.M11 + matrixScreen.M12 * matrixScreen.M12);
            double scaleY = Math.Sqrt(matrixScreen.M21 * matrixScreen.M21 + matrixScreen.M22 * matrixScreen.M22);

            double dx = screenDelta.X / (scaleX > 0 ? scaleX : 1.0);
            double dy = screenDelta.Y / (scaleY > 0 ? scaleY : 1.0);

            if (parent is Canvas)
            {
                double left = Canvas.GetLeft(AdornedElement);
                if (double.IsNaN(left))
                {
                    left = AdornedElement.TranslatePoint(new Point(0, 0), (UIElement)parent).X;
                }

                double top = Canvas.GetTop(AdornedElement);
                if (double.IsNaN(top))
                {
                    top = AdornedElement.TranslatePoint(new Point(0, 0), (UIElement)parent).Y;
                }

                TransformHelper.SetCanvasX(AdornedElement, left + dx);
                TransformHelper.SetCanvasY(AdornedElement, top + dy);
            }
            else
            {
                var x = TransformHelper.GetTransformX(AdornedElement);
                var y = TransformHelper.GetTransformY(AdornedElement);
                TransformHelper.SetTransformX(AdornedElement, x + dx);
                TransformHelper.SetTransformY(AdornedElement, y + dy);
            }

            TranslationRequested?.Invoke(this, screenDelta);
        }

        #endregion

        #region Resize

        private static bool IsResizeOperation(TransformOperation op)
        {
            return op >= TransformOperation.ResizeTopLeft && op <= TransformOperation.ResizeLeft;
        }

        /// <summary>
        /// 将 TransformOperation 映射为 ResizeGripDirection。
        /// </summary>
        private static ResizeGripDirection ToResizeDirection(TransformOperation op)
        {
            switch (op)
            {
                case TransformOperation.ResizeTop: 
                    return ResizeGripDirection.Top;
                case TransformOperation.ResizeBottom: 
                    return ResizeGripDirection.Bottom;
                case TransformOperation.ResizeLeft: 
                    return ResizeGripDirection.Left;
                case TransformOperation.ResizeRight: 
                    return ResizeGripDirection.Right;
                case TransformOperation.ResizeTopLeft: 
                    return ResizeGripDirection.TopLeft;
                case TransformOperation.ResizeTopRight: 
                    return ResizeGripDirection.TopRight;
                case TransformOperation.ResizeBottomLeft: 
                    return ResizeGripDirection.BottomLeft;
                case TransformOperation.ResizeBottomRight: 
                    return ResizeGripDirection.BottomRight;
                default: 
                    return ResizeGripDirection.None;
            }
        }

        /// <summary>
        /// Capture Once — 在拖拽开始时快照锚点和初始尺寸 (同 TransformAdorner.TransformRig_ResizeStarted)。
        /// </summary>
        private void BeginResize(TransformOperation op)
        {
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyBeginResize(op);
            }
        }

        internal void ApplyBeginResize(TransformOperation op)
        {
            if (AdornedFE == null)
            {
                return;
            }

            IsResizing = true;
            ResizeAccDelta = new Vector(0, 0);
            ResizeDirection = ToResizeDirection(op);

            // 确定锚点
            Point anchorLocal;
            switch (ResizeDirection)
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

            var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
            if (parent == null)
            {
                return;
            }

            GeneralTransform transformToParent = AdornedFE.TransformToVisual(parent);
            ResizeAnchorInParent = transformToParent.Transform(anchorLocal);
            ResizeInitialWidth = double.IsNaN(AdornedFE.Width) ? AdornedFE.ActualWidth : AdornedFE.Width;
            ResizeInitialHeight = double.IsNaN(AdornedFE.Height) ? AdornedFE.ActualHeight : AdornedFE.Height;

            Matrix mat = (transformToParent as Transform)?.Value ?? Matrix.Identity;
            mat.OffsetX = 0;
            mat.OffsetY = 0;
            ResizeInitialTransformMatrix = mat;

            ResizeStarted?.Invoke(this, ResizeDirection);
        }

        /// <summary>
        /// 处理每帧的缩放增量 (同 TransformAdorner.TransformRig_ResizeRequested)。
        /// screenDelta 是父容器坐标系的位移。
        /// </summary>
        private void PerformResizeDelta(Vector screenDelta)
        {
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyResizeDelta(screenDelta);
            }
        }

        internal void ApplyResizeDelta(Vector screenDelta)
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

            // screenDelta 是屏幕设备像素。GlobalScaleX/Y 已经包含父容器的缩放，
            // 所以只需要除以 DPI 转换为 WPF DIPs，GlobalScale 再负责 DIPs → 逻辑坐标。
            PresentationSource source = PresentationSource.FromVisual(parent);
            Matrix dpiMatrix = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;
            double dpiScaleX = Math.Sqrt(dpiMatrix.M11 * dpiMatrix.M11 + dpiMatrix.M12 * dpiMatrix.M12);
            double dpiScaleY = Math.Sqrt(dpiMatrix.M21 * dpiMatrix.M21 + dpiMatrix.M22 * dpiMatrix.M22);

            Vector dipsDelta = new Vector(
                screenDelta.X / (dpiScaleX > 0 ? dpiScaleX : 1.0),
                screenDelta.Y / (dpiScaleY > 0 ? dpiScaleY : 1.0));

            // 反旋转到元素本地空间
            Matrix invRot = Matrix.Identity;
            invRot.Rotate(-RotationAngle);
            Vector localDelta = invRot.Transform(dipsDelta);

            // 累加
            ResizeAccDelta += localDelta;

            // 根据方向计算 dw / dh
            double dw = 0, dh = 0;
            switch (ResizeDirection)
            {
                case ResizeGripDirection.TopLeft:
                    dw = -ResizeAccDelta.X;
                    dh = -ResizeAccDelta.Y;
                    break;
                case ResizeGripDirection.Top:
                    dh = -ResizeAccDelta.Y;
                    break;
                case ResizeGripDirection.TopRight:
                    dw = ResizeAccDelta.X;
                    dh = -ResizeAccDelta.Y;
                    break;
                case ResizeGripDirection.Left:
                    dw = -ResizeAccDelta.X;
                    break;
                case ResizeGripDirection.Right:
                    dw = ResizeAccDelta.X;
                    break;
                case ResizeGripDirection.BottomLeft:
                    dw = -ResizeAccDelta.X;
                    dh = ResizeAccDelta.Y;
                    break;
                case ResizeGripDirection.Bottom:
                    dh = ResizeAccDelta.Y;
                    break;
                case ResizeGripDirection.BottomRight:
                    dw = ResizeAccDelta.X;
                    dh = ResizeAccDelta.Y;
                    break;
            }

            // Delta 是像素坐标，转换为逻辑坐标
            double dw_logical = dw / GlobalScaleX;
            double dh_logical = dh / GlobalScaleY;

            double newW = ResizeInitialWidth + dw_logical;
            double newH = ResizeInitialHeight + dh_logical;

            // 限制尺寸
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

            // 修正 AccDelta 以匹配受限后的尺寸
            double clamped_dw = (newW - ResizeInitialWidth) * GlobalScaleX;
            double clamped_dh = (newH - ResizeInitialHeight) * GlobalScaleY;

            switch (ResizeDirection)
            {
                case ResizeGripDirection.TopLeft:
                    ResizeAccDelta.X = -clamped_dw;
                    ResizeAccDelta.Y = -clamped_dh;
                    break;
                case ResizeGripDirection.Top:
                    ResizeAccDelta.Y = -clamped_dh;
                    break;
                case ResizeGripDirection.TopRight:
                    ResizeAccDelta.X = clamped_dw;
                    ResizeAccDelta.Y = -clamped_dh;
                    break;
                case ResizeGripDirection.Left:
                    ResizeAccDelta.X = -clamped_dw;
                    break;
                case ResizeGripDirection.Right:
                    ResizeAccDelta.X = clamped_dw;
                    break;
                case ResizeGripDirection.BottomLeft:
                    ResizeAccDelta.X = -clamped_dw;
                    ResizeAccDelta.Y = clamped_dh;
                    break;
                case ResizeGripDirection.Bottom:
                    ResizeAccDelta.Y = clamped_dh;
                    break;
                case ResizeGripDirection.BottomRight:
                    ResizeAccDelta.X = clamped_dw;
                    ResizeAccDelta.Y = clamped_dh;
                    break;
            }

            // 基于初始矩阵推导新中心点，确保锚点不动
            Point anchorLocalNew;
            switch (ResizeDirection)
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

            Point centerRelAnchorNew = new Point(newW / 2 - anchorLocalNew.X, newH / 2 - anchorLocalNew.Y);
            Vector offsetToCenter = ResizeInitialTransformMatrix.Transform(new Vector(centerRelAnchorNew.X, centerRelAnchorNew.Y));
            Point centerNew = new Point(ResizeAnchorInParent.X + offsetToCenter.X, ResizeAnchorInParent.Y + offsetToCenter.Y);

            if (parent is Canvas)
            {
                TransformHelper.SetCanvasX(AdornedFE, centerNew.X - newW / 2 - AdornedFE.Margin.Left);
                TransformHelper.SetCanvasY(AdornedFE, centerNew.Y - newH / 2 - AdornedFE.Margin.Top);
            }
            else
            {
                TransformHelper.SetTransformX(AdornedFE, centerNew.X - newW / 2 - AdornedFE.Margin.Left);
                TransformHelper.SetTransformY(AdornedFE, centerNew.Y - newH / 2 - AdornedFE.Margin.Top);
            }

            ResizeRequested?.Invoke(this, new ResizeEventArgs(ResizeDirection, localDelta));
        }

        private void EndResize()
        {
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyEndResize();
            }
        }

        internal void ApplyEndResize()
        {
            IsResizing = false;
            ResizeCompleted?.Invoke(this, ResizeDirection);
        }

        #endregion

        #region Rotation

        private static bool IsRotationOperation(TransformOperation op)
        {
            return op == TransformOperation.Rotate || (op >= TransformOperation.RotateTopLeft && op <= TransformOperation.RotateBottomLeft);
        }

        /// <summary>
        /// 在旋转开始时捕获鼠标角度偏移 (同 RSTransformRig.Rotation_DragStarted)。
        /// </summary>
        private void BeginRotation(MouseEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 计算元素中心在父坐标系中的位置
            Point centerLocal = new Point(AdornedFE.ActualWidth / 2, AdornedFE.ActualHeight / 2);
            Point centerInParent = AdornedFE.TranslatePoint(centerLocal, parent);
            Point mousePos = e.GetPosition(parent);

            double radians = Math.Atan2(mousePos.Y - centerInParent.Y, mousePos.X - centerInParent.X);
            double currentMouseAngle = radians * (180 / Math.PI);

            // 捕获绝对鼠标角度与当前 RotationAngle 之间的偏移量
            InitialRotationOffset = (currentMouseAngle + 90) - this.RotationAngle;
        }

        /// <summary>
        /// 处理每帧的旋转增量 (同 RSTransformRig.Rotation_DragDelta)。
        /// </summary>
        private void PerformRotationDelta(MouseEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 计算元素中心
            Point centerLocal = new Point(AdornedFE.ActualWidth / 2, AdornedFE.ActualHeight / 2);
            Point centerInParent = AdornedFE.TranslatePoint(centerLocal, parent);
            Point mousePos = e.GetPosition(parent);

            double radians = Math.Atan2(mousePos.Y - centerInParent.Y, mousePos.X - centerInParent.X);
            double currentMouseAngle = radians * (180 / Math.PI);

            double finalRotation = (currentMouseAngle + 90 - InitialRotationOffset) % 360;
            if (finalRotation < 0)
            {
                finalRotation += 360;
            }

            double delta = finalRotation - this.RotationAngle;
            if (delta > 180) 
            {
                delta -= 360;
            }
            if (delta < -180) 
            {
                delta += 360;
            }

            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyRotationChange(delta, this == item ? finalRotation : (double?)null);
            }
        }

        internal void ApplyRotationChange(double delta, double? finalRotationOverride)
        {
            double newAngle;
            if (finalRotationOverride.HasValue)
            {
                newAngle = finalRotationOverride.Value;
            }
            else
            {
                newAngle = (this.RotationAngle + delta) % 360;
                if (newAngle < 0) 
                {
                    newAngle += 360;
                }
            }

            this.RotationAngle = newAngle;
            TransformHelper.SetRotation(AdornedElement, newAngle);
            RotationRequested?.Invoke(this, newAngle);
        }

        private void EndRotation()
        {
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyEndRotation();
            }
        }

        internal void ApplyEndRotation()
        {
            RotationCompleted?.Invoke(this, this.RotationAngle);
        }

        #endregion

        #region Direction Button Click

        /// <summary>
        /// 检测点击是否落在方向按钮的三角区域上。
        /// 如果是，则更新 RectDirection 并返回 true。
        /// </summary>
        private bool HandleDirectionButtonClick(Point localPoint)
        {
            if (!IsSingleSelect)
            {
                return false;
            }

            // 方向按钮由 RSTransformVisual.DrawDirectionButtons 绘制
            // 使用 RSTransformVisual 的 HitTest 已包含 Move/Resize/Rotate 区域
            // 但方向按钮区域需要单独处理（它们绘制在 DirectionHost 区域内）
            var directionHit = TransformVisual.HitTestDirectionButton(localPoint, VisualPixelSize, RectDirection);
            if (directionHit != RectDirection)
            {
                this.RectDirection = directionHit;
                UpdateVisual();
                return true;
            }
            return false;
        }

        #endregion

        #region Keyboard

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (!IsSelect || AdornedFE == null)
            {
                return;
            }

            double step = 10.0;
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Shift))
            {
                step = 1.0;
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

            PerformMoveDelta(delta);
            e.Handled = true;
        }

        #endregion
    }
}

