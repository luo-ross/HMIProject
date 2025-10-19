using CommunityToolkit.Mvvm.Input;
using NPOI.SS.Formula.Functions;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.Win32API;
using RS.Win32API.Structs;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSDockPanel : ContentControl
    {
        public static HashSet<RSDockPanel> RSDockPanelList { get; private set; }
        /// <summary>
        /// 标题栏
        /// </summary>
        private Grid PART_Caption;

        /// <summary>
        /// 是否可以进行拖拽
        /// </summary>
        public bool IsCanDragPanel { get; set; }

        /// <summary>
        /// 记录Panel绑定的用户视图
        /// </summary>
        public FrameworkElement UserView { get; set; }

        /// <summary>
        /// 记录GridSpliter
        /// </summary>
        public GridSplitter GridSplitter { get; set; }

        /// <summary>
        /// 记录所在行
        /// </summary>
        public int GridRow { get; private set; }

        /// <summary>
        /// 记录所在列
        /// </summary>
        public int GridColumn { get; private set; }

        /// <summary>
        /// 记录合并行数
        /// </summary>
        public int GridRowSpan { get; private set; }

        /// <summary>
        /// 记录合并列数
        /// </summary>
        public int GridColumnSpan { get; private set; }

        /// <summary>
        /// 记录Panel在屏幕上的位置
        /// </summary>
        public Rect ScreenRect { get; private set; }

        /// <summary>
        /// 窗体名称
        /// </summary>
        public string WindowName { get; private set; }

        /// <summary>
        /// 窗体句柄
        /// </summary>
        public nint ParentWindowHandle { get; private set; }

        /// <summary>
        /// 记录该容器在哪个DockGrid
        /// </summary>
        public RSDockGrid DockGrid { get; private set; }

        static RSDockPanel()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDockPanel), new FrameworkPropertyMetadata(typeof(RSDockPanel)));
            RSDockPanelList = new HashSet<RSDockPanel>();
        }

        public RSDockPanel()
        {
            this.PreviewMouseDown += RSDockPanel_PreviewMouseDown;
            this.SetValue(WindowFloatingCommandPropertyKey, new RelayCommand<object>(OnWindowFloatingExecuted, OnWindowFloatingCanExecuted));
            this.SetValue(WindowDockingCommandPropertyKey, new RelayCommand<object>(OnWindowDockingExecuted, OnWindowDockingCanExecuted));
            this.SetValue(WindowTabDockingCommandPropertyKey, new RelayCommand<object>(OnWindowTabDockingExecuted, OnWindowTabDockingCanExecuted));
            this.SetValue(WindowAutoHiddenCommandPropertyKey, new RelayCommand<object>(OnindowAutoHiddenExecuted, OnindowAutoHiddenCanExecuted));
            this.SetValue(CloseWindowCommandPropertyKey, new RelayCommand<object>(OnCloseWindowExecuted, OnCloseWindowCanExecuted));
            this.SetValue(MaximizeWindowCommandPropertyKey, new RelayCommand<object>(OnMaximizeWindowExecuted, OnMaximizeWindowCanExecuted));
            this.SetValue(RestoreWindowCommandPropertyKey, new RelayCommand<object>(OnMaximizeWindowExecuted, OnRestoreWindowCanExecuted));
            this.Loaded += RSDockPanel_Loaded;
            this.SizeChanged += RSDockPanel_SizeChanged;
            this.Unloaded += RSDockPanel_Unloaded;
            AddRSDockPanel(this);
        }


        public static void RemoveRSDockPanel(RSDockPanel dockPanel)
        {
            if (RSDockPanelList.Contains(dockPanel))
            {
                RSDockPanelList.Remove(dockPanel);
            }
        }

        public static void AddRSDockPanel(RSDockPanel dockPanel)
        {
            RSDockPanelList.Add(dockPanel);
        }

        public static List<RSDockPanel> GetDockPanelListUnderCursorPos()
        {
            var cursorPos = NativeMethods.GetCursorPos();
            return GetDockPanelList(cursorPos);
        }


        public static List<RSDockPanel> GetDockPanelList(POINT cursorPos)
        {
           
            return RSDockPanelList.Where(t => t.ScreenRect.X <= cursorPos.X
            && t.ScreenRect.Y <= cursorPos.Y
            && t.ScreenRect.Right >= cursorPos.X
            && t.ScreenRect.Bottom >= cursorPos.Y).ToList();

        }


        private void RSDockPanel_Unloaded(object sender, RoutedEventArgs e)
        {
            this.UnRegisterWindowEvents(this.ParentWindow);
        }

        private void RSDockPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.RefreshScreenRect();
        }

        private void RSDockPanel_Loaded(object sender, RoutedEventArgs e)
        {
            //获取父窗体
            this.ParentWindow = this.TryFindParent<Window>();
            //记录窗口
            if (this.ParentWindow != null && this.IsWindowDockPanel)
            {
                this.WindowName = this.ParentWindow.Name;
                if (string.IsNullOrEmpty(this.WindowName))
                {
                    this.WindowName = $"Window_{Guid.NewGuid().ToString("N")}";
                    this.ParentWindow.Name = this.WindowName;
                }
            }
            //获取父容器DockGrid
            this.DockGrid = this.TryFindParent<RSDockGrid>();

            this.RegisterWindowEvents(this.ParentWindow);
            this.RefreshScreenRect();
            this.RefreshGridPositionInfo();

        }

        /// <summary>
        /// 更新Panel在屏幕上的位置信息 
        /// 屏幕移动的时候需要刷新
        /// 自身尺寸发生改变的时候需要刷新
        /// </summary>
        private void RefreshScreenRect()
        {
            if (PresentationSource.FromVisual(this) == null)
            {
                return;
            }

            var screenPoint = this.PointToScreen(new Point(0, 0));
            this.ScreenRect = new Rect(screenPoint.X,
                                                 screenPoint.Y,
                                                 this.ActualWidth,
                                                 this.ActualHeight);

        }

        /// <summary>
        /// 获取到控件所在行列信息
        /// </summary>
        private void RefreshGridPositionInfo()
        {
            this.GridRow = Grid.GetRow(this);
            this.GridColumn = Grid.GetColumn(this);
            this.GridRowSpan = Grid.GetRowSpan(this);
            this.GridColumnSpan = Grid.GetColumnSpan(this);
        }


        private bool OnRestoreWindowCanExecuted(object? obj)
        {
            return ParentWindow != null && ParentWindow.WindowState == WindowState.Maximized;
        }

        private bool OnMaximizeWindowCanExecuted(object? obj)
        {
            return ParentWindow != null && ParentWindow.WindowState == WindowState.Normal;
        }

        private void OnMaximizeWindowExecuted(object? obj)
        {
            if (this.ParentWindow == null)
            {
                return;
            }
            if (this.ParentWindow.WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this.ParentWindow);
            }
            else
            {
                SystemCommands.MaximizeWindow(this.ParentWindow);
            }
        }

        private bool OnCloseWindowCanExecuted(object? obj)
        {
            return true;
        }

        private void OnCloseWindowExecuted(object? obj)
        {
            if (this.ParentWindow != null && this.IsWindowDockPanel)
            {
                //如果是窗体 则处理窗体的业务逻辑
                SystemCommands.CloseWindow(this.ParentWindow);
            }
            else
            {
                //从父容器Grid里移除掉这个Panel
                this.DockGrid?.RemoveDockPanel(this);
            }

            this.ClosePopup(obj);
        }

        private bool OnindowAutoHiddenCanExecuted(object? obj)
        {
            return true;
        }

        private void OnindowAutoHiddenExecuted(object? obj)
        {
            this.ClosePopup(obj);
        }

        private bool OnWindowTabDockingCanExecuted(object? obj)
        {
            return true;
        }

        private void OnWindowTabDockingExecuted(object? obj)
        {
            this.ClosePopup(obj);
        }

        private bool OnWindowDockingCanExecuted(object? obj)
        {
            return true;
        }

        private void OnWindowDockingExecuted(object? obj)
        {
            this.ClosePopup(obj);
        }

        private bool OnWindowFloatingCanExecuted(object? obj)
        {
            return true;
        }

        private void OnWindowFloatingExecuted(object? obj)
        {
            this.ClosePopup(obj);
        }

        private void ClosePopup(object? obj)
        {
            if (obj is Popup popup)
            {
                popup.IsOpen = false;
            }
        }

        private void RSDockPanel_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            this.ActiveRSDockPanel();
        }


        public void ShowCaptionContextMenu()
        {
            this.IsCaptionContextMenuOpen = true;
        }


        public void ActiveRSDockPanel()
        {
            ClearActiveStaus();
            this.IsActive = true;
        }

        private void ClearActiveStaus()
        {
            foreach (var item in RSDockPanelList)
            {
                item.IsActive = false;
            }
        }



        private static readonly DependencyPropertyKey WindowFloatingCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(WindowFloatingCommand),
                typeof(ICommand),
                typeof(RSDockPanel),
                new PropertyMetadata(null));
        public static readonly DependencyProperty WindowFloatingCommandProperty = WindowFloatingCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 浮动
        /// </summary>
        public ICommand WindowFloatingCommand
        {
            get { return (ICommand)GetValue(WindowFloatingCommandProperty); }
        }

        private static readonly DependencyPropertyKey WindowDockingCommandPropertyKey =
         DependencyProperty.RegisterReadOnly(nameof(WindowDockingCommand),
             typeof(ICommand),
             typeof(RSDockPanel),
             new PropertyMetadata(null));
        public static readonly DependencyProperty WindowDockingCommandProperty = WindowDockingCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 停靠
        /// </summary>
        public ICommand WindowDockingCommand
        {
            get { return (ICommand)GetValue(WindowDockingCommandProperty); }
        }

        private static readonly DependencyPropertyKey WindowTabDockingCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(WindowTabDockingCommand),
          typeof(ICommand),
          typeof(RSDockPanel),
          new PropertyMetadata(null));
        public static readonly DependencyProperty WindowTabDockingCommandProperty = WindowTabDockingCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 作为选项式文档停靠
        /// </summary>
        public ICommand WindowTabDockingCommand
        {
            get { return (ICommand)GetValue(WindowTabDockingCommandProperty); }
        }

        private static readonly DependencyPropertyKey WindowAutoHiddenCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(WindowAutoHiddenCommand)
            , typeof(ICommand),
            typeof(RSDockPanel),
            new PropertyMetadata(null));
        public static readonly DependencyProperty WindowAutoHiddenCommandProperty = WindowAutoHiddenCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 自动隐藏
        /// </summary>
        public ICommand WindowAutoHiddenCommand
        {
            get { return (ICommand)GetValue(WindowAutoHiddenCommandProperty); }
        }

        private static readonly DependencyPropertyKey CloseWindowCommandPropertyKey =
        DependencyProperty.RegisterReadOnly(nameof(CloseWindowCommand),
            typeof(ICommand),
            typeof(RSDockPanel),
            new PropertyMetadata(null));
        public static readonly DependencyProperty CloseWindowCommandProperty = CloseWindowCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 关闭
        /// </summary>
        public ICommand CloseWindowCommand
        {
            get { return (ICommand)GetValue(CloseWindowCommandProperty); }
        }



        private static readonly DependencyPropertyKey MaximizeWindowCommandPropertyKey =
      DependencyProperty.RegisterReadOnly(nameof(MaximizeWindowCommand),
          typeof(ICommand),
          typeof(RSDockPanel),
          new PropertyMetadata(null));
        public static readonly DependencyProperty MaximizeWindowCommandProperty = MaximizeWindowCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 窗体最大化
        /// </summary>
        public ICommand MaximizeWindowCommand
        {
            get { return (ICommand)GetValue(MaximizeWindowCommandProperty); }
        }


        private static readonly DependencyPropertyKey RestoreWindowCommandPropertyKey =
    DependencyProperty.RegisterReadOnly(nameof(RestoreWindowCommand),
        typeof(ICommand),
        typeof(RSDockPanel),
        new PropertyMetadata(null));
        public static readonly DependencyProperty RestoreWindowCommandProperty = RestoreWindowCommandPropertyKey.DependencyProperty;

        /// <summary>
        /// 窗体状态恢复
        /// </summary>
        public ICommand RestoreWindowCommand
        {
            get { return (ICommand)GetValue(RestoreWindowCommandProperty); }
        }

        public object MidCaptionContent
        {
            get { return (object)GetValue(MidCaptionContentProperty); }
            set { SetValue(MidCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty MidCaptionContentProperty =
            DependencyProperty.Register("MidCaptionContent",
                typeof(object),
                typeof(RSDockPanel),
                new PropertyMetadata(null));

        public object LeftCaptionContent
        {
            get { return (object)GetValue(LeftCaptionContentProperty); }
            set { SetValue(LeftCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty LeftCaptionContentProperty =
            DependencyProperty.Register("LeftCaptionContent",
                typeof(object),
                typeof(RSDockPanel),
                new PropertyMetadata(null));

        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }

        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight",
                typeof(double),
                typeof(RSDockPanel),
                new PropertyMetadata(25D));

        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
         DependencyProperty.Register("Title",
                typeof(string),
                typeof(RSDockPanel),
                new PropertyMetadata(null));


        public Brush ActiveCaptionBackground
        {
            get { return (Brush)GetValue(ActiveCaptionBackgroundProperty); }
            set { SetValue(ActiveCaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ActiveCaptionBackgroundProperty =
        DependencyProperty.Register("ActiveCaptionBackground",
                typeof(Brush),
                typeof(RSDockPanel),
                new PropertyMetadata(default));


        public Brush NotActiveCaptionBackground
        {
            get { return (Brush)GetValue(NotActiveCaptionBackgroundProperty); }
            set { SetValue(NotActiveCaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty NotActiveCaptionBackgroundProperty =
        DependencyProperty.Register("NotActiveCaptionBackground",
                typeof(Brush),
                typeof(RSDockPanel),
                new PropertyMetadata(default));

        public bool IsActive
        {
            get { return (bool)GetValue(IsActiveProperty); }
            set { SetValue(IsActiveProperty, value); }
        }

        public static readonly DependencyProperty IsActiveProperty =
        DependencyProperty.Register("IsActive",
                typeof(bool),
                typeof(RSDockPanel),
                new PropertyMetadata(false));

        public bool IsAutoHidden
        {
            get { return (bool)GetValue(IsAutoHiddenProperty); }
            set { SetValue(IsAutoHiddenProperty, value); }
        }

        public static readonly DependencyProperty IsAutoHiddenProperty =
        DependencyProperty.Register("IsAutoHidden",
            typeof(bool),
            typeof(RSDockPanel),
            new PropertyMetadata(false));

        /// <summary>
        /// 记录是否是窗体级别的面板
        /// 用在拖拽的时候 会将这个面板放在窗体里
        /// </summary>
        public bool IsWindowDockPanel
        {
            get { return (bool)GetValue(IsWindowDockPanelProperty); }
            set { SetValue(IsWindowDockPanelProperty, value); }
        }

        public static readonly DependencyProperty IsWindowDockPanelProperty =
            DependencyProperty.Register("IsWindowDockPanel",
                typeof(bool),
                typeof(RSDockPanel),
                new PropertyMetadata(false));


        /// <summary>
        /// 父窗体
        /// </summary>
        public Window ParentWindow
        {
            get { return (Window)GetValue(ParentWindowProperty); }
            set { SetValue(ParentWindowProperty, value); }
        }

        public static readonly DependencyProperty ParentWindowProperty =
            DependencyProperty.Register("ParentWindow",
                typeof(Window),
                typeof(RSDockPanel),
                new PropertyMetadata(null, OnParentWindowPropertyChanged));

        private static void OnParentWindowPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dockPanel = (RSDockPanel)d;
            if (e.OldValue is Window oldWindow)
            {
                dockPanel?.UnRegisterWindowEvents(oldWindow);
            }
            if (e.NewValue is Window newWindow)
            {
                //获取窗口句柄
                dockPanel.ParentWindowHandle = new WindowInteropHelper(newWindow).Handle;
            }
        }

        private void RegisterWindowEvents(Window window)
        {
            if (window == null)
            {
                return;
            }
            window.LocationChanged -= ParentWindow_LocationChanged;
            window.Closed -= Window_Closed;
            window.LocationChanged += ParentWindow_LocationChanged;
            window.Closed += Window_Closed;
        }


        private void Window_Closed(object? sender, EventArgs e)
        {
            this.IsWindowDockPanel = false;
            RemoveRSDockPanel(this);
        }

        private void UnRegisterWindowEvents(Window window)
        {
            if (window == null)
            {
                return;
            }
            window.LocationChanged -= ParentWindow_LocationChanged;
            window.Closed -= Window_Closed;
        }

        private void ParentWindow_LocationChanged(object? sender, EventArgs e)
        {
            this.RefreshScreenRect();
        }


        public bool IsCaptionContextMenuOpen
        {
            get { return (bool)GetValue(IsCaptionContextMenuOpenProperty); }
            set { SetValue(IsCaptionContextMenuOpenProperty, value); }
        }


        public static readonly DependencyProperty IsCaptionContextMenuOpenProperty =
            DependencyProperty.Register("IsCaptionContextMenuOpen",
                typeof(bool),
                typeof(RSDockPanel),
                new PropertyMetadata(false));

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Caption = this.GetTemplateChild(nameof(this.PART_Caption)) as Grid;
            if (this.PART_Caption != null)
            {
                this.PART_Caption.MouseEnter += PART_Caption_MouseEnter;
                this.PART_Caption.MouseLeave += PART_Caption_MouseLeave;
                this.PART_Caption.PreviewMouseMove += PART_Caption_PreviewMouseMove;
            }
        }

        private void PART_Caption_PreviewMouseMove(object sender, MouseEventArgs e)
        {
            var frameworkElement = sender as FrameworkElement;
            var mousePositionRelative = e.GetPosition(frameworkElement);
            if (mousePositionRelative.X >= 0
                && mousePositionRelative.Y >= 0
                && mousePositionRelative.X <= frameworkElement.ActualWidth
                && mousePositionRelative.Y <= frameworkElement.ActualHeight)
            {
                this.IsCanDragPanel = true;
            }
            else
            {
                this.IsCanDragPanel = false;
            }
        }

        private void PART_Caption_MouseEnter(object sender, MouseEventArgs e)
        {
            this.IsCanDragPanel = true;
        }

        private void PART_Caption_MouseLeave(object sender, MouseEventArgs e)
        {
            this.IsCanDragPanel = false;
        }


    }
}
