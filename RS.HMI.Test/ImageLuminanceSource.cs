using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ZXing;

namespace RS.HMI.Test
{
    /// <summary>
    /// A luminance source class which consumes a Mat image from OpenCVSharp and calculates the luminance values based on the bytes of the image
    /// </summary>
    internal class ImageLuminanceSource : BaseLuminanceSource
    {
        public ImageLuminanceSource(Mat image)
           : base(image.Width, image.Height)
        {
            CalculateLuminance(image);
        }

        protected ImageLuminanceSource(byte[] luminances, int width, int height)
           : base(luminances, width, height)
        {
        }

        protected override LuminanceSource CreateLuminanceSource(byte[] newLuminances, int width, int height)
        {
            return new ImageLuminanceSource(newLuminances, width, height);
        }

        private void CalculateLuminance(Mat src)
        {
            if (src == null)
                throw new ArgumentNullException("src");
            if (src.Dims > 2)
                throw new ArgumentException("Mat dimensions must be 2");

            var pixelFormat = GetOptimumPixelFormats(src.Type());

            if (src.IsSubmatrix())
                throw new NotSupportedException("Submatrix");
            //if (src.IsContinuous())
            //   throw new NotSupportedException("Continuous");

            unsafe
            {
                byte* pSrc = (byte*)(src.Data);
                //int sstep = (int) src.Step();

                switch (pixelFormat)
                {
                    case RGBLuminanceSource.BitmapFormat.Gray8:
                        CalculateLuminanceGray8(pSrc, src.DataEnd.ToInt64() - src.DataStart.ToInt64());
                        break;
                    case RGBLuminanceSource.BitmapFormat.BGR24:
                        CalculateLuminanceBGR24(pSrc, src.DataEnd.ToInt64() - src.DataStart.ToInt64());
                        break;
                    case RGBLuminanceSource.BitmapFormat.BGRA32:
                        CalculateLuminanceBGRA32(pSrc, src.DataEnd.ToInt64() - src.DataStart.ToInt64());
                        break;
                }
            }
        }

        private static RGBLuminanceSource.BitmapFormat GetOptimumPixelFormats(MatType type)
        {
            if (type == MatType.CV_8UC1 || type == MatType.CV_8SC1)
                return RGBLuminanceSource.BitmapFormat.Gray8;
            if (type == MatType.CV_8UC3 || type == MatType.CV_8SC3)
                return RGBLuminanceSource.BitmapFormat.BGR24;
            if (type == MatType.CV_8UC4 || type == MatType.CV_8SC4)
                return RGBLuminanceSource.BitmapFormat.BGRA32;

            //if (type == MatType.CV_16UC1 || type == MatType.CV_16SC1)
            //   return RGBLuminanceSource.BitmapFormat.Gray16;
            //if (type == MatType.CV_16UC3 || type == MatType.CV_16SC3)
            //   return RGBLuminanceSource.BitmapFormat.Rgb48;
            //if (type == MatType.CV_16UC4 || type == MatType.CV_16SC4)
            //   return RGBLuminanceSource.BitmapFormat.Rgba64;

            //if (type == MatType.CV_32SC4)
            //   return RGBLuminanceSource.BitmapFormat.Prgba64;

            //if (type == MatType.CV_32FC1)
            //   return RGBLuminanceSource.BitmapFormat.Gray32Float;
            //if (type == MatType.CV_32FC3)
            //   return RGBLuminanceSource.BitmapFormat.Rgb128Float;
            //if (type == MatType.CV_32FC4)
            //   return RGBLuminanceSource.BitmapFormat.Rgba128Float;

            throw new ArgumentOutOfRangeException(type.GetType().Name, "Not supported MatType");
        }

        private unsafe void CalculateLuminanceGray8(byte* rgbRawBytes, long length)
        {
            for (int index = 0, luminanceIndex = 0; index < length && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // MemCopy should be faster
                luminances[luminanceIndex] = rgbRawBytes[index++];
            }
        }

        private unsafe void CalculateLuminanceBGR24(byte* rgbRawBytes, long length)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < length && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                int b = rgbRawBytes[rgbIndex++];
                int g = rgbRawBytes[rgbIndex++];
                int r = rgbRawBytes[rgbIndex++];
                luminances[luminanceIndex] = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
            }
        }

        private unsafe void CalculateLuminanceBGRA32(byte* rgbRawBytes, long length)
        {
            for (int rgbIndex = 0, luminanceIndex = 0; rgbIndex < length && luminanceIndex < luminances.Length; luminanceIndex++)
            {
                // Calculate luminance cheaply, favoring green.
                var b = rgbRawBytes[rgbIndex++];
                var g = rgbRawBytes[rgbIndex++];
                var r = rgbRawBytes[rgbIndex++];
                var alpha = rgbRawBytes[rgbIndex++];
                var luminance = (byte)((RChannelWeight * r + GChannelWeight * g + BChannelWeight * b) >> ChannelWeight);
                luminances[luminanceIndex] = (byte)(((luminance * alpha) >> 8) + (255 * (255 - alpha) >> 8));
            }
        }
    }
}
