using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OpenCvSharp;
using RS.Commons;
using RS.Commons.Attributs;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TencentCloud.Tiems.V20190416.Models;

namespace RS.HMIServer.BLL
{
    public enum WatermarkPosition
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight,
        Center
    }


    [ServiceInjectConfig(typeof(IOpenCVBLL), ServiceLifetime.Singleton, IsInterceptor = true)]
    public class OpenCVBLL : IOpenCVBLL
    {
        private readonly ILogBLL LogBLL;
        private readonly IConfiguration Configuration;

        public OpenCVBLL(IConfiguration configuration, ILogBLL logBLL)
        {
            this.LogBLL = logBLL;
            this.Configuration = configuration;
        }

        public async Task<OperateResult<ImgVerifyInitModel>> GetVerifyImgInitModelAsync()
        {
            string verifyImgDir = Directory.GetCurrentDirectory();
            verifyImgDir = Path.Combine(verifyImgDir, "VerifyImgs");
            string iconsDir = Path.Combine(verifyImgDir, "Icons");
            var iconsList = Directory.GetFiles(iconsDir);
            var iconPath = iconsList[Random.Shared.Next(0, iconsList.Length)];
            var iconMat = Cv2.ImRead(iconPath);

            var iconMatGray = iconMat.CvtColor(ColorConversionCodes.BGR2GRAY);
            var iconMask = iconMatGray.Threshold(127, 255, ThresholdTypes.Binary);

            var fileList = Directory.GetFiles(verifyImgDir);
            var verifyImgPath = fileList[Random.Shared.Next(fileList.Length)];
            var imgMat = Cv2.ImRead(verifyImgPath);

            //var sliderBtnWidth = 50;
            //var sliderBtnHeight = 50;
            var iconWidth = iconMat.Width;
            var iconHeight = iconMat.Height;
            var imgWidth = imgMat.Width;
            var imgHeight = imgMat.Height;

            //iconMask = iconMask.Resize(new Size(sliderBtnWidth, sliderBtnHeight));

            //然后随机获取另外2个坐标点
            var positionList = this.GetRandomPosition(imgMat.Width, imgMat.Height, iconWidth, iconHeight, 4);

            var firstPosition = positionList.First();
            var lastPosition = positionList.Last();
            Rect roiConfirm = new Rect(firstPosition.left, firstPosition.top, iconWidth, iconHeight);

            //先裁剪图
            using var roiMat = imgMat[roiConfirm].Clone();

            Mat bitwiseAndMat = new Mat();
            Cv2.BitwiseAnd(roiMat, roiMat, bitwiseAndMat, iconMask);

            using var bgraCropped = new Mat();
            Cv2.CvtColor(bitwiseAndMat, bgraCropped, ColorConversionCodes.BGR2BGRA);

            var channels = bgraCropped.Split();
            channels[3] = iconMask;
            Mat mergeMat = new Mat();
            Cv2.Merge(channels, mergeMat);

            //mergeMat.SaveImage("heart2.png");
            //Cv2.ImShow("123123", mergeMat);
            //Cv2.WaitKey(0);

            for (int i = 0; i < positionList.Count - 1; i++)
            {
                var position = positionList[i];
                Rect roi = new Rect(position.left, position.top, iconWidth, iconHeight);
                // 这里把颜色也随机

                using var colorMask = new Mat(iconMask.Size(), MatType.CV_8UC3, new Scalar(Random.Shared.Next(255), Random.Shared.Next(255), Random.Shared.Next(255)));
                var subMat = imgMat.SubMat(roi);
                //这里第一个不需要旋转
                if (i == 0)
                {
                    colorMask.CopyTo(subMat, iconMask);
                }
                else
                {
                    //其他的执行旋转
                    var iconMaskClone = iconMask.Clone();
                    iconMaskClone = RotateHeart(iconMaskClone, Random.Shared.Next(0, 360));
                    colorMask.CopyTo(subMat, iconMaskClone);
                }
            }
            ////要把BGR转为RGB
            //imgMat = imgMat.CvtColor(ColorConversionCodes.BGR2RGB);
            //mergeMat = mergeMat.CvtColor(ColorConversionCodes.BGRA2RGBA);
            //Cv2.ImShow("123123", imgMat);
            //Cv2.WaitKey(0);
            //这样子就获取到了ROI Rect 还有一个透明图片
            var image1Bytes = imgMat.ToBytes();
            var image2Bytes = mergeMat.ToBytes();

            // 创建一个新的数组来存储两个图片的数据
            // 前8个字节用来存储第一个图片的长度（作为分隔标记）
            var combinedLength = 8 + image1Bytes.Length + image2Bytes.Length;
            var combinedBytes = new byte[combinedLength];

            // 写入第一个图片的长度（转换为字节数组）
            BitConverter.GetBytes(image1Bytes.Length).CopyTo(combinedBytes, 0);
            // 复制第一个图片数据
            image1Bytes.CopyTo(combinedBytes, 8);
            // 复制第二个图片数据
            image2Bytes.CopyTo(combinedBytes, 8 + image1Bytes.Length);

            ImgVerifyInitModel verifyImgInitModel = new ImgVerifyInitModel();
            verifyImgInitModel.ImgBuffer = combinedBytes;
            verifyImgInitModel.ImgWidth = imgWidth;
            verifyImgInitModel.ImgHeight = imgHeight;
            verifyImgInitModel.IconWidth = iconWidth;
            verifyImgInitModel.IconHeight = iconHeight;
            verifyImgInitModel.Rect = new RectModel(roiConfirm.X, roiConfirm.Y, roiConfirm.Width, roiConfirm.Height);
            verifyImgInitModel.ImgBtnPositionX = lastPosition.left;
            verifyImgInitModel.ImgBtnPositionY = lastPosition.top;

            return OperateResult.CreateSuccessResult(verifyImgInitModel);
        }



        private Mat RotateHeart(Mat maskImage, double angle)
        {
            // 获取图像中心点
            Point2f center = new Point2f(maskImage.Width / 2f, maskImage.Height / 2f);

            // 计算旋转矩阵
            Mat rotationMatrix = Cv2.GetRotationMatrix2D(center, angle, 1.0);

            // 创建输出图像
            Mat rotatedMask = new Mat();

            // 执行旋转，使用白色填充空白区域
            Cv2.WarpAffine(
                maskImage,
                rotatedMask,
                rotationMatrix,
                maskImage.Size(),
                InterpolationFlags.Linear,
                BorderTypes.Constant,
                new Scalar(0) // 背景填充黑色
            );

            return rotatedMask;
        }


        private List<(int left, int top)> GetRandomPosition(int imgWidth, int imgHeight, int rectWidth, int rectHeight, int pointCount)
        {
            // 参数验证
            if (imgWidth <= rectWidth || imgHeight <= rectHeight || pointCount <= 0)
            {
                throw new ArgumentException("Invalid parameters");
            }

            List<(int x, int y)> pointList = new List<(int x, int y)>(pointCount); // 预分配容量

            //增加一个margin 5的边框 不然掩码贴着边框不好看
            int xMin = 5;
            int xMax = imgWidth - rectWidth - xMin;
            int yMin = 5;
            int yMax = imgHeight - rectHeight - yMin;

            // 添加最大尝试次数，防止无限循环
            int maxAttempts = pointCount * 100;
            int attempts = 0;

            while (pointList.Count < pointCount && attempts < maxAttempts)
            {
                attempts++;
                var pos = GetRandomPoint(xMin, xMax, yMin, yMax);

                // 使用Any直接判断，不需要创建中间List
                if (pointList.Any(point => IsOverlapping(pos, point, rectWidth, rectHeight)))
                {
                    continue;
                }

                pointList.Add(pos);
            }

            if (pointList.Count < pointCount)
            {
                throw new InvalidOperationException($"Could not find {pointCount} non-overlapping positions after {maxAttempts} attempts");
            }

            return pointList;
        }

        private (int left, int top) GetRandomPoint(int xMin, int xMax, int yMin, int yMax)
        {
            return (
                Random.Shared.Next(xMin, xMax + 1),
                Random.Shared.Next(yMin, yMax + 1)
            );
        }

        private bool IsOverlapping((int left, int top) pos1, (int left, int top) pos2, int width, int height)
        {
            return !(pos2.left > pos1.left + width ||
                    pos2.left + width < pos1.left ||
                    pos2.top > pos1.top + height ||
                    pos2.top + height < pos1.top);
        }



        private void AddTextWatermark(Mat img, string text)
        {

            // 设置文字属性
            double fontSize = 1.5; // 字体大小
            int thickness = 2;     // 字体粗细
            var color = new Scalar(255, 255, 255); // 白色文字

            // 获取文字的大小
            var font = HersheyFonts.HersheyComplex;
            Size textSize = Cv2.GetTextSize(text, font, fontSize, thickness, out int baseline);

            // 计算文字位置（右下角）
            var textPoint = new Point(
                img.Width - textSize.Width - 20,  // 距离右边距 20 像素
                img.Height - 20                    // 距离底部 20 像素
            );

            // 添加文字
            Cv2.PutText(
                img,            // 图像
                text,           // 文本
                textPoint,      // 位置
                font,           // 字体
                fontSize,       // 字体大小
                color,         // 颜色
                thickness      // 粗细
            );

        }

        // 带更多自定义选项的版本
        private void AddCustomTextWatermark(
            string inputImagePath,
            string outputImagePath,
            string text,
            double fontSize = 1.5,
            int thickness = 2,
            WatermarkPosition position = WatermarkPosition.BottomRight,
            int margin = 20,
            Scalar? color = null)
        {
            using var img = Cv2.ImRead(inputImagePath);

            // 默认白色
            color ??= new Scalar(255, 255, 255);

            // 获取文字大小
            var font = HersheyFonts.HersheyComplex;
            Size textSize = Cv2.GetTextSize(text, font, fontSize, thickness, out int baseline);

            // 计算文字位置
            Point textPoint = CalculatePosition(
                position,
                img.Size(),
                textSize,
                margin
            );

            // 可选：添加半透明背景
            var bgRect = new Rect(
                textPoint.X - 5,
                textPoint.Y - textSize.Height - 5,
                textSize.Width + 10,
                textSize.Height + 10
            );

            // 绘制半透明背景
            using var overlay = img.Clone();
            Cv2.Rectangle(overlay, bgRect, new Scalar(0, 0, 0), -1);
            Cv2.AddWeighted(overlay, 0.5, img, 0.5, 0, img);

            // 添加文字
            Cv2.PutText(img, text, textPoint, font, fontSize, color.Value, thickness);

            // 保存图像
            img.SaveImage(outputImagePath);
        }



        private Point CalculatePosition(
            WatermarkPosition position,
            Size imageSize,
            Size textSize,
            int margin)
        {
            return position switch
            {
                WatermarkPosition.TopLeft => new Point(
                    margin,
                    textSize.Height + margin),

                WatermarkPosition.TopRight => new Point(
                    imageSize.Width - textSize.Width - margin,
                    textSize.Height + margin),

                WatermarkPosition.BottomLeft => new Point(
                    margin,
                    imageSize.Height - margin),

                WatermarkPosition.BottomRight => new Point(
                    imageSize.Width - textSize.Width - margin,
                    imageSize.Height - margin),

                WatermarkPosition.Center => new Point(
                    (imageSize.Width - textSize.Width) / 2,
                    (imageSize.Height + textSize.Height) / 2),

                _ => new Point(margin, margin)
            };
        }


    }
}
