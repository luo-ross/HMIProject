using RS.Widgets.Controls;
using RS.Widgets.CustomEventArgs;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.Widgets.Services;
using RS.Widgets.Structs;
using RS.Widgets.Utilities;
using RS.Widgets.Visuals;
using RS.Widgets.UndoActions;
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
        #region Fields

        private FrameworkElement AdornedFE => AdornedElement as FrameworkElement;

        // 全局选中服务（所有 TransformAdorner 实例共享）
        private static readonly RSSelectService<TransformAdorner> SelectionService = new RSSelectService<TransformAdorner>();

        // 全局撤销服务 (该服务实例仅供 TransformAdorner 系统使用，独立于外部全局服务)
        internal static readonly IUndoService UndoService = new UndoService();


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
        private bool IsDragging;              // 已超过最小拖拽阈值，正式开始拖拽
        private TransformOperation PendingOperation;  // MouseDown 时 hit-test 结果（拖拽操作候选）
        private TransformOperation CurrentOperation;  // 实际拖拽中的操作
        private Point MouseDownPosition;      // MouseDown 时在父级坐标系中的位置
        private Point LastMouseScreen;        // 上一帧鼠标在父级坐标系中的位置

        private static TransformUndoAction currentUndoAction;

        private RectDirection? HoveredDirectionButton;

        private double InitialRotationOffset;

        private bool IsResizing;
        private Point ResizeAnchorInParent;
        private double ResizeInitialWidth;
        private double ResizeInitialHeight;
        private Matrix ResizeInitialTransformMatrix;
        private Vector ResizeAccDelta;
        private ResizeGripDirection ResizeDirection;
        private bool _isInternalSync;

        private Matrix AdornerMatrix = Matrix.Identity;
        private bool WasAnySelectedInStack = false;

        #endregion

        #region Events

        public event EventHandler<double>? RotationStarted;
        public event EventHandler<double>? RotationRequested;
        public event EventHandler<double>? RotationCompleted;
        public event EventHandler? TranslationStarted;
        public event EventHandler<Vector>? TranslationRequested;
        public event EventHandler? TranslationCompleted;
        public event EventHandler<ResizeGripDirection>? ResizeStarted;
        public event EventHandler<ResizeEventArgs>? ResizeRequested;
        public event EventHandler<ResizeGripDirection>? ResizeCompleted;

        #endregion

        #region Dependency Properties

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is TransformAdorner adorner)
            {
                adorner.UpdateVisual();
            }
        }
        public Brush BorderBrush
        {
            get { return (Brush)GetValue(BorderBrushProperty); }
            set { SetValue(BorderBrushProperty, value); }
        }

        public static readonly DependencyProperty BorderBrushProperty =
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(TransformAdorner),
                new PropertyMetadata(null, OnVisualPropertyChanged));


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

        #endregion

        #region Properties

        private TransformData dataModel;
        public TransformData DataModel
        {
            get
            {
                return dataModel;
            }
            set
            {
                if (dataModel != null)
                {
                    dataModel.PropertyChanged -= OnDataModelPropertyChanged;
                }
                dataModel = value;
                if (dataModel != null)
                {
                    dataModel.PropertyChanged += OnDataModelPropertyChanged;
                }
                UpdateDataModel();
            }
        }

        private void OnDataModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (_isInternalSync) return;

            // 当模型属性（例如经由撤销/重做）改变时，同步回元素
            SyncModelToElement();
        }

        #endregion

        #region Constructor & Initialization

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


        #endregion

        #region Selection

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
                SortAdornersByZIndex(hitList);

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
                SortAdornersByZIndex(hitList);

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

        #region 鼠标事件

        protected override void OnMouseLeftButtonDown(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonDown(e);

            var window = Window.GetWindow(this);
            var hitList = VisualHelper.FindAllFromPoint<TransformAdorner>(window, e.GetPosition(window));
            WasAnySelectedInStack = hitList.Any(r => r.IsSelect);

            TransformAdorner target = this;

            // 如果没有任何元素被选中
            if (!WasAnySelectedInStack)
            {
                target = Select(e);
            }
            else
            {
                SortAdornersByZIndex(hitList);
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
            if (AdornedElement is UIElement ui)
            {
                ui.Focus();
            }

            Point pt = ToLocalPoint(e.GetPosition(this));
            var operation = HitTest(pt);

            if (IsSingleSelect && HandleDirectionButtonClick(pt))
            {
                e.Handled = true;
                return;
            }

            if (operation == TransformOperation.None)
            {
                return;
            }
          
            PendingOperation = operation;
            MouseDownPosition = GetScreenPosition(e);
            LastMouseScreen = MouseDownPosition;
            this.CaptureMouse(); 
            IsDragging = false;

            this.Cursor = GetCursorForOperation(PendingOperation);
            this.Focus();
            e.Handled = true;
        }

        protected override void OnMouseLeftButtonUp(MouseButtonEventArgs e)
        {
            base.OnMouseLeftButtonUp(e);

            if (IsDragging)
            {
                if (IsResizeOperation(CurrentOperation))
                {
                    EndResize();
                }
                else if (IsRotationOperation(CurrentOperation))
                {
                    EndRotation();
                }
                else if (CurrentOperation == TransformOperation.Move)
                {
                    EndMove();
                }

                IsDragging = false;
                CurrentOperation = TransformOperation.None;
                HoveredDirectionButton = null;
                UpdateDataModel(); 
            }
            if (this.IsMouseCaptured)
            {
                if (WasAnySelectedInStack)
                {
                    Select(e);
                }
            }

            if (this.IsMouseCaptured)
            {
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
     
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (this.IsMouseCaptured && !IsDragging)
            {
                Point current = GetScreenPosition(e);
                Vector diff = current - MouseDownPosition;

                if (Math.Abs(diff.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    CurrentOperation = PendingOperation;
                    IsDragging = true;
                    LastMouseScreen = current;
                    if (IsResizeOperation(CurrentOperation))
                    {
                        BeginResize(CurrentOperation);
                    }
                    else if (IsRotationOperation(CurrentOperation))
                    {
                        BeginRotation(e);
                    }
                    else if (CurrentOperation == TransformOperation.Move)
                    {
                        BeginMove();
                    }
                }
                else
                {
                    this.Cursor = GetCursorForOperation(PendingOperation);
                }
                e.Handled = true;
                return;
            }

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

            Point pt = ToLocalPoint(e.GetPosition(this));
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

            BeginMove();
            PerformMoveDelta(delta);
            EndMove();
            e.Handled = true;
        }
        #endregion

        #region Transform Operations

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
            UpdateDataModel();
        }

        private void BeginMove()
        {
            BeginUndoAction("移动");
            foreach (var item in SelectionService.SelectedItems) item.ApplyBeginMove();
        }

        internal void ApplyBeginMove()
        {
            TranslationStarted?.Invoke(this, EventArgs.Empty);
            ExecuteAttachedCommand(TransformHelper.MoveStartedCommandProperty, DataModel);
        }

        private void EndMove()
        {
            CommitUndoAction();
            foreach (var item in SelectionService.SelectedItems) item.ApplyEndMove();
        }

        internal void ApplyEndMove()
        {
            TranslationCompleted?.Invoke(this, EventArgs.Empty);
            ExecuteAttachedCommand(TransformHelper.MoveCompletedCommandProperty, DataModel);
        }
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
            BeginUndoAction("缩放");
            foreach (var item in SelectionService.SelectedItems) item.ApplyBeginResize(op);
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

            Point anchorLocal = GetResizeAnchorPoint(ResizeDirection, AdornedFE.ActualWidth, AdornedFE.ActualHeight);

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
            ExecuteAttachedCommand(TransformHelper.ResizeStartedCommandProperty, DataModel);
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

            bool isShift = Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            // ── 等比例缩放 (Shift) ──
            if (isShift && ResizeInitialWidth > 0 && ResizeInitialHeight > 0)
            {
                double aspectRatio = ResizeInitialWidth / ResizeInitialHeight;

                switch (ResizeDirection)
                {
                    case ResizeGripDirection.TopLeft:
                    case ResizeGripDirection.TopRight:
                    case ResizeGripDirection.BottomLeft:
                    case ResizeGripDirection.BottomRight:
                        // 角点缩放：取变化比例较大的轴作为基准
                        double ratioW = Math.Abs(dw_logical) / Math.Max(1, ResizeInitialWidth);
                        double ratioH = Math.Abs(dh_logical) / Math.Max(1, ResizeInitialHeight);

                        if (ratioW > ratioH)
                        {
                            newH = newW / aspectRatio;
                        }
                        else
                        {
                            newW = newH * aspectRatio;
                        }
                        break;

                    case ResizeGripDirection.Left:
                    case ResizeGripDirection.Right:
                        // 左右边缘：始终固定高度比例
                        newH = newW / aspectRatio;
                        break;

                    case ResizeGripDirection.Top:
                    case ResizeGripDirection.Bottom:
                        // 上下边缘：始终固定宽度比例
                        newW = newH * aspectRatio;
                        break;
                }
            }

            // 限制尺寸
            if (newW < 1) 
            { 
                newW = 1; 
                if (isShift) 
                {
                    newH = newW / (ResizeInitialWidth / ResizeInitialHeight);
                }
            }
            if (newH < 1) 
            { 
                newH = 1; 
                if (isShift) 
                {
                    newW = newH * (ResizeInitialWidth / ResizeInitialHeight);
                }
            }

            // 再次检查 Min/Max 限制，并在等比例模式下保持比例
            if (newW < AdornedFE.MinWidth) 
            { 
                newW = AdornedFE.MinWidth; 
                if (isShift)
                {
                    newH = newW / (ResizeInitialWidth / ResizeInitialHeight);
                }
            }
            if (newW > AdornedFE.MaxWidth) 
            { 
                newW = AdornedFE.MaxWidth; 
                if (isShift)
                {
                    newH = newW / (ResizeInitialWidth / ResizeInitialHeight);
                }
            }
            if (newH < AdornedFE.MinHeight) 
            { 
                newH = AdornedFE.MinHeight; 
                if (isShift)
                {
                    newW = newH * (ResizeInitialWidth / ResizeInitialHeight);
                }
            }
            if (newH > AdornedFE.MaxHeight) 
            { 
                newH = AdornedFE.MaxHeight; 
                if (isShift)
                {
                    newW = newH * (ResizeInitialWidth / ResizeInitialHeight);
                }
            }

            AdornedFE.Width = newW;
            AdornedFE.Height = newH;

            // 修正 AccDelta 以匹配实际尺寸（防止死区）
            // 优化：在等比例缩放 (Shift) 时，如果不触碰物理边界限制，我们不强制同步 AccDelta。
            // 这样鼠标可以自由在对角线附近晃动而不会因为每帧的修剪产生步进感（丝滑的关键）。
            bool hitPhysicalLimit = (newW <= 1) || (newW <= AdornedFE.MinWidth) || (newW >= AdornedFE.MaxWidth) ||
                                   (newH <= 1) || (newH <= AdornedFE.MinHeight) || (newH >= AdornedFE.MaxHeight);

            if (!isShift || hitPhysicalLimit)
            {
                double clamped_dw_pix = (newW - ResizeInitialWidth) * GlobalScaleX;
                double clamped_dh_pix = (newH - ResizeInitialHeight) * GlobalScaleY;

                switch (ResizeDirection)
                {
                    case ResizeGripDirection.TopLeft:
                        ResizeAccDelta.X = -clamped_dw_pix;
                        ResizeAccDelta.Y = -clamped_dh_pix;
                        break;
                    case ResizeGripDirection.Top:
                        ResizeAccDelta.Y = -clamped_dh_pix;
                        break;
                    case ResizeGripDirection.TopRight:
                        ResizeAccDelta.X = clamped_dw_pix;
                        ResizeAccDelta.Y = -clamped_dh_pix;
                        break;
                    case ResizeGripDirection.Left:
                        ResizeAccDelta.X = -clamped_dw_pix;
                        break;
                    case ResizeGripDirection.Right:
                        ResizeAccDelta.X = clamped_dw_pix;
                        break;
                    case ResizeGripDirection.BottomLeft:
                        ResizeAccDelta.X = -clamped_dw_pix;
                        ResizeAccDelta.Y = clamped_dh_pix;
                        break;
                    case ResizeGripDirection.Bottom:
                        ResizeAccDelta.Y = clamped_dh_pix;
                        break;
                    case ResizeGripDirection.BottomRight:
                        ResizeAccDelta.X = clamped_dw_pix;
                        ResizeAccDelta.Y = clamped_dh_pix;
                        break;
                }
            }

            Point anchorLocalNew = GetResizeAnchorPoint(ResizeDirection, newW, newH);

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
            UpdateDataModel();
        }

        private void EndResize()
        {
            CommitUndoAction();
            foreach (var item in SelectionService.SelectedItems) item.ApplyEndResize();
        }

        internal void ApplyEndResize()
        {
            IsResizing = false;
            ResizeCompleted?.Invoke(this, ResizeDirection);
            ExecuteAttachedCommand(TransformHelper.ResizeCompletedCommandProperty, DataModel);
        }
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

            BeginUndoAction("旋转");
            foreach (var item in SelectionService.SelectedItems) item.ApplyBeginRotation();
        }

        internal void ApplyBeginRotation()
        {
            RotationStarted?.Invoke(this, this.RotationAngle);
            ExecuteAttachedCommand(TransformHelper.RotationStartedCommandProperty, DataModel);
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
            UpdateDataModel();
        }

        private void EndRotation()
        {
            CommitUndoAction();
            foreach (var item in SelectionService.SelectedItems) item.ApplyEndRotation();
        }

        internal void ApplyEndRotation()
        {
            RotationCompleted?.Invoke(this, this.RotationAngle);
            ExecuteAttachedCommand(TransformHelper.RotationCompletedCommandProperty, DataModel);
        }
      

        private bool HandleDirectionButtonClick(Point localPoint)
        {
            if (!IsSingleSelect)
            {
                return false;
            }
            var directionHit = TransformVisual.HitTestDirectionButton(localPoint, VisualPixelSize, RectDirection);
            if (directionHit != RectDirection)
            {
                this.RectDirection = directionHit;
                UpdateVisual();
                UpdateDataModel();
                return true;
            }
            return false;
        }
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
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle);

                case TransformOperation.ResizeLeft:
                case TransformOperation.ResizeRight:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle + 90);

                case TransformOperation.ResizeTopLeft:
                case TransformOperation.ResizeBottomRight:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle - 45);

                case TransformOperation.ResizeTopRight:
                case TransformOperation.ResizeBottomLeft:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle + 45);

                // ── Rotation cursors (rotated by element angle + corner offset) ──
                case TransformOperation.Rotate:
                    switch (RectDirection)
                    {
                        case RectDirection.Top: return GetRotatedCursor(BaseRotationCursorData, RotationAngle - 45);
                        case RectDirection.Right: return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 45);
                        case RectDirection.Bottom: return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 135);
                        case RectDirection.Left: return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 225);
                        default: return GetRotatedCursor(BaseRotationCursorData, RotationAngle - 45);
                    }

                case TransformOperation.RotateTopLeft:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle - 90);

                case TransformOperation.RotateTopRight:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle);

                case TransformOperation.RotateBottomRight:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 90);

                case TransformOperation.RotateBottomLeft:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 180);

                default:
                    return Cursors.Arrow;
            }
        }

        /// <summary>
        /// 创建一个动态旋转的旋转光标。
        /// </summary>
        private static Cursor GetRotatedCursor(CursorData baseData, double angle)
        {
            if (baseData.Bitmap == null) return Cursors.Arrow;
            var cursorData = CursorHelper.RotateCursor(baseData, angle);
            if (cursorData.Bitmap == null) return Cursors.Arrow;
            return CursorHelper.CreateCursor(cursorData.Bitmap, cursorData.HotspotX, cursorData.HotspotY);
        }

        #endregion

        #region DataModel Sync

        /// <summary>
        /// 更新绑定的数据模型
        /// </summary>
        public void UpdateDataModel()
        {
            if (DataModel == null || AdornedFE == null || _isInternalSync)
            {
                return;
            }

            _isInternalSync = true;
            try
            {
                var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
                if (parent == null)
                {
                    return;
                }

            // 基本数据
            DataModel.Width = AdornedFE.ActualWidth;
            DataModel.Height = AdornedFE.ActualHeight;
            DataModel.Angle = RotationAngle;
            DataModel.Direction = RectDirection;

            // 位置 (Canvas 或 Transform)
            if (parent is Canvas)
            {
                double x_coord = Canvas.GetLeft(AdornedFE);
                double y_coord = Canvas.GetTop(AdornedFE);

                // 如果是 NaN (未设置)，尝试通过坐标转换还原
                if (double.IsNaN(x_coord) || double.IsNaN(y_coord))
                {
                    Point pos = AdornedFE.TranslatePoint(new Point(0, 0), parent);
                    if (double.IsNaN(x_coord)) x_coord = pos.X;
                    if (double.IsNaN(y_coord)) y_coord = pos.Y;
                }

                DataModel.X = x_coord;
                DataModel.Y = y_coord;
            }
            else
            {
                DataModel.X = TransformHelper.GetTransformX(AdornedFE);
                DataModel.Y = TransformHelper.GetTransformY(AdornedFE);
            }

            // 四个角点坐标 (相对于父容器)
            double w = AdornedFE.ActualWidth;
            double h = AdornedFE.ActualHeight;

            DataModel.TopLeft = AdornedFE.TranslatePoint(new Point(0, 0), parent);
            DataModel.TopRight = AdornedFE.TranslatePoint(new Point(w, 0), parent);
            DataModel.BottomLeft = AdornedFE.TranslatePoint(new Point(0, h), parent);
            DataModel.BottomRight = AdornedFE.TranslatePoint(new Point(w, h), parent);
            }
            finally
            {
                _isInternalSync = false;
            }
        }

        public void SyncModelToElement()
        {
            if (AdornedFE == null || DataModel == null || _isInternalSync) return;

            _isInternalSync = true;
            try
            {
                var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
                if (parent == null) return;

                if (parent is Canvas)
                {
                    TransformHelper.SetCanvasX(AdornedFE, DataModel.X);
                    TransformHelper.SetCanvasY(AdornedFE, DataModel.Y);
                }
                else
                {
                    TransformHelper.SetTransformX(AdornedFE, DataModel.X);
                    TransformHelper.SetTransformY(AdornedFE, DataModel.Y);
                }

                if (DataModel.Width > 0) AdornedFE.Width = DataModel.Width;
                if (DataModel.Height > 0) AdornedFE.Height = DataModel.Height;

                RotationAngle = DataModel.Angle;
                UpdateVisual();
            }
            finally
            {
                _isInternalSync = false;
            }
        }
        #endregion

        #region Rendering & Layout

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
            UpdateDataModel();
        }

        protected override int VisualChildrenCount
        {
            get { return Visuals.Count; }
        }

        protected override Visual GetVisualChild(int index)
        {
            return Visuals[index];
        }

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

        #region Helpers

        private static Point GetResizeAnchorPoint(ResizeGripDirection direction, double width, double height)
        {
            return direction switch
            {
                ResizeGripDirection.TopLeft => new Point(width, height),
                ResizeGripDirection.Top => new Point(width / 2, height),
                ResizeGripDirection.TopRight => new Point(0, height),
                ResizeGripDirection.Left => new Point(width, height / 2),
                ResizeGripDirection.Right => new Point(0, height / 2),
                ResizeGripDirection.BottomLeft => new Point(width, 0),
                ResizeGripDirection.Bottom => new Point(width / 2, 0),
                ResizeGripDirection.BottomRight => new Point(0, 0),
                _ => new Point()
            };
        }

        private static void BeginUndoAction(string actionName)
        {
            currentUndoAction = new TransformUndoAction { Name = actionName };
            foreach (var item in SelectionService.SelectedItems)
            {
                var memento = TransformMemento.Capture(item.DataModel);
                if (memento != null)
                {
                    currentUndoAction.Changes.Add((item.DataModel, memento, null));
                }
            }
        }

        private static void CommitUndoAction()
        {
            if (currentUndoAction == null) return;
            foreach (var item in SelectionService.SelectedItems)
            {
                var memento = TransformMemento.Capture(item.DataModel);
                var entry = currentUndoAction.Changes.FirstOrDefault(c => c.target == item.DataModel);
                if (entry != default && memento != null)
                {
                    var index = currentUndoAction.Changes.IndexOf(entry);
                    currentUndoAction.Changes[index] = (entry.target, entry.before, memento);
                }
            }
            UndoService.AddAction(currentUndoAction);
            currentUndoAction = null;
        }

        private static void SortAdornersByZIndex(List<TransformAdorner> hitList)
        {
            hitList.Sort((a, b) => 
            {
                UIElement elA = a.AdornedElement as UIElement;
                UIElement elB = b.AdornedElement as UIElement;
                if (elA == null || elB == null) return 0;
                
                Panel pA = VisualTreeHelper.GetParent(elA) as Panel;
                Panel pB = VisualTreeHelper.GetParent(elB) as Panel;
                if (pA != pB || pA == null) return 0;
            
                int zA = Panel.GetZIndex(elA);
                int zB = Panel.GetZIndex(elB);
                if (zA != zB) return zA.CompareTo(zB);
            
                int idxA = pA.Children.IndexOf(elA);
                int idxB = pB.Children.IndexOf(elB);
                return idxA.CompareTo(idxB);
            });
        }

        private void ExecuteAttachedCommand(DependencyProperty commandProperty, object parameter)
        {
            if (AdornedElement == null) return;
            var command = AdornedElement.GetValue(commandProperty) as ICommand;
            if (command != null && command.CanExecute(parameter))
            {
                command.Execute(parameter);
            }
        }
        #endregion

    }
}

