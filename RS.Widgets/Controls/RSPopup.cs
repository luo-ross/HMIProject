using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;
using Windows.Win32;
using Windows.Win32.Foundation;
namespace RS.Widgets.Controls
{

    public class RSPopup : Popup
    {
        /// <summary> 
        /// 加载窗口随动事件 
        /// </summary> 
        public RSPopup()
        {
            this.Loaded += RSPopup_Loaded;
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
            UpdateWindow();
        }

        /// <summary>  
        /// 更新Popup窗体位置
        /// </summary>  
        private void UpdateWindow()
        {
            var handle = ((HwndSource)PresentationSource.FromVisual(this.Child)).Handle;
            HWND hWnd = new HWND(handle);
            if (Ross.GetWindowRect(hWnd, out RECT lpRect))
            {
                Ross.SetWindowPos(new HWND(hWnd),new HWND(Topmost ? -1 : -2) , lpRect.left, lpRect.top, (int)this.Width, (int)this.Height, 0);
            }
        }

    }
}
