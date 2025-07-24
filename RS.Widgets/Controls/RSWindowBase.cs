using RS.Commons.Helper;
using RS.Widgets.Enums;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Shell;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;


namespace RS.Widgets.Controls
{
    public class RSWindowBase : Window
    {
        
        public HwndSource HwndSource;

        public RSWindowBase()
        {
            #region 这里样式必须在构造函数配置
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            #endregion
            this.StateChanged += RSWindow_StateChanged;
            this.Loaded += RSWindowBase_Loaded;
            this.Closing += RSWindow_Closing;

            #region 获取窗体关闭缩小最大化的本地化描述
            this.CloseDes = GetSystemString(ExternDll.User32, NativeMethods.IDS_CLOSE);
            this.MinimizeDes = GetSystemString(ExternDll.User32, NativeMethods.IDS_MINIMIZE);
            this.MaximizeDes = GetSystemString(ExternDll.User32, NativeMethods.IDS_MAXIMIZE);
            this.RestoreDes = GetSystemString(ExternDll.User32, NativeMethods.IDS_RESTORE_DOWN);
            #endregion
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HwndSource = (HwndSource)PresentationSource.FromVisual(this);
            var _WINDOWPLACEMENT = ApplicationBase.ViewModel.WINDOWPLACEMENT;
            _WINDOWPLACEMENT = ApplicationBase.ViewModel.WINDOWPLACEMENT;
            if (IsAutoWindowPlacement)
            {
                NativeMethods.SetWindowPlacement(new HandleRef(null, this.HwndSource.Handle), ref _WINDOWPLACEMENT);
            }
        }


        public string CloseDes
        {
            get { return (string)GetValue(CloseDesProperty); }
            set { SetValue(CloseDesProperty, value); }
        }

        public static readonly DependencyProperty CloseDesProperty =
            DependencyProperty.Register("CloseDes", typeof(string), typeof(RSWindowBase), new PropertyMetadata(null));

        public string MinimizeDes
        {
            get { return (string)GetValue(MinimizeDesProperty); }
            set { SetValue(MinimizeDesProperty, value); }
        }

        public static readonly DependencyProperty MinimizeDesProperty =
            DependencyProperty.Register("MinimizeDes", typeof(string), typeof(RSWindowBase), new PropertyMetadata(null));


        public string MaximizeDes
        {
            get { return (string)GetValue(MaximizeDesProperty); }
            set { SetValue(MaximizeDesProperty, value); }
        }


        public static readonly DependencyProperty MaximizeDesProperty =
            DependencyProperty.Register("MaximizeDes", typeof(string), typeof(RSWindowBase), new PropertyMetadata(null));




        public string RestoreDes
        {
            get { return (string)GetValue(RestoreDesProperty); }
            set { SetValue(RestoreDesProperty, value); }
        }

        public static readonly DependencyProperty RestoreDesProperty =
            DependencyProperty.Register("RestoreDes", typeof(string), typeof(RSWindowBase), new PropertyMetadata(null));





        public bool IsAutoWindowPlacement
        {
            get { return (bool)GetValue(IsAutoWindowPlacementProperty); }
            set { SetValue(IsAutoWindowPlacementProperty, value); }
        }

        public static readonly DependencyProperty IsAutoWindowPlacementProperty =
            DependencyProperty.Register("IsAutoWindowPlacement", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(false));

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
        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty =
            DependencyProperty.Register("BorderCornerRadius", typeof(CornerRadius), typeof(RSWindowBase), new PropertyMetadata(new CornerRadius(0)));



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
            WINDOWPLACEMENT _WINDOWPLACEMENT = new WINDOWPLACEMENT();
            NativeMethods.GetWindowPlacement(new HandleRef(null, this.HwndSource.Handle), ref _WINDOWPLACEMENT);
            ApplicationBase.ViewModel.WINDOWPLACEMENT = _WINDOWPLACEMENT;
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
            NativeMethods.SetWindowPos(new HandleRef(null, hWnd), new HandleRef(null, IntPtr.Zero), 0, 0, nWidth, nHeight, (int)(SWP.NOZORDER | SWP.NOACTIVATE));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
        }

        private string GetSystemString(string dllName, uint resourceId)
        {
            IntPtr hInstance = NativeMethods.LoadLibrary(dllName);
            if (hInstance == IntPtr.Zero)
            {
                return null;
            }

            StringBuilder sb = new StringBuilder(256);
            int length = NativeMethods.LoadString(hInstance, resourceId, sb, sb.Capacity);
            return length > 0 ? sb.ToString() : null;
        }


    }
}
