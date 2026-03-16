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
    public class RSTransformAdorner : Adorner, ISelectable
    {
        #region Fields

        private FrameworkElement AdornedFE
        {
            get
            {
                return AdornedElement as FrameworkElement;
            }
        }

        private static readonly SelectService<RSTransformAdorner> SelectionService = new SelectService<RSTransformAdorner>();

        internal static readonly IUndoService UndoService = new UndoService();


        private double GlobalScaleX = 1;
        private double GlobalScaleY = 1;
        private Size VisualPixelSize = new Size(0, 0);

        private readonly RSTransformVisual TransformVisual;
        private readonly VisualCollection Visuals;

        // 布局边距 — 扩大 Adorner 的布局边界，
        // 以便在 AdornedElement (负坐标) 外部绘制的区域
        private const double HitPadding = 10.0;

        // 缓存的光标数据（加载一次，在实例间共享）
        private static readonly CursorData BaseRotationCursorData;
        private static readonly CursorData BaseResizeCursorData;

        // ── 鼠标 / 拖拽状态 ──
        private bool IsDragging;              // 已超过最小拖拽阈值，正式开始拖拽
        private VisualOperation PendingOperation;  // MouseDown 时 hit-test 结果（拖拽操作候选）
        private VisualOperation CurrentOperation;  // 实际拖拽中的操作
        private Point MouseDownPosition;      // MouseDown 时在父级坐标系中的位置
        private Point LastMouseScreen;        // 上一帧鼠标在父级坐标系中的位置

        private static TransformUndoAction currentUndoAction;

        private VisualOperation HoveredDirectionButton = VisualOperation.None;

        private double InitialRotationOffset;

        private bool IsResizing;
        private Point ResizeAnchorInParent;
        private double ResizeInitialWidth;
        private double ResizeInitialHeight;
        private Matrix ResizeInitialTransformMatrix;
        private Vector ResizeAccDelta;
        private ResizeGripDirection ResizeDirection;
        private bool IsInternalSync;

        private Matrix AdornerMatrix = Matrix.Identity;
        private bool WasAnySelectedInStack = false;
        private bool PendingSelectionCycle = false;
        private bool _isPivotVisible = false;

        private static readonly HashSet<Window> HookedWindows = new HashSet<Window>();

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
            if (d is RSTransformAdorner adorner)
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
            DependencyProperty.Register(nameof(BorderBrush), typeof(Brush), typeof(RSTransformAdorner),
                new PropertyMetadata(null, OnVisualPropertyChanged));


        public double RotationAngle
        {
            get { return (double)GetValue(RotationAngleProperty); }
            set { SetValue(RotationAngleProperty, value); }
        }

        public static readonly DependencyProperty RotationAngleProperty =
            DependencyProperty.Register(nameof(RotationAngle), typeof(double), typeof(RSTransformAdorner),
                new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));

        public RectDirection RectDirection
        {
            get { return (RectDirection)GetValue(RectDirectionProperty); }
            set { SetValue(RectDirectionProperty, value); }
        }

        public static readonly DependencyProperty RectDirectionProperty =
            DependencyProperty.Register(nameof(RectDirection), typeof(RectDirection), typeof(RSTransformAdorner),
                new FrameworkPropertyMetadata(RectDirection.Top, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsSelect
        {
            get { return (bool)GetValue(IsSelectProperty); }
            set { SetValue(IsSelectProperty, value); }
        }

        public static readonly DependencyProperty IsSelectProperty =
            DependencyProperty.Register(nameof(IsSelect), typeof(bool), typeof(RSTransformAdorner),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsSingleSelect
        {
            get { return (bool)GetValue(IsSingleSelectProperty); }
            set { SetValue(IsSingleSelectProperty, value); }
        }

        public static readonly DependencyProperty IsSingleSelectProperty =
            DependencyProperty.Register(nameof(IsSingleSelect), typeof(bool), typeof(RSTransformAdorner),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsDirectionEnabled
        {
            get { return (bool)GetValue(IsDirectionEnabledProperty); }
            set { SetValue(IsDirectionEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsDirectionEnabledProperty =
            DependencyProperty.Register(nameof(IsDirectionEnabled), typeof(bool), typeof(RSTransformAdorner),
                new FrameworkPropertyMetadata(false, FrameworkPropertyMetadataOptions.BindsTwoWayByDefault, OnVisualPropertyChanged));


        public bool IsRotationEnabled
        {
            get { return (bool)GetValue(IsRotationEnabledProperty); }
            set { SetValue(IsRotationEnabledProperty, value); }
        }

        public static readonly DependencyProperty IsRotationEnabledProperty =
            DependencyProperty.Register(nameof(IsRotationEnabled), typeof(bool), typeof(RSTransformAdorner),
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
            if (IsInternalSync) 
            {
                return;
            }
            SyncModelToElement();
        }

        #endregion

        #region Constructor & Initialization

        static RSTransformAdorner()
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

        public RSTransformAdorner(FrameworkElement adornedElement) : base(adornedElement)
        {
            var brush = new SolidColorBrush(ColorHelper.GetNextVibrantColor());
            brush.Freeze();
            this.SetCurrentValue(BorderBrushProperty, brush);

            TransformVisual = new RSTransformVisual();
            Visuals = new VisualCollection(this) { TransformVisual };

            this.Focusable = true;
            this.Loaded += OnTransformAdornerLoaded;
            this.Unloaded += OnTransformAdornerUnloaded;
        }

        private void OnTransformAdornerUnloaded(object sender, RoutedEventArgs e)
        {
            var window = Window.GetWindow(this);
            if (window != null)
            {
                window.MouseLeftButtonUp -= Window_MouseLeftButtonUp;
            }
            this.Loaded -= OnTransformAdornerLoaded;
            this.Unloaded -= OnTransformAdornerUnloaded;
        }

        private void OnTransformAdornerLoaded(object sender, RoutedEventArgs e)
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

                if (!HookedWindows.Contains(window))
                {
                    window.PreviewKeyDown += Window_PreviewKeyDown;
                    window.SizeChanged += Window_SizeChanged;
                    window.LocationChanged += Window_LocationChanged;
                    window.Unloaded += Window_Unloaded;
                    HookedWindows.Add(window);
                }
            }
        }

        private static void Window_Unloaded(object sender, RoutedEventArgs e)
        {
            if (sender is Window window)
            {
                window.PreviewKeyDown -= Window_PreviewKeyDown;
                window.SizeChanged -= Window_SizeChanged;
                window.LocationChanged -= Window_LocationChanged;
                window.Unloaded -= Window_Unloaded;
                HookedWindows.Remove(window);
            }
        }

        private static void Window_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateAdornersForWindow(sender as Window);
        }

        private static void Window_LocationChanged(object sender, EventArgs e)
        {
            UpdateAdornersForWindow(sender as Window);
        }

        private static void UpdateAdornersForWindow(Window window)
        {
            if (window == null)
            {
                return;
            }
            var layer = AdornerLayer.GetAdornerLayer(window);
            if (layer != null)
            {
                layer.Update();
            }
        }

        private static void Window_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.Z)
                {
                    if (UndoService.CanUndo)
                    {
                        UndoService.Undo();
                        e.Handled = true;
                    }
                }
                else if (e.Key == Key.Y || (e.Key == Key.Z && Keyboard.Modifiers.HasFlag(ModifierKeys.Shift)))
                {
                    if (UndoService.CanRedo)
                    {
                        UndoService.Redo();
                        e.Handled = true;
                    }
                }
            }
        }

        private static void Window_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (SelectionService.SelectedItems.Count > 0 && sender is Window window)
            {
                Point pt = e.GetPosition(window);
                if (VisualHelper.TryFindFromPoint<RSTransformAdorner>(window, pt) == null)
                {
                    UnselectAll();
                }
            }
        }
        #endregion

        #region Selection
        /// <summary>
        /// 取消选中所有项
        /// </summary>
        public static void UnselectAll()
        {
            SelectionService.ClearSelect();
        }

        public void SelectAll()
        {
            var window = Window.GetWindow(this);
            if (window == null)
            {
                return;
            }

            var allAdorners = VisualHelper.FindVisualChildren<RSTransformAdorner>(window).ToList();
            foreach (var adorner in allAdorners)
            {
                SelectionService.AddSelect(adorner);
            }
        }

        public RSTransformAdorner Select(MouseEventArgs e)
        {
            var window = Window.GetWindow(this);
            var hitList = VisualHelper.FindAllFromPoint<RSTransformAdorner>(window, e.GetPosition(window));

            bool isMulti = Keyboard.Modifiers.HasFlag(ModifierKeys.Control) || Keyboard.Modifiers.HasFlag(ModifierKeys.Shift);

            if (!hitList.Contains(this))
            {
                hitList.Add(this);
            }

            RSTransformAdorner target = this;

            if (isMulti)
            {
                SortAdornersByStableIndex(hitList);
                hitList.Reverse();

                bool allSelected = true;
                foreach (var r in hitList)
                {
                    if (!r.IsSelect)
                    {
                        allSelected = false;
                        break;
                    }
                }

                if (hitList.Count > 1 && allSelected)
                {
                    foreach (var r in hitList)
                    {
                        SelectionService.MultiSelect(r);
                    }
                    target = this;
                }
                else
                {
                    target = this;
                    foreach (var r in hitList)
                    {
                        if (!r.IsSelect)
                        {
                            target = r;
                            break;
                        }
                    }
                    SelectionService.MultiSelect(target);
                }
            }
            else
            {
                SortAdornersByStableIndex(hitList);
                
                RSTransformAdorner current = null;
                foreach (var r in hitList)
                {
                    if (r.IsSelect)
                    {
                        current = r;
                        break;
                    }
                }
                
                if (current != null && hitList.Count > 1)
                {
                    int currentIndex = hitList.IndexOf(current);
                    int targetIndex = currentIndex - 1;
                    if (targetIndex < 0) 
                    {
                        targetIndex = hitList.Count - 1;
                    }
                    target = hitList[targetIndex];
                }
                else
                {
                    target = hitList[hitList.Count - 1];
                }

                SelectionService.SingleSelect(target);
            }
            target.BringToFront();
            return target;
        }

        private void BringToFront()
        {
            // 仅在 AdornerLayer 置顶装饰器，不改变被装饰元素的 Panel.ZIndex
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
            var hitList = VisualHelper.FindAllFromPoint<RSTransformAdorner>(window, e.GetPosition(window));
            bool anySelected = false;
            foreach (var r in hitList)
            {
                if (r.IsSelect)
                {
                    anySelected = true;
                    break;
                }
            }
            WasAnySelectedInStack = anySelected;
            
            PendingSelectionCycle = false;
            RSTransformAdorner target = null;

            for (int i = hitList.Count - 1; i >= 0; i--)
            {
                if (hitList[i].IsSelect)
                {
                    target = hitList[i];
                    break;
                }
            }

            if (target == null)
            {
                if (hitList.Count > 0)
                {
                    target = hitList[hitList.Count - 1];
                }
                else
                {
                    target = this;
                }
            }

            if (target != this)
            {
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
            var operation = GetVisualOperation(pt);

            if (IsSingleSelect)
            {
                var directionHit = TransformVisual.GetDirectionButtonOperation(pt, VisualPixelSize, RectDirection);
                if (directionHit != VisualOperation.None)
                {
                    this.CaptureMouse();
                    e.Handled = true;
                    return;
                }
            }

            if (operation == VisualOperation.None)
            {
                return;
            }
          
            PendingOperation = operation;
            MouseDownPosition = GetScreenPosition(e);
            LastMouseScreen = MouseDownPosition;
            this.CaptureMouse(); 
            IsDragging = false;

            // 选择逻辑改进：
            // 如果点击的是背景(Move) 且 操作项未选中，则立即选中（提升反馈感）
            if (PendingOperation == VisualOperation.Move)
            {
                if (!this.IsSelect)
                {
                    Select(e);
                }
                else if (WasAnySelectedInStack)
                {
                    // 如果操作项已选中，则标记为待循环（在 MouseUp 中处理，以免干扰拖拽）
                    PendingSelectionCycle = true;
                }
            }

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
                else if (CurrentOperation == VisualOperation.Move)
                {
                    EndMove();
                }

                IsDragging = false;
                CurrentOperation = VisualOperation.None;
                HoveredDirectionButton = VisualOperation.None;
                UpdateDataModel(); 
            }
            else 
            {
                Point pt = ToLocalPoint(e.GetPosition(this));
                if (IsSingleSelect && HandleDirectionButtonClick(pt))
                {
                    if (this.IsMouseCaptured)
                    {
                        PendingOperation = VisualOperation.None;
                        this.ReleaseMouseCapture();
                    }
                    e.Handled = true;
                    return;
                }

                // 如果存在待处理的循环请求，则执行循环
                if (PendingSelectionCycle && this.IsMouseCaptured)
                {
                    Select(e);
                }
            }

            PendingSelectionCycle = false;

            if (this.IsMouseCaptured)
            {
                PendingOperation = VisualOperation.None;
                this.ReleaseMouseCapture();
            }
            e.Handled = true;
        }

      
        private Point GetScreenPosition(MouseEventArgs e)
        {
            return this.PointToScreen(e.GetPosition(this));
        }

        private Vector GetParentScale()
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return new Vector(1, 1);
            }

            PresentationSource source = PresentationSource.FromVisual(parent);
            Matrix matrixScreen = source?.CompositionTarget?.TransformToDevice ?? Matrix.Identity;

            GeneralTransform transformToRoot = parent.TransformToAncestor(source?.RootVisual ?? parent);
            if (transformToRoot is Transform t)
            {
                matrixScreen.Append(t.Value);
            }

            double scaleX = Math.Sqrt(matrixScreen.M11 * matrixScreen.M11 + matrixScreen.M12 * matrixScreen.M12);
            double scaleY = Math.Sqrt(matrixScreen.M21 * matrixScreen.M21 + matrixScreen.M22 * matrixScreen.M22);

            return new Vector(scaleX > 0 ? scaleX : 1.0, scaleY > 0 ? scaleY : 1.0);
        }
     
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (IsDragging)
            {
                Point currentScreen = GetScreenPosition(e);
                Vector screenDelta = currentScreen - LastMouseScreen;
                LastMouseScreen = currentScreen;
                
                switch (CurrentOperation)
                {
                    case VisualOperation.Move:
                        PerformMoveDelta(screenDelta);
                        break;
                    case VisualOperation.ResizeTop:
                    case VisualOperation.ResizeBottom:
                    case VisualOperation.ResizeLeft:
                    case VisualOperation.ResizeRight:
                    case VisualOperation.ResizeTopLeft:
                    case VisualOperation.ResizeTopRight:
                    case VisualOperation.ResizeBottomLeft:
                    case VisualOperation.ResizeBottomRight:
                        PerformResizeDelta(screenDelta);
                        break;
                    case VisualOperation.Rotate:
                    case VisualOperation.RotateTopLeft:
                    case VisualOperation.RotateTopRight:
                    case VisualOperation.RotateBottomLeft:
                    case VisualOperation.RotateBottomRight:
                        PerformRotationDelta(e);
                        break;
                    case VisualOperation.MovePivot:
                        PerformPivotDelta(screenDelta);
                        break;
                }
                e.Handled = true;
                return;
            }

            if (this.IsMouseCaptured && !IsDragging)
            {
                Point currentScreen = GetScreenPosition(e);
                Vector diff = currentScreen - MouseDownPosition;

                if (Math.Abs(diff.X) >= SystemParameters.MinimumHorizontalDragDistance ||
                    Math.Abs(diff.Y) >= SystemParameters.MinimumVerticalDragDistance)
                {
                    if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control)) 
                    {
                        return;
                    }

                    if (!IsSelect) 
                    {
                        Select(e);
                    }

                    CurrentOperation = PendingOperation;
                    IsDragging = true;
                    PendingSelectionCycle = false; // 一旦开始拖拽，取消循环选择标记
                    LastMouseScreen = currentScreen;

                    if (IsResizeOperation(CurrentOperation)) 
                    {
                        BeginResize(CurrentOperation);
                    }
                    else if (IsRotationOperation(CurrentOperation)) 
                    {
                        BeginRotation(e);
                    }
                    else if (CurrentOperation == VisualOperation.Move) 
                    {
                        BeginMove();
                    }
                    
                    // 仅在拖拽开始时刷新一次光标状态
                    this.Cursor = GetCursorForOperation(CurrentOperation);
                }
                else
                {
                    this.Cursor = GetCursorForOperation(PendingOperation);
                }
                e.Handled = true;
                return;
            }

            Point pt = ToLocalPoint(e.GetPosition(this));
            if (IsSingleSelect && IsDirectionEnabled)
            {
                var hitDir = TransformVisual.GetDirectionButtonOperation(pt, VisualPixelSize, RectDirection);
                VisualOperation newHover = hitDir;

                if (newHover != HoveredDirectionButton)
                {
                    HoveredDirectionButton = newHover;
                    UpdateVisual();
                }

                if (HoveredDirectionButton != VisualOperation.None)
                {
                    this.Cursor = Cursors.Hand;
                    return;
                }
            }
            else if (HoveredDirectionButton != VisualOperation.None)
            {
                HoveredDirectionButton = VisualOperation.None;
                UpdateVisual();
            }

            // 更新旋转中心显示逻辑
            double px = DataModel != null ? DataModel.PivotX : 0.5;
            double py = DataModel != null ? DataModel.PivotY : 0.5;

            var opPre = GetVisualOperation(pt, true); 
            bool isRotationHover = IsRotationOperation(opPre);
            bool isPivotHoverNow = TransformVisual.IsPointInPivot(pt, VisualPixelSize, px, py);

            bool shouldShowPivot = isRotationHover || 
                                  (IsDragging && (IsRotationOperation(CurrentOperation) || CurrentOperation == VisualOperation.MovePivot)) ||
                                  (_isPivotVisible && isPivotHoverNow);

            if (shouldShowPivot != _isPivotVisible)
            {
                _isPivotVisible = shouldShowPivot;
                UpdateVisual();
            }

            var op = GetVisualOperation(pt, false);
            this.Cursor = GetCursorForOperation(op);
        }

  
        protected override void OnMouseLeave(MouseEventArgs e)
        {
            base.OnMouseLeave(e);
            if (!IsDragging)
            {
                this.Cursor = Cursors.Arrow;
            }

            if (HoveredDirectionButton != VisualOperation.None)
            {
                HoveredDirectionButton = VisualOperation.None;
                UpdateVisual();
            }
        }
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (Keyboard.Modifiers.HasFlag(ModifierKeys.Control))
            {
                if (e.Key == Key.A)
                {
                    SelectAll();
                    e.Handled = true;
                    return;
                }
            }

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

            Vector parentScale = GetParentScale();
            double dx = screenDelta.X / parentScale.X;
            double dy = screenDelta.Y / parentScale.Y;

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

            UpdatePivotFeedback(); // Added this line
            if (TranslationRequested != null)
            {
                TranslationRequested.Invoke(this, screenDelta);
            }
            UpdateDataModel();
        }

        private void BeginMove()
        {
            BeginUndoAction("移动");
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyBeginMove();
            }
        }

        internal void ApplyBeginMove()
        {
            TranslationStarted?.Invoke(this, EventArgs.Empty);
            ExecuteAttachedCommand(TransformHelper.MoveStartedCommandProperty, DataModel);
        }

        private void EndMove()
        {
            CommitUndoAction();
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyEndMove();
            }
        }

        internal void ApplyEndMove()
        {
            TranslationCompleted?.Invoke(this, EventArgs.Empty);
            ExecuteAttachedCommand(TransformHelper.MoveCompletedCommandProperty, DataModel);
        }
        private static bool IsResizeOperation(VisualOperation op)
        {
            return op >= VisualOperation.ResizeTopLeft && op <= VisualOperation.ResizeLeft;
        }

        private static ResizeGripDirection ToResizeDirection(VisualOperation op)
        {
            switch (op)
            {
                case VisualOperation.ResizeTop: 
                    return ResizeGripDirection.Top;
                case VisualOperation.ResizeBottom: 
                    return ResizeGripDirection.Bottom;
                case VisualOperation.ResizeLeft: 
                    return ResizeGripDirection.Left;
                case VisualOperation.ResizeRight: 
                    return ResizeGripDirection.Right;
                case VisualOperation.ResizeTopLeft: 
                    return ResizeGripDirection.TopLeft;
                case VisualOperation.ResizeTopRight: 
                    return ResizeGripDirection.TopRight;
                case VisualOperation.ResizeBottomLeft: 
                    return ResizeGripDirection.BottomLeft;
                case VisualOperation.ResizeBottomRight: 
                    return ResizeGripDirection.BottomRight;
                default: 
                    return ResizeGripDirection.None;
            }
        }

  
        private void BeginResize(VisualOperation op)
        {
            BeginUndoAction("缩放");
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyBeginResize(op);
            }
        }

        internal void ApplyBeginResize(VisualOperation op)
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

            // screenDelta 是屏幕设备像素。GetParentScale() 获取全路径缩放。
            Vector parentScale = GetParentScale();

            Vector dipsDelta = new Vector(
                screenDelta.X / parentScale.X,
                screenDelta.Y / parentScale.Y);

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

            // 关键修复：使用更通用的锚点同步逻辑，确保在有旋转/自定义中心时 Resize 依然稳定
            double px = DataModel != null ? DataModel.PivotX : 0.5;
            double py = DataModel != null ? DataModel.PivotY : 0.5;
            double sx = TransformHelper.GetScaleX(AdornedFE);
            double sy = TransformHelper.GetScaleY(AdornedFE);
            double rotation = this.RotationAngle;

            // 1. 计算新尺寸下的局部变换矩阵（不含位移）
            Matrix mLocalNew = Matrix.Identity;
            mLocalNew.ScaleAt(sx, sy, newW * px, newH * py);
            mLocalNew.RotateAt(rotation, newW * px, newH * py);

            // 2. 将锚点(在旋转坐标系中应保持不动)在新的局部坐标系中进行变换
            Point anchorLocalNew = GetResizeAnchorPoint(ResizeDirection, newW, newH);
            Point anchorInParentNew = mLocalNew.Transform(anchorLocalNew);

            // 3. 补偿位移，使锚点的世界坐标保持为捕捉到的 ResizeAnchorInParent
            double newX = ResizeAnchorInParent.X - anchorInParentNew.X;
            double newY = ResizeAnchorInParent.Y - anchorInParentNew.Y;

            if (parent is Canvas)
            {
                TransformHelper.SetCanvasX(AdornedFE, newX - AdornedFE.Margin.Left);
                TransformHelper.SetCanvasY(AdornedFE, newY - AdornedFE.Margin.Top);
            }
            else
            {
                TransformHelper.SetTransformX(AdornedFE, newX - AdornedFE.Margin.Left);
                TransformHelper.SetTransformY(AdornedFE, newY - AdornedFE.Margin.Top);
            }

            UpdateDataModel();
            UpdatePivotFeedback(); 
            if (ResizeRequested != null)
            {
                ResizeRequested.Invoke(this, new ResizeEventArgs(ResizeDirection, localDelta));
            }
            UpdateDataModel();
        }

        private void EndResize()
        {
            CommitUndoAction();
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyEndResize();
            }
        }

        internal void ApplyEndResize()
        {
            IsResizing = false;
            ResizeCompleted?.Invoke(this, ResizeDirection);
            ExecuteAttachedCommand(TransformHelper.ResizeCompletedCommandProperty, DataModel);
        }
        private static bool IsRotationOperation(VisualOperation op)
        {
            return op == VisualOperation.Rotate || (op >= VisualOperation.RotateTopLeft && op <= VisualOperation.RotateBottomLeft);
        }

     
        private void BeginRotation(MouseEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 计算元素中心 (使用动态 Pivot)
            double px = DataModel != null ? DataModel.PivotX : 0.5;
            double py = DataModel != null ? DataModel.PivotY : 0.5;
            Point centerLocal = new Point(AdornedFE.ActualWidth * px, AdornedFE.ActualHeight * py);
            Point centerInParent = AdornedFE.TranslatePoint(centerLocal, parent);
            Point mousePos = e.GetPosition(parent);

            double radians = Math.Atan2(mousePos.Y - centerInParent.Y, mousePos.X - centerInParent.X);
            double currentMouseAngle = radians * (180 / Math.PI);

            // 捕获绝对鼠标角度与当前 RotationAngle 之间的偏移量
            InitialRotationOffset = (currentMouseAngle + 90) - this.RotationAngle;

            BeginUndoAction("旋转");
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyBeginRotation();
            }
        }

        internal void ApplyBeginRotation()
        {
            if (RotationStarted != null)
            {
                RotationStarted.Invoke(this, this.RotationAngle);
            }
            ExecuteAttachedCommand(TransformHelper.RotationStartedCommandProperty, DataModel);
        }
        

  
        private void PerformRotationDelta(MouseEventArgs e)
        {
            var parent = VisualTreeHelper.GetParent(AdornedElement) as UIElement;
            if (parent == null)
            {
                return;
            }

            // 计算元素中心
            double px = DataModel != null ? DataModel.PivotX : 0.5;
            double py = DataModel != null ? DataModel.PivotY : 0.5;
            Point centerLocal = new Point(AdornedFE.ActualWidth * px, AdornedFE.ActualHeight * py);
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

        private void PerformPivotDelta(Vector screenDelta)
        {
            if (AdornedFE == null || DataModel == null)
            {
                return;
            }

            double w = AdornedFE.ActualWidth;
            double h = AdornedFE.ActualHeight;
            if (w <= 0 || h <= 0)
            {
                return;
            }

            // 1. 获取当前的变换参数
            double rotation = this.RotationAngle;
            double sx = TransformHelper.GetScaleX(AdornedFE);
            double sy = TransformHelper.GetScaleY(AdornedFE);
            double oldPx = DataModel.PivotX;
            double oldPy = DataModel.PivotY;

            // 2. 记录旧中心在局部变换空间（不含平移）下的位置
            Matrix mOld = Matrix.Identity;
            mOld.ScaleAt(sx, sy, w * oldPx, h * oldPy);
            mOld.RotateAt(rotation, w * oldPx, h * oldPy);
            Point originInParentOld = mOld.Transform(new Point(0, 0));

            // 3. 计算新 Pivot (限制在边界内 0.0 - 1.0)
            Point pt = ToLocalPoint(Mouse.GetPosition(this));
            double newPx = Math.Max(0, Math.Min(1.0, pt.X / w));
            double newPy = Math.Max(0, Math.Min(1.0, pt.Y / h));

            // 4. 计算新中心在局部变换空间下的位置
            Matrix mNew = Matrix.Identity;
            mNew.ScaleAt(sx, sy, w * newPx, h * newPy);
            mNew.RotateAt(rotation, w * newPx, h * newPy);
            Point originInParentNew = mNew.Transform(new Point(0, 0));

            // 5. 补偿位移，使控件视觉位置保持不动
            double dx = originInParentOld.X - originInParentNew.X;
            double dy = originInParentOld.Y - originInParentNew.Y;

            DataModel.X += dx;
            DataModel.Y += dy;
            DataModel.PivotX = newPx;
            DataModel.PivotY = newPy;

            // 同步外部坐标反馈
            UpdatePivotFeedback();

            // 立即应用到元素，以便旋转实时生效
            if (AdornedFE.Parent is Canvas)
            {
                TransformHelper.SetCanvasX(AdornedFE, DataModel.X);
                TransformHelper.SetCanvasY(AdornedFE, DataModel.Y);
            }
            else
            {
                TransformHelper.SetTransformX(AdornedFE, DataModel.X);
                TransformHelper.SetTransformY(AdornedFE, DataModel.Y);
            }

            TransformHelper.UpdateRenderTransform(AdornedFE);
            UpdateVisual();
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
            UpdatePivotFeedback(); // Added this line
            if (RotationRequested != null)
            {
                RotationRequested.Invoke(this, newAngle);
            }
            UpdateDataModel();
        }

        private void EndRotation()
        {
            CommitUndoAction();
            foreach (var item in SelectionService.SelectedItems)
            {
                item.ApplyEndRotation();
            }
        }

        internal void ApplyEndRotation()
        {
            if (RotationCompleted != null)
            {
                RotationCompleted.Invoke(this, this.RotationAngle);
            }
            ExecuteAttachedCommand(TransformHelper.RotationCompletedCommandProperty, DataModel);
        }
      

        private bool HandleDirectionButtonClick(Point localPoint)
        {
            if (!IsSingleSelect)
            {
                return false;
            }
            var directionHit = TransformVisual.GetDirectionButtonOperation(localPoint, VisualPixelSize, RectDirection);
            if (directionHit != VisualOperation.None)
            {
                switch (directionHit)
                {
                    case VisualOperation.ChangeDirectionTop:
                        this.RectDirection = RectDirection.Top;
                        break;
                    case VisualOperation.ChangeDirectionBottom:
                        this.RectDirection = RectDirection.Bottom;
                        break;
                    case VisualOperation.ChangeDirectionLeft:
                        this.RectDirection = RectDirection.Left;
                        break;
                    case VisualOperation.ChangeDirectionRight:
                        this.RectDirection = RectDirection.Right;
                        break;
                }
                UpdateVisual();
                UpdateDataModel();
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取给定点对应的 VisualOperation 测试结果。
        /// </summary>
        private VisualOperation GetVisualOperation(Point p, bool forceHidePivot = false)
        {
            double px = DataModel != null ? DataModel.PivotX : 0.5;
            double py = DataModel != null ? DataModel.PivotY : 0.5;
            bool showPivot = _isPivotVisible && !forceHidePivot;
            return TransformVisual.GetVisualOperation(p, VisualPixelSize, RectDirection, IsDirectionEnabled, IsRotationEnabled, px, py, showPivot);
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
            var op = GetVisualOperation(pt);
            if (op != VisualOperation.None)
            {
                return new PointHitTestResult(this, hitTestParameters.HitPoint);
            }
            return null;
        }

    
        private Cursor GetCursorForOperation(VisualOperation op)
        {
            if (!IsSelect)
            {
                return Cursors.Arrow;
            }

            switch (op)
            {
                case VisualOperation.Move:
                    return Cursors.SizeAll;

                case VisualOperation.ResizeTop:
                case VisualOperation.ResizeBottom:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle);

                case VisualOperation.ResizeLeft:
                case VisualOperation.ResizeRight:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle + 90);

                case VisualOperation.ResizeTopLeft:
                case VisualOperation.ResizeBottomRight:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle - 45);

                case VisualOperation.ResizeTopRight:
                case VisualOperation.ResizeBottomLeft:
                    return GetRotatedCursor(BaseResizeCursorData, RotationAngle + 45);

                case VisualOperation.Rotate:
                    switch (RectDirection)
                    {
                        case RectDirection.Top: return GetRotatedCursor(BaseRotationCursorData, RotationAngle - 45);
                        case RectDirection.Right: return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 45);
                        case RectDirection.Bottom: return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 135);
                        case RectDirection.Left: return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 225);
                        default: return GetRotatedCursor(BaseRotationCursorData, RotationAngle - 45);
                    }

                case VisualOperation.RotateTopLeft:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle - 90);

                case VisualOperation.RotateTopRight:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle);

                case VisualOperation.RotateBottomRight:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 90);

                case VisualOperation.RotateBottomLeft:
                    return GetRotatedCursor(BaseRotationCursorData, RotationAngle + 180);

                case VisualOperation.MovePivot:
                    return Cursors.Cross;

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
            if (DataModel == null || AdornedFE == null || IsInternalSync)
            {
                return;
            }

            IsInternalSync = true;
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

            UpdatePivotFeedback();
            }
            finally
            {
                IsInternalSync = false;
            }
        }

        private void UpdatePivotFeedback()
        {
            if (DataModel == null || AdornedFE == null)
            {
                return;
            }

            var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
            if (parent != null)
            {
                double px = DataModel.PivotX;
                double py = DataModel.PivotY;
                DataModel.Pivot = AdornedFE.TranslatePoint(new Point(AdornedFE.ActualWidth * px, AdornedFE.ActualHeight * py), parent);
            }
        }

        public void SyncModelToElement()
        {
            if (AdornedFE == null || DataModel == null || IsInternalSync) 
            {
                return;
            }

            IsInternalSync = true;
            try
            {
                var parent = VisualTreeHelper.GetParent(AdornedFE) as UIElement;
                if (parent == null) 
                {
                    return;
                }

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

                if (DataModel.Width > 0) 
                {
                    AdornedFE.Width = DataModel.Width;
                }
                if (DataModel.Height > 0) 
                {
                    AdornedFE.Height = DataModel.Height;
                }

                RotationAngle = DataModel.Angle;
                TransformHelper.SetRotation(AdornedFE, DataModel.Angle);
                RectDirection = DataModel.Direction;
                
                // 确保同步旋转中心
                TransformHelper.UpdateRenderTransform(AdornedFE);
                UpdateVisual();
            }
            finally
            {
                IsInternalSync = false;
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

            bool showRotationCenter = (CurrentOperation == VisualOperation.Rotate || 
                                     CurrentOperation == VisualOperation.RotateTopLeft || 
                                     CurrentOperation == VisualOperation.RotateTopRight || 
                                     CurrentOperation == VisualOperation.RotateBottomLeft || 
                                     CurrentOperation == VisualOperation.RotateBottomRight ||
                                     CurrentOperation == VisualOperation.MovePivot);

            double px = DataModel != null ? DataModel.PivotX : 0.5;
            double py = DataModel != null ? DataModel.PivotY : 0.5;

            TransformVisual.Render(VisualPixelSize, BorderBrush, IsSelect, IsSingleSelect, RectDirection, IsDirectionEnabled, HoveredDirectionButton, _isPivotVisible, px, py);
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
                
                // 修复 bug: 不能使用 TransformBounds，因为它返回的是轴对齐包围盒，
                // 在旋转时会导致裁剪区域比实际大。应该直接变换几何体。
                var geometry = new RectangleGeometry(clipRect);
                geometry.Transform = toAdorner as Transform;
                return geometry;
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

            // 将 DrawingVisual 定位在放大后的 Adorner 的内部 (HitPadding, HitPadding)处，
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
            switch (direction)
            {
                case ResizeGripDirection.TopLeft:
                    return new Point(width, height);
                case ResizeGripDirection.Top:
                    return new Point(width / 2, height);
                case ResizeGripDirection.TopRight:
                    return new Point(0, height);
                case ResizeGripDirection.Left:
                    return new Point(width, height / 2);
                case ResizeGripDirection.Right:
                    return new Point(0, height / 2);
                case ResizeGripDirection.BottomLeft:
                    return new Point(width, 0);
                case ResizeGripDirection.Bottom:
                    return new Point(width / 2, 0);
                case ResizeGripDirection.BottomRight:
                    return new Point(0, 0);
                default:
                    return new Point();
            }
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
            if (currentUndoAction == null) 
            {
                return;
            }
            foreach (var item in SelectionService.SelectedItems)
            {
                var memento = TransformMemento.Capture(item.DataModel);
                TransformData target = item.DataModel;
                
                (TransformData target, TransformMemento before, TransformMemento after) entry = default;
                foreach (var c in currentUndoAction.Changes)
                {
                    if (c.target == target)
                    {
                        entry = c;
                        break;
                    }
                }
                
                if (entry != default && memento != null)
                {
                    var index = currentUndoAction.Changes.IndexOf(entry);
                    currentUndoAction.Changes[index] = (entry.target, entry.before, memento);
                }
            }
            UndoService.AddAction(currentUndoAction);
            currentUndoAction = null;
        }

        private static void SortAdornersByStableIndex(List<RSTransformAdorner> hitList)
        {
            hitList.Sort(CompareAdorners);
        }

        private static int CompareAdorners(RSTransformAdorner a, RSTransformAdorner b)
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

            // 首先按 ZIndex 排序（如果有）
            int zA = Panel.GetZIndex(elA);
            int zB = Panel.GetZIndex(elB);
            if (zA != zB)
            {
                return zA.CompareTo(zB);
            }

            int idxA = pA.Children.IndexOf(elA);
            int idxB = pB.Children.IndexOf(elB);
            return idxA.CompareTo(idxB);
        }

        private void ExecuteAttachedCommand(DependencyProperty commandProperty, object parameter)
        {
            if (AdornedElement == null)
            {
                return;
            }
            var command = AdornedElement.GetValue(commandProperty) as ICommand;
            if (command != null)
            {
                if (command.CanExecute(parameter))
                {
                    command.Execute(parameter);
                }
            }
        }
        #endregion

    }
}
