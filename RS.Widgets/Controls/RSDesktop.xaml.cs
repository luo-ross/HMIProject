using RS.Win32API;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// RSDesktop.xaml 的交互逻辑
    /// </summary>
    public partial class RSDesktop : Window
    {
        public RSDesktop()
        {
            InitializeComponent();
            InitRSDesktop();
            this.Loaded += RSDesktop_Loaded;
            this.Closed += RSDesktop_Closed;
        }

        private void RSDesktop_Closed(object? sender, EventArgs e)
        {
            this.MediaVideo.Close();
            this.MediaVideo = null;
            RefreshDesktop();
        }

        private unsafe void RefreshDesktop()
        {
            NativeMethods.SystemParametersInfo(NativeMethods.SPI_SETDESKWALLPAPER, 0, IntPtr.Zero, (int)SPIF.UPDATEINIFILE);
        }

        private void RSDesktop_Loaded(object sender, RoutedEventArgs e)
        {
            RefreshDesktop();
            var progmanHWND = NativeMethods.FindWindow("Progman", null);
            NativeMethods.SendMessage(progmanHWND, 0x052c, IntPtr.Zero, IntPtr.Zero);
            IntPtr workerWHWND = default;
            NativeMethods.EnumWindows((hWND, lPARAM) =>
            {
                var sHELLDLL_DefViewHWND = NativeMethods.FindWindowEx(hWND, default, "SHELLDLL_DefView", null);
                if (sHELLDLL_DefViewHWND != default)
                {
                    workerWHWND = NativeMethods.FindWindowEx(default, hWND, "WorkerW", null);
                    return false;
                }
                return true;
            }, IntPtr.Zero);
            if (workerWHWND == default)
            {
                throw new Exception("创建动态桌面失败！");
            }
            NativeMethods.ShowWindow(workerWHWND, NativeMethods.SW_HIDE);
            var rSDesktopHWND = NativeMethods.FindWindow(null, "RSDesktop");
            NativeMethods.SetParent(rSDesktopHWND, progmanHWND);
            this.MediaVideo.Play();
        }

        private void InitRSDesktop()
        {
            this.WindowStyle = WindowStyle.None;
            this.ResizeMode = ResizeMode.NoResize;
            this.Left = 0;
            this.Top = 0;
            this.Width = SystemParameters.WorkArea.Width;
            this.Height = SystemParameters.WorkArea.Height;
        }

        public Uri VideoSourceUri
        {
            get { return (Uri)GetValue(VideoSourceUriProperty); }
            set { SetValue(VideoSourceUriProperty, value); }
        }
        public static readonly DependencyProperty VideoSourceUriProperty =
            DependencyProperty.Register("VideoSourceUri", typeof(Uri), typeof(RSDesktop), new PropertyMetadata(null));

        private void MediaVideo_MediaEnded(object sender, RoutedEventArgs e)
        {
            this.MediaVideo.Stop();
            this.MediaVideo.Play();
        }
    }
}
