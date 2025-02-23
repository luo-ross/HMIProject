using Microsoft.Win32.SafeHandles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media.Imaging;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;

namespace RS.WPFApp.Commons
{
    public class CursorHelper
    {
        static class NativeMethods
        {

            public struct IconInfo
            {

                public bool fIcon;

                public int xHotspot;

                public int yHotspot;

                public IntPtr hbmMask;

                public IntPtr hbmColor;

            }

            [DllImport("user32.dll")]

            public static extern SafeIconHandle CreateIconIndirect(ref IconInfo icon);

            [DllImport("user32.dll")]

            public static extern bool DestroyIcon(IntPtr hIcon);

            [DllImport("user32.dll")]

            [return: MarshalAs(UnmanagedType.Bool)]

            public static extern bool GetIconInfo(IntPtr hIcon, ref IconInfo pIconInfo);

        }

        [SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]

        class SafeIconHandle : SafeHandleZeroOrMinusOneIsInvalid
        {
            public SafeIconHandle() : base(true)
            {

            }

            protected override bool ReleaseHandle()
            {
                return NativeMethods.DestroyIcon(handle);
            }
        }

        public static Cursor InternalCreateCursor(System.Drawing.Bitmap bitmap, int xHotSpot, int yHotSpot)
        {
            var iconInfo = new NativeMethods.IconInfo
            {
                xHotspot = xHotSpot,
                yHotspot = yHotSpot,
                fIcon = false
            };
            NativeMethods.GetIconInfo(bitmap.GetHicon(), ref iconInfo);
            var cursorHandle = NativeMethods.CreateIconIndirect(ref iconInfo);
            return CursorInteropHelper.Create(cursorHandle);
        }

        public static Cursor CreateCursor(UIElement element, int xHotSpot = 0, int yHotSpot = 0)
        {
            element.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            element.Arrange(new Rect(new Point(), element.DesiredSize));
            var renderTargetBitmap = new RenderTargetBitmap(
              (int)element.DesiredSize.Width, (int)element.DesiredSize.Height,
              96, 96, PixelFormats.Pbgra32);
            renderTargetBitmap.Render(element);
            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(renderTargetBitmap));
            using (var memoryStream = new MemoryStream())
            {
                encoder.Save(memoryStream);
                using (var bitmap = new System.Drawing.Bitmap(memoryStream))
                {
                    return InternalCreateCursor(bitmap, xHotSpot, yHotSpot);
                }
            }
        }

    }
}
