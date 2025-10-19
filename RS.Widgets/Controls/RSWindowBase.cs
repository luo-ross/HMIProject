using RS.Commons.Helper;
using RS.Win32API;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;


namespace RS.Widgets.Controls
{
    public class RSWindowBase : Window
    {
        public static string SystemCloseDes { get; private set; }
        public static string SystemMinimizeDes { get; private set; }
        public static string SystemMaximizeDes { get; private set; }
        public static string SystemRestoreDes { get; private set; }

        public HwndSource HwndSource;

        static RSWindowBase()
        {
            InitSystemDefaultDes();
        }

        public RSWindowBase()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow, CanCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeRestoreWindow, CanMaximizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, MaximizeRestoreWindow, CanRestoreWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu, CanShowSystemMenu));
            // 添加命令绑定
            this.CommandBindings.Add(new CommandBinding(RSCommands.CleanTextCommand, CleanTextText));

            this.StateChanged += RSWindow_StateChanged;
            this.Loaded += RSWindowBase_Loaded;
            this.Closing += RSWindow_Closing;
        }
       
        private  void CleanTextText(object sender, ExecutedRoutedEventArgs e)
        {
            RSCommands.CleanText(e.Parameter);
        }

        private void CanShowSystemMenu(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.ShowSystemMenu(this, this.PointToScreen(Mouse.GetPosition(this)));
        }

        private void CanRestoreWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WindowState == WindowState.Maximized;
        }

        private void CanMaximizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WindowState == WindowState.Normal;
        }

        private void MaximizeRestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CanCloseWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }



        public  void UpdateWindowChrome()
        {
            WindowChrome.SetWindowChrome(this, null);
            var chrome = new WindowChrome
            {
                CaptionHeight = this.CaptionHeight + this.BlurRadius,
                ResizeBorderThickness = new Thickness(0),
                GlassFrameThickness = new Thickness(-1),
                CornerRadius = new CornerRadius(0, 0, 0, 0),
                UseAeroCaptionButtons = false,
            };
            WindowChrome.SetWindowChrome(this, chrome);
        }

        public string WINDOWPLACEMENTConfigKey
        {
            get { return (string)GetValue(WINDOWPLACEMENTConfigKeyProperty); }
            set { SetValue(WINDOWPLACEMENTConfigKeyProperty, value); }
        }

        public static readonly DependencyProperty WINDOWPLACEMENTConfigKeyProperty =
            DependencyProperty.Register("WINDOWPLACEMENTConfigKey", typeof(string), typeof(RSWindowBase), new PropertyMetadata(null));



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HwndSource = (HwndSource)PresentationSource.FromDependencyObject(this);
            if (!string.IsNullOrEmpty(this.WINDOWPLACEMENTConfigKey))
            {
                var WINDOWPLACEMENT = ConfigHelpler.GetDefaultConfig(WINDOWPLACEMENTConfigKey, new WINDOWPLACEMENT());
                NativeMethods.SetWindowPlacement(new HandleRef(null, this.HwndSource.Handle), ref WINDOWPLACEMENT);
            }
        }



        public bool IsShowIcon
        {
            get { return (bool)GetValue(IsShowIconProperty); }
            set { SetValue(IsShowIconProperty, value); }
        }

        public static readonly DependencyProperty IsShowIconProperty =
            DependencyProperty.Register("IsShowIcon", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));


        public bool IsShowMidCaptionContent
        {
            get { return (bool)GetValue(IsShowMidCaptionContentProperty); }
            set { SetValue(IsShowMidCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty IsShowMidCaptionContentProperty =
            DependencyProperty.Register("IsShowMidCaptionContent", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));


        public bool IsShowLeftCaptionContent
        {
            get { return (bool)GetValue(IsShowLeftCaptionContentProperty); }
            set { SetValue(IsShowLeftCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty IsShowLeftCaptionContentProperty =
            DependencyProperty.Register("IsShowLeftCaptionContent", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));



        [Description("是否沉浸式")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public bool IsFitSystem
        {
            get { return (bool)GetValue(IsFitSystemProperty); }
            set { SetValue(IsFitSystemProperty, value); }
        }

        public static readonly DependencyProperty IsFitSystemProperty =
            DependencyProperty.Register("IsFitSystem", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(false));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsMaxsizedFullScreenProperty)
            {
                if (this.IsMaxsizedFullScreen)
                {
                    SystemCommands.MaximizeWindow(this);
                }
                else
                {
                    SystemCommands.RestoreWindow(this);
                }
            }
        }



        [Description("是否显示窗体关闭放大缩小按钮")]
        [Browsable(true)]
        public bool IsShowWinBtnCommands
        {
            get { return (bool)GetValue(IsShowWinBtnCommandsProperty); }
            set { SetValue(IsShowWinBtnCommandsProperty, value); }
        }

        public static readonly DependencyProperty IsShowWinBtnCommandsProperty =
            DependencyProperty.Register("IsShowWinBtnCommands", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));




        public bool IsShowCaption
        {
            get { return (bool)GetValue(IsShowCaptionProperty); }
            set { SetValue(IsShowCaptionProperty, value); }
        }

        public static readonly DependencyProperty IsShowCaptionProperty =
            DependencyProperty.Register("IsShowCaption", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));





        [Description("是否显示标题")]
        public bool IsShowTitle
        {
            get { return (bool)GetValue(IsShowTitleProperty); }
            set { SetValue(IsShowTitleProperty, value); }
        }

        public static readonly DependencyProperty IsShowTitleProperty =
            DependencyProperty.Register("IsShowTitle", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));




        [Description("自定义中部标题栏内容")]
        [Browsable(false)]
        public object MidCaptionContent
        {
            get { return (object)GetValue(MidCaptionContentProperty); }
            set { SetValue(MidCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty MidCaptionContentProperty =
            DependencyProperty.Register("MidCaptionContent", typeof(object), typeof(RSWindowBase), new PropertyMetadata(null));


        [Description("自定义左侧标题栏内容")]
        [Browsable(false)]
        public object LeftCaptionContent
        {
            get { return (object)GetValue(LeftCaptionContentProperty); }
            set { SetValue(LeftCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty LeftCaptionContentProperty =
            DependencyProperty.Register("LeftCaptionContent", typeof(object), typeof(RSWindowBase), new PropertyMetadata(null));



        [Description("标题栏高度设置")]
        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }

        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(RSWindowBase),
                new PropertyMetadata(32D, OnCaptionHeightPropertyChanged));



        [Description("窗体阴影半径")]
        public double BlurRadius
        {
            get { return (double)GetValue(BlurRadiusProperty); }
            set { SetValue(BlurRadiusProperty, value); }
        }

        public static readonly DependencyProperty BlurRadiusProperty =
            DependencyProperty.Register("BlurRadius", typeof(double), typeof(RSWindowBase),
                new PropertyMetadata(15D, OnCaptionHeightPropertyChanged));

        private static void OnCaptionHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsWindowBase = d as RSWindowBase;
            rsWindowBase.UpdateWindowChrome();
        }


        [Description("窗口最大化时是否全屏")]
        [Browsable(false)]
        public bool IsMaxsizedFullScreen
        {
            get { return (bool)GetValue(IsMaxsizedFullScreenProperty); }
            set { SetValue(IsMaxsizedFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsMaxsizedFullScreenProperty =
            DependencyProperty.Register("IsMaxsizedFullScreen", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(false));


        [Description("圆角边框大小")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public CornerRadius CornerRadius
        {
            get { return (CornerRadius)GetValue(CornerRadiusProperty); }
            set { SetValue(CornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty CornerRadiusProperty =
            DependencyProperty.Register("CornerRadius", typeof(CornerRadius), typeof(RSWindowBase), new PropertyMetadata(null));



        [Description("是否使用圆角")]
        public bool IsUseCornerRadius
        {
            get { return (bool)GetValue(IsUseCornerRadiusProperty); }
            set { SetValue(IsUseCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty IsUseCornerRadiusProperty =
            DependencyProperty.Register("IsUseCornerRadius", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(false));




        public Brush ActiveCaptionBackground
        {
            get { return (Brush)GetValue(ActiveCaptionBackgroundProperty); }
            set { SetValue(ActiveCaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty ActiveCaptionBackgroundProperty =
            DependencyProperty.Register("ActiveCaptionBackground", typeof(Brush), typeof(RSWindowBase), new PropertyMetadata(default));


        public Brush NotActiveCaptionBackground
        {
            get { return (Brush)GetValue(NotActiveCaptionBackgroundProperty); }
            set { SetValue(NotActiveCaptionBackgroundProperty, value); }
        }

        public static readonly DependencyProperty NotActiveCaptionBackgroundProperty =
            DependencyProperty.Register("NotActiveCaptionBackground", typeof(Brush), typeof(RSWindowBase), new PropertyMetadata(default));


        #region Icon参数设置

        public CornerRadius IconCornerRadius
        {
            get { return (CornerRadius)GetValue(IconCornerRadiusProperty); }
            set { SetValue(IconCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty IconCornerRadiusProperty =
            DependencyProperty.Register("IconCornerRadius", typeof(CornerRadius), typeof(RSWindowBase), new PropertyMetadata(new CornerRadius(10)));


        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(RSWindowBase), new PropertyMetadata(20D));


        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(RSWindowBase), new PropertyMetadata(20D));


        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }

        public static readonly DependencyProperty IconMarginProperty =
            DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(RSWindowBase), new PropertyMetadata(new Thickness(5, 0, 0, 0)));

        #endregion


        private void RSWindow_Closing(object? sender, CancelEventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(this.WINDOWPLACEMENTConfigKey))
            {
                WINDOWPLACEMENT wINDOWPLACEMENT = new WINDOWPLACEMENT();
                NativeMethods.GetWindowPlacement(new HandleRef(null, this.HwndSource.Handle), ref wINDOWPLACEMENT);
                ConfigHelpler.SaveAppConfigAsync(this.WINDOWPLACEMENTConfigKey, wINDOWPLACEMENT);
            }

            ApplicationBase.RSWinInfoBar?.Close();

        }

        private void RSWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.RefreshWindowSizeAndLocation();
            }
        }


        private void RSWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.RefreshWindowSizeAndLocation();
            }
        }

        private void RefreshWindowSizeAndLocation()
        {

            // 使用 WindowInteropHelper 获取窗口句柄
            var hWnd = new WindowInteropHelper(this).Handle;
            int nWidth = IsMaxsizedFullScreen ? (int)SystemParameters.PrimaryScreenWidth : (int)SystemParameters.WorkArea.Width;  // 新的宽度
            int nHeight = IsMaxsizedFullScreen ? (int)SystemParameters.PrimaryScreenHeight : (int)SystemParameters.WorkArea.Height; // 新的高度
            //NativeMethods.SetWindowPos(new HandleRef(null, hWnd), new HandleRef(null, IntPtr.Zero), 0, 0, nWidth, nHeight, (int)(SWP.NOZORDER | SWP.NOACTIVATE));
            NativeMethods.SetWindowPos(new HandleRef(null, hWnd), new HandleRef(null, IntPtr.Zero), 0, 0, nWidth, nHeight, 0);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.UpdateWindowChrome();
        }

        private static void InitSystemDefaultDes()
        {
            IntPtr hInstance = NativeMethods.LoadLibrary(ExternDll.User32);
            if (hInstance == IntPtr.Zero)
            {
                return;
            }
            #region 获取窗体关闭缩小最大化的本地化描述
            SystemCloseDes = GetSystemDefaultDes(hInstance, NativeMethods.IDS_CLOSE);
            SystemMinimizeDes = GetSystemDefaultDes(hInstance, NativeMethods.IDS_MINIMIZE);
            SystemMaximizeDes = GetSystemDefaultDes(hInstance, NativeMethods.IDS_MAXIMIZE);
            SystemRestoreDes = GetSystemDefaultDes(hInstance, NativeMethods.IDS_RESTORE_DOWN);
            #endregion
        }

        private static string GetSystemDefaultDes(IntPtr hInstance, uint resourceId)
        {
            StringBuilder sb = new StringBuilder(256);
            int length = NativeMethods.LoadString(hInstance, resourceId, sb, sb.Capacity);
            return length > 0 ? sb.ToString() : null;
        }
      
    }
}
