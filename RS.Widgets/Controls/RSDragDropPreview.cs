using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Media3D;
using RS.Win32API;
using System.Runtime.InteropServices;
using System.Reflection.Metadata;
using Org.BouncyCastle.Bcpg.OpenPgp;
using RS.Widgets.Commons;
using System.Windows.Controls.Primitives;

namespace RS.Widgets.Controls
{

    public class RSDragDropPreview : Window
    {
        static RSDragDropPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDragDropPreview), new FrameworkPropertyMetadata(typeof(RSDragDropPreview)));
        }
        public RSDragDropPreview()
        {
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.IsHitTestVisible = false;
            this.ShowInTaskbar = false;
            this.Topmost = true;
            this.Left = int.MinValue;
            this.Top = int.MinValue;
            this.SizeToContent = SizeToContent.WidthAndHeight;
            this.Loaded += RSDragDropPreview_Loaded;
        }
        public RSDragDropPreview(UIElement visual) : this()
        {
            this.Content = new Border
            {
                Background = new VisualBrush(visual),
                Width = ((FrameworkElement)visual).ActualWidth,
                Height = ((FrameworkElement)visual).ActualHeight,
                Opacity = 0.8
            };
        }


        private void RSDragDropPreview_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetHitTestable(this.IsHitTestVisible);
        }


        /// <summary>
        /// 这里关键，必须设置窗体鼠标事件可以穿透
        /// </summary>
        /// <param name="hitTestable"></param>
        private void SetHitTestable(bool hitTestable)
        {
            var handle = new WindowInteropHelper(this).Handle;
            var styles = NativeMethods.GetWindowLong(new HandleRef(this, handle), NativeMethods.GWL_EXSTYLE);
            var flags = styles;
            if (((flags & NativeMethods.WS_EX_TRANSPARENT) == 0) != hitTestable)
            {
                if (hitTestable)
                {
                    styles = (flags & ~NativeMethods.WS_EX_TRANSPARENT);
                }
                else
                {
                    styles = (flags | NativeMethods.WS_EX_TRANSPARENT);
                }
                NativeMethods.SetWindowLong(new HandleRef(null, handle), NativeMethods.GWL_EXSTYLE, new HandleRef(null, styles));
            }
        }



    }
}
