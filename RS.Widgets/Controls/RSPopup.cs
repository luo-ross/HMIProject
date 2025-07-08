using RS.Win32API;
using RS.Win32API.Structs;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
namespace RS.Widgets.Controls
{
    public class RSPopup : Popup
    {
        private HwndSource HwndSource;
        /// <summary> 
        /// 加载窗口随动事件 
        /// </summary> 
        public RSPopup()
        {
            this.Loaded += RSPopup_Loaded;
            this.Opened += RSPopup_Opened;
        }

        private void RSPopup_Opened(object? sender, EventArgs e)
        {

        }

        private void RSPopup_Loaded(object sender, RoutedEventArgs e)
        {
            var rsWindow = this.TryFindParent<RSWindow>();
            if (rsWindow != null)
            {
                rsWindow.LocationChanged -= RsWindow_LocationChanged;
                rsWindow.SizeChanged -= RsWindow_SizeChanged;
                rsWindow.LocationChanged += RsWindow_LocationChanged;
                rsWindow.SizeChanged += RsWindow_SizeChanged;
            }
        }

        private void RsWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdatePopupPosition();
        }

        /// <summary> 
        /// 刷新位置 
        /// </summary> 
        private void RsWindow_LocationChanged(object? sender, EventArgs e)
        {
            this.UpdatePopupPosition();
        }

        private void UpdatePopupPosition()
        {
            try
            {
                var updatePosition = typeof(Popup).GetMethod("UpdatePosition", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                if (this.IsOpen)
                {
                    updatePosition?.Invoke(this, null);
                }
            }
            catch
            {
                return;
            }
        }


        public static DependencyProperty TopmostProperty = Window.TopmostProperty.AddOwner(typeof(Popup), new FrameworkPropertyMetadata(false, OnTopmostPropertyChanged));
        public bool Topmost
        {
            get { return (bool)GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }

        private static void OnTopmostPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            (d as RSPopup)?.UpdateWindow();
        }

        /// <summary>  
        /// 重写
        /// </summary>  
        /// <param name="e"></param>  
        protected override void OnOpened(EventArgs e)
        {
            base.OnOpened(e);
            UpdateWindow();
        }

        /// <summary>  
        /// 更新Popup窗体位置
        /// </summary>  
        private void UpdateWindow()
        {
            var handle = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;

            RECT lpRect = new RECT();
            if (NativeMethods.IntGetWindowRect(new HandleRef(null, handle), ref lpRect))
            {
                NativeMethods.SetWindowPos(new HandleRef(null, handle), new HandleRef(null, Topmost ? -1 : -2), lpRect.Left, lpRect.Top, (int)this.Width, (int)this.Height, 0);
            }
        }
    }
}
