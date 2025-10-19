using OpenCvSharp;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Imaging;

namespace RS.HMI.Client.Commons
{
    public class CvMatHelper
    {
        /// <summary>
        /// 获取Windows Forms Bitmap（零拷贝版本）
        /// </summary>
        /// <param name="mat">要转换的Mat</param>
        /// <returns>Windows Forms Bitmap</returns>
        public static Bitmap GetBitmap(Mat mat)
        {
            Mat displayMat = PrepareMatForDisplay(mat);
            try
            {
                return new Bitmap(
                    displayMat.Width,
                    displayMat.Height,
                    (int)displayMat.Step(),
                    System.Drawing.Imaging.PixelFormat.Format24bppRgb,
                    displayMat.Data
                );
            }
            catch (Exception ex)
            {
                return null;
            }
        }

        /// <summary>
        /// 获取WPF BitmapSource（零拷贝版本）
        /// </summary>
        /// <param name="mat">要转换的Mat</param>
        /// <returns>WPF BitmapSource</returns>
        public static BitmapSource GetBitmapSource(Mat mat)
        {
            Mat displayMat = PrepareMatForDisplay(mat);

            unsafe
            {
                try
                {
                    return BitmapSource.Create(
                        displayMat.Width,
                        displayMat.Height,
                        96.0, 96.0,
                        System.Windows.Media.PixelFormats.Rgb24,
                        null,
                        new IntPtr((byte*)displayMat.Data.ToPointer()),
                        (int)displayMat.Step() * displayMat.Height,
                        (int)displayMat.Step()
                    );
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"创建BitmapSource失败: {ex.Message}");
                    return null;
                }
            }
        }



        public static Mat PrepareMatForDisplay(Mat mat)
        {
            // 计算正确的步长
            int width = mat.Width;
            int height = mat.Height;
            int channels = mat.Channels();
            int theoreticalStride = width * channels;
            int alignedStride = (theoreticalStride + 3) & ~3; // 4字节对齐

            if (theoreticalStride != alignedStride)
            {
                int paddingBytes = alignedStride - theoreticalStride;
                paddingBytes = paddingBytes / 2;
                Cv2.CopyMakeBorder(
                    mat, mat, 0, 0, paddingBytes, paddingBytes,
                    BorderTypes.Constant, new Scalar(0, 0, 0)
                );
            }

            // 颜色转换
            ConvertMatColorSpace(mat);

            return mat;
        }

        /// <summary>
        /// 转换Mat的颜色空间
        /// </summary>
        /// <param name="mat">要转换的Mat</param>
        public static void ConvertMatColorSpace(Mat mat)
        {
            switch (mat.Channels())
            {
                case 1:
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.GRAY2RGB);
                    break;
                case 3:
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGR2RGB);
                    break;
                case 4:
                    Cv2.CvtColor(mat, mat, ColorConversionCodes.BGRA2RGB);
                    break;
            }
        }



    }
}
