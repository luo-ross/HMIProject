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

namespace RS.Widgets.Commons
{
    public static class ImgHelper
    {


        /// <summary>
        /// Planar格式图像转Bitmap
        /// </summary>
        /// <param name="pointerRed">R平面</param>
        /// <param name="pointerGreen">G平面</param>
        /// <param name="pointerBlue">B平面</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns></returns>
        public static unsafe Bitmap GetBitmap(IntPtr pointerRed,
            IntPtr pointerGreen,
            IntPtr pointerBlue,
            int width,
            int height)
        {
            try
            {
                var pixelCount = width * height;

                // 创建 RGB Bitmap
                Bitmap bitmap = new Bitmap(width,
                    height,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                // 锁定位图数据
                BitmapData bitmapData = bitmap.LockBits(
                    new Rectangle(0, 0, width, height),
                    ImageLockMode.WriteOnly,
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb);

                try
                {

                    // 处理 Planar 格式数据
                    byte* rPtr = (byte*)pointerRed.ToPointer();
                    byte* gPtr = (byte*)pointerGreen.ToPointer();
                    byte* bPtr = (byte*)pointerBlue.ToPointer();
                    byte* dstPtr = (byte*)bitmapData.Scan0.ToPointer();

                    // 将工作分成多个块
                    int processorCount = Environment.ProcessorCount;
                    int chunkSize = pixelCount / processorCount;
                    Parallel.For(0, processorCount, (int chunk) =>
                    {
                        int start = chunk * chunkSize;
                        int end = (chunk == processorCount - 1) ?
                        pixelCount : (chunk + 1) * chunkSize;

                        for (int i = start; i < end; i++)
                        {
                            dstPtr[i * 3 + 0] = rPtr[i];
                            dstPtr[i * 3 + 1] = gPtr[i];
                            dstPtr[i * 3 + 2] = bPtr[i];
                        }
                    });
                }
                finally
                {
                    bitmap.UnlockBits(bitmapData);
                }
                return bitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        /// <summary>
        /// Planar格式图像转BitmapSource
        /// </summary>
        /// <param name="pointerRed">R平面</param>
        /// <param name="pointerGreen">G平面</param>
        /// <param name="pointerBlue">B平面</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns></returns>
        public static unsafe BitmapSource GetBitmapSource(IntPtr pointerRed,
            IntPtr pointerGreen,
            IntPtr pointerBlue,
            int width,
            int height)
        {
            try
            {
                var pixelCount = width * height;

                // 创建像素数据数组
                byte[] pixelData = new byte[pixelCount * 3]; // RGB24格式，每像素3字节

                // 处理 Planar 格式数据
                byte* rPtr = (byte*)pointerRed.ToPointer();
                byte* gPtr = (byte*)pointerGreen.ToPointer();
                byte* bPtr = (byte*)pointerBlue.ToPointer();

                // 并行处理像素数据
                int processorCount = Environment.ProcessorCount;
                int chunkSize = pixelCount / processorCount;

                Parallel.For(0, processorCount, (int chunk) =>
                {
                    int start = chunk * chunkSize;
                    int end = (chunk == processorCount - 1)
                    ? pixelCount : (chunk + 1) * chunkSize;

                    for (int i = start; i < end; i++)
                    {
                        int pixelIndex = i * 3;
                        pixelData[pixelIndex + 0] = rPtr[i]; // R
                        pixelData[pixelIndex + 1] = gPtr[i]; // G
                        pixelData[pixelIndex + 2] = bPtr[i]; // B
                    }
                });

                // 创建BitmapSource
                return BitmapSource.Create(
                    width,
                    height,
                    96, 96, // DPI
                    PixelFormats.Rgb24,
                    null,
                    pixelData,
                    width * 3); // stride
            }
            catch (Exception ex)
            {
                return null;
            }
        }


        /// <summary>
        /// Planar格式图像转WriteableBitmap
        /// </summary>
        /// <param name="pointerRed">R平面</param>
        /// <param name="pointerGreen">G平面</param>
        /// <param name="pointerBlue">B平面</param>
        /// <param name="width">图像宽度</param>
        /// <param name="height">图像高度</param>
        /// <returns></returns>
        public static unsafe WriteableBitmap GetWriteableBitmap(IntPtr pointerRed,
            IntPtr pointerGreen,
            IntPtr pointerBlue,
            int width,
            int height)
        {
            try
            {
                // 创建WriteableBitmap
                WriteableBitmap bitmap = new WriteableBitmap(
                    width,
                    height,
                    96, 96, // DPI
                    PixelFormats.Bgra32, // 使用BGRA32格式，WPF推荐格式
                    null);

                // 锁定位图进行写入
                bitmap.Lock();

                try
                {
                    // 处理 Planar 格式数据
                    byte* rPtr = (byte*)pointerRed.ToPointer();
                    byte* gPtr = (byte*)pointerGreen.ToPointer();
                    byte* bPtr = (byte*)pointerBlue.ToPointer();
                    byte* dstPtr = (byte*)bitmap.BackBuffer.ToPointer();

                    // 并行处理像素数据
                    int processorCount = Environment.ProcessorCount;
                    int chunkSize = (width * height) / processorCount;

                    Parallel.For(0, processorCount, (int chunk) =>
                    {
                        int start = chunk * chunkSize;
                        int end = (chunk == processorCount - 1)
                        ? (width * height) : (chunk + 1) * chunkSize;

                        for (int i = start; i < end; i++)
                        {
                            int x = i % width;
                            int y = i / width;
                            int dstIndex = y * bitmap.BackBufferStride + x * 4;

                            // BGRA32格式：BGRA顺序
                            dstPtr[dstIndex + 0] = bPtr[i]; // B
                            dstPtr[dstIndex + 1] = gPtr[i]; // G
                            dstPtr[dstIndex + 2] = rPtr[i]; // R
                            dstPtr[dstIndex + 3] = 255;     // A (完全不透明)
                        }
                    });

                    // 标记脏区域
                    bitmap.AddDirtyRect(new Int32Rect(0, 0, width, height));
                }
                finally
                {
                    bitmap.Unlock();
                }

                return bitmap;
            }
            catch (Exception ex)
            {
                return null;
            }
        }



        public static unsafe Bitmap Create8BitBitmap(Bitmap src)
        {
            Bitmap bitmap = new Bitmap(src.Width, src.Height, PixelFormat.Format8bppIndexed);
            ColorPalette palette = bitmap.Palette;
            for (int i = 0; i < 256; i++)
            {
                ref System.Drawing.Color reference = ref palette.Entries[i];
                reference = System.Drawing.Color.FromArgb(i, i, i);
            }

            bitmap.Palette = palette;
            Rectangle rect = new Rectangle(0, 0, src.Width, src.Height);
            BitmapData bitmapData = bitmap.LockBits(rect, ImageLockMode.ReadWrite, PixelFormat.Format8bppIndexed);
            int num = bitmapData.Stride - bitmapData.Width;
            byte* ptr = (byte*)(void*)bitmapData.Scan0;
            int num2 = 0;
            while (num2 < src.Height)
            {
                int num3 = 0;
                while (num3 < src.Width)
                {
                    System.Drawing.Color pixel = src.GetPixel(num3, num2);
                    int num4 = (pixel.R + pixel.G + pixel.B) / 3;
                    *ptr = (byte)num4;
                    num3++;
                    ptr++;
                }
                num2++;
                ptr += num;
            }

            bitmap.UnlockBits(bitmapData);
            return bitmap;
        }

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
