using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Windows;
using System.Windows.Media.Imaging;
using PixelFormat = System.Drawing.Imaging.PixelFormat;

namespace RS.Commons.Helper
{
    public static class ImgHelper
    {
        public static BitmapImage GetBitmapSource(string imgPath, int decodePixelWidth = 0, int decodePixelHeight = 0)
        {
            BitmapImage image = new BitmapImage();
            image.BeginInit();
            image.CacheOption = BitmapCacheOption.Default;
            if (decodePixelHeight > 0)
            {
                image.DecodePixelWidth = decodePixelWidth;
            }

            if (decodePixelWidth > 0)
            {
                image.DecodePixelWidth = decodePixelWidth;
            }

            image.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
            image.UriSource = new Uri(imgPath, UriKind.Absolute);
            image.EndInit();
            image.Freeze();

            return image;
        }

        public static Bitmap ToBitmap(this BitmapSource bitmapSource)
        {
            // 获取 BitmapSource 的像素格式
            var sourceFormat = bitmapSource.Format;

            // 根据 BitmapSource 的格式选择目标 Bitmap 的格式
            PixelFormat targetFormat;
            switch (sourceFormat.ToString())
            {
                case nameof(PixelFormats.Bgr24):
                    targetFormat = PixelFormat.Format24bppRgb;
                    break;
                case nameof(PixelFormats.Bgr32):
                case nameof(PixelFormats.Bgra32):
                    targetFormat = PixelFormat.Format32bppArgb;
                    break;
                case nameof(PixelFormats.Pbgra32):
                    targetFormat = PixelFormat.Format32bppPArgb;
                    break;
                case nameof(PixelFormats.Gray8):
                    targetFormat = PixelFormat.Format8bppIndexed;
                    break;
                default:
                    throw new NotSupportedException($"不支持的像素格式: {sourceFormat}");
            }

            // 创建 Bitmap 对象
            var bitmap = new Bitmap(
                bitmapSource.PixelWidth,
                bitmapSource.PixelHeight,
                targetFormat);

            // 锁定 Bitmap 数据
            var bitmapData = bitmap.LockBits(
                new Rectangle(0, 0, bitmap.Width, bitmap.Height),
                ImageLockMode.WriteOnly,
                bitmap.PixelFormat);

            try
            {
                // 将 BitmapSource 数据复制到 Bitmap
                bitmapSource.CopyPixels(
                    Int32Rect.Empty,
                    bitmapData.Scan0,
                    bitmapData.Height * bitmapData.Stride,
                    bitmapData.Stride);

                // 如果需要，处理 Alpha 通道
                if (sourceFormat == PixelFormats.Pbgra32 || sourceFormat == PixelFormats.Bgra32)
                {
                    ConvertBgraToArgb(bitmapData.Scan0, bitmapData.Width, bitmapData.Height, bitmapData.Stride);
                }
            }
            finally
            {
                // 解锁 Bitmap 数据
                bitmap.UnlockBits(bitmapData);
            }

            return bitmap;
        }

        // 将 BGRA 数据转换为 ARGB 数据
        private static unsafe void ConvertBgraToArgb(IntPtr scan0, int width, int height, int stride)
        {
            byte* pPixel = (byte*)scan0;

            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    // 获取当前像素的 BGRA 值
                    byte b = pPixel[0];
                    byte g = pPixel[1];
                    byte r = pPixel[2];
                    byte a = pPixel[3];

                    // 转换为 ARGB
                    pPixel[0] = b;
                    pPixel[1] = g;
                    pPixel[2] = r;
                    pPixel[3] = a;

                    // 移动到下一个像素
                    pPixel += 4;
                }

                // 移动到下一行
                pPixel += stride - (width * 4);
            }
        }
      
    }
}
