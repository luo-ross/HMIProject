using Microsoft.Win32.SafeHandles;
using RS.Widgets.Structs;
using RS.Win32API;
using RS.Win32API.SafeHandles;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace RS.Widgets.Utilities
{
    public class CursorHelper
    {
        public static Cursor InternalCreateCursor(System.Drawing.Bitmap bitmap, int xHotSpot, int yHotSpot)
        {
            NativeMethods.GetIconInfo(new HandleRef(null, bitmap.GetHicon()), out ICONINFO iconInfo);
            iconInfo.xHotspot = xHotSpot;
            iconInfo.yHotspot = yHotSpot;
            iconInfo.fIcon = false;
            var cursorHandle = NativeMethods.CreateIconIndirect(iconInfo);
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


        /// <summary>
        /// 渲染任意角度旋转的BitmapSource，动态扩大画布防止裁剪
        /// </summary>
        public static BitmapSource RotateBitmapSource(BitmapSource source, double rotationAngle)
        {
            // 计算旋转后的外接矩形尺寸
            double angleRad = rotationAngle * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(angleRad));
            double sin = Math.Abs(Math.Sin(angleRad));

            int newWidth = (int)(source.Width * cos + source.Height * sin);
            int newHeight = (int)(source.Width * sin + source.Height * cos);

            // 至少保持 1 像素，避免 RenderTargetBitmap 报错
            newWidth = Math.Max(1, newWidth);
            newHeight = Math.Max(1, newHeight);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                // 将旋转中心平移到新画布的中心
                dc.PushTransform(new TranslateTransform(newWidth / 2.0, newHeight / 2.0));
                dc.PushTransform(new RotateTransform(rotationAngle));
                // 图片相对于自身中心点绘制
                dc.DrawImage(source, new Rect(-source.Width / 2.0, -source.Height / 2.0, source.Width, source.Height));
                dc.Pop();
                dc.Pop();
            }

            RenderTargetBitmap renderTarget = new RenderTargetBitmap(
                newWidth, newHeight,
                source.DpiX, source.DpiY,
                PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);

            return renderTarget;
        }


        /// <summary>
        /// 将 BitmapSource 转换为带 Alpha 通道的光标
        /// </summary>
        public static Cursor CreateCursor(BitmapSource bitmapSource, int xHotspot = -1, int yHotspot = -1)
        {
            IntPtr hBitmap = IntPtr.Zero;
            BitmapHandle hMonoMask = BitmapHandle.CreateFromHandle(IntPtr.Zero);

            try
            {
                // 使用 PNG 编码中转以保留 Alpha 通道 (利用 ToBitmap 扩展)
                using (var drawBitmap = bitmapSource.ToBitmap())
                {
                    hBitmap = drawBitmap.GetHbitmap();
                    hMonoMask = CreateMonoMask(drawBitmap.Width, drawBitmap.Height);

                    var iconInfo = new ICONINFO
                    {
                        fIcon = false, // false 表示光标
                        xHotspot = xHotspot < 0 ? drawBitmap.Width / 2 : xHotspot,
                        yHotspot = yHotspot < 0 ? drawBitmap.Height / 2 : yHotspot,
                        hbmMask = hMonoMask,
                        hbmColor = BitmapHandle.CreateFromHandle(hBitmap)
                    };

                    IconHandle cursorHandle = NativeMethods.CreateIconIndirect(iconInfo);
                    if (cursorHandle.CriticalGetHandle() == IntPtr.Zero)
                    {
                        return Cursors.None;
                    }
                    return CursorInteropHelper.Create(cursorHandle);
                }
            }
            catch
            {
                return Cursors.None;
            }
            finally
            {
                if (hBitmap != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(hBitmap);
                }
                if (hMonoMask.DangerousGetHandle() != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(hMonoMask.DangerousGetHandle());
                }
            }
        }

        public static BitmapHandle CreateMonoMask(int width, int height)
        {
            // 单色位图每行必须是 16 bit (2 bytes) 对齐的
            int stride = ((width + 15) >> 4) << 1;
            byte[] bits = new byte[stride * height];
            return NativeMethods.CreateBitmap(width, height, 1, 1, bits);
        }

        public static BitmapSource DrawingImageToBitmapSource(DrawingImage drawingImage, int width = 32, int height = 32)
        {
            var renderTarget = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            var drawingVisual = new DrawingVisual();
            using (var dc = drawingVisual.RenderOpen())
            {
                dc.DrawImage(drawingImage, new Rect(0, 0, width, height));
            }
            renderTarget.Render(drawingVisual);
            return renderTarget;
        }


        /// <summary>
        /// 从系统 Cursor 提取 CursorData (带位图和热点)
        /// </summary>
        public static CursorData GetCursorData(Cursor cursor)
        {
            IntPtr hCursor = GetCursorHandle(cursor);
            if (hCursor == IntPtr.Zero)
            {
                return default;
            }
            NativeMethods.GetIconInfo(new HandleRef(null, hCursor), out ICONINFO iconInfo);
            try
            {
                BitmapSource bitmap = Imaging.CreateBitmapSourceFromHBitmap(
                    iconInfo.hbmColor.DangerousGetHandle() != IntPtr.Zero ? iconInfo.hbmColor.DangerousGetHandle() : iconInfo.hbmMask.DangerousGetHandle(),
                    IntPtr.Zero,
                    Int32Rect.Empty,
                    BitmapSizeOptions.FromEmptyOptions());

                return new CursorData
                {
                    Bitmap = bitmap,
                    HotspotX = iconInfo.xHotspot,
                    HotspotY = iconInfo.yHotspot
                };
            }
            finally
            {
                if (iconInfo.hbmColor.DangerousGetHandle() != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(iconInfo.hbmColor.DangerousGetHandle());
                }
                if (iconInfo.hbmMask.DangerousGetHandle() != IntPtr.Zero)
                {
                    NativeMethods.DeleteObject(iconInfo.hbmMask.DangerousGetHandle());
                }
            }
        }

        public static BitmapSource GetBitmapSourceFromCursor(Cursor cursor)
        {
            return GetCursorData(cursor).Bitmap;
        }

        private static IntPtr GetCursorHandle(Cursor cursor)
        {
            var safeHandle = cursor.ReflectionGetField<SafeHandle>("_cursorHandle");
            return safeHandle?.DangerousGetHandle() ?? IntPtr.Zero;
        }


        public static CursorData RotateCursor(CursorData data, double rotationAngle)
        {
            double angleRad = rotationAngle * Math.PI / 180.0;
            double cos = Math.Abs(Math.Cos(angleRad));
            double sin = Math.Abs(Math.Sin(angleRad));

            int oldWidth = data.Bitmap.PixelWidth;
            int oldHeight = data.Bitmap.PixelHeight;

            int newWidth = (int)Math.Ceiling(oldWidth * cos + oldHeight * sin);
            int newHeight = (int)Math.Ceiling(oldWidth * sin + oldHeight * cos);

            newWidth = Math.Max(1, newWidth);
            newHeight = Math.Max(1, newHeight);

            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext dc = drawingVisual.RenderOpen())
            {
                dc.PushTransform(new TranslateTransform(newWidth / 2.0, newHeight / 2.0));
                dc.PushTransform(new RotateTransform(rotationAngle));
                dc.DrawImage(data.Bitmap, new Rect(-oldWidth / 2.0, -oldHeight / 2.0, oldWidth, oldHeight));
                dc.Pop();
                dc.Pop();
            }

            RenderTargetBitmap renderTarget = new RenderTargetBitmap(newWidth, newHeight, 96, 96, PixelFormats.Pbgra32);
            renderTarget.Render(drawingVisual);

            // 计算旋转后的热点
            // 热点相对于中心点的向量进行旋转
            double centerX = oldWidth / 2.0;
            double centerY = oldHeight / 2.0;

            double dx = data.HotspotX - centerX;
            double dy = data.HotspotY - centerY;

            double realCos = Math.Cos(angleRad);
            double realSin = Math.Sin(angleRad);

            double rotatedDx = dx * realCos - dy * realSin;
            double rotatedDy = dx * realSin + dy * realCos;

            return new CursorData
            {
                Bitmap = renderTarget,
                HotspotX = (int)Math.Round(newWidth / 2.0 + rotatedDx),
                HotspotY = (int)Math.Round(newHeight / 2.0 + rotatedDy)
            };
        }
    }
}
