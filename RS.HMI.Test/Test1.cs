using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.IdentityModel.Tokens;
using OpenCvSharp;
using OpenCvSharp.Internal.Vectors;
using System.Diagnostics;
using System.Xml.Schema;

namespace RS.HMI.Test
{
    [TestClass]
    public sealed class Test1
    {
        [TestMethod]
        public void Str()
        {
            string testStr = "sdf       sd sdf sd  sfsd  sdfsd dsfsf sf";
            string result = Test(testStr, 5);
            result = Test(testStr, 8);
            result = Test(testStr, 4);
            result = Test(testStr, 10);
            result = Test(testStr, 15);
            result = Test(testStr, 50);
            Console.ReadLine();
        }


        public string Test(string s, int len)
        {
            if (len > s.Length)
            {
                return s.Trim();
            }
            if (len <= 0)
            {
                return null;
            }
            var skipData = s.Skip(len).Take(s.Length - len).ToList();
            int firstSpaceIndex = skipData.IndexOf(' ');
            string result = new string(s.Take(len + firstSpaceIndex).ToArray());
            Debug.WriteLine($"截取长度{len}:{result}");
            return result;
        }


        [TestMethod]
        public void TestMethod1()
        {
            string iconsDir = "D:\\Users\\Administrator\\Desktop\\VerifyImgs\\Icons";
            string verifyImgDir = "D:\\Users\\Administrator\\Desktop\\VerifyImgs";
            var iconsList = Directory.GetFiles(iconsDir);
            var iconPath = iconsList[Random.Shared.Next(iconsList.Length)];
            var iconMat = Cv2.ImRead(iconPath);

            var iconMatGray = iconMat.CvtColor(ColorConversionCodes.BGR2GRAY);
            var iconMask = iconMatGray.Threshold(127, 255, ThresholdTypes.Binary);

            var fileList = Directory.GetFiles(verifyImgDir);
            var verifyImgPath = fileList[Random.Shared.Next(fileList.Length)];
            var imgMat = Cv2.ImRead(verifyImgPath);

            var sliderBtnWidth = 50;
            var sliderBtnHeight = 50;
            var imgWidth = imgMat.Width;
            var imgHeight = imgMat.Height;

            iconMask = iconMask.Resize(new Size(sliderBtnWidth, sliderBtnHeight));

            //然后随机获取另外2个坐标点
            var positionList = GetRandomPosition(imgMat.Width, imgMat.Height, sliderBtnWidth, sliderBtnHeight, 3);

            var firstPosition = positionList.First();
            Rect roiConfirm = new Rect(firstPosition.left, firstPosition.top, sliderBtnWidth, sliderBtnHeight);

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

            for (int i = 0; i < positionList.Count; i++)
            {
                var position = positionList[i];
                Rect roi = new Rect(position.left, position.top, sliderBtnWidth, sliderBtnHeight);
                // 这里把颜色也随机 一定程度可以让人琢磨不清

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

            Console.ReadLine();
        }

        public static Mat RotateHeart(Mat maskImage, double angle)
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


        public List<(int left, int top)> GetRandomPosition(int imgWidth, int imgHeight, int rectWidth, int rectHeight, int pointCount)
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

        public (int left, int top) GetRandomPoint(int xMin, int xMax, int yMin, int yMax)
        {
            return (
                Random.Shared.Next(xMin, xMax + 1),
                Random.Shared.Next(yMin, yMax + 1)
            );
        }

        private static bool IsOverlapping((int left, int top) pos1, (int left, int top) pos2, int width, int height)
        {
            // 保持原样，这个实现已经很好了
            return !(pos2.left > pos1.left + width ||
                    pos2.left + width < pos1.left ||
                    pos2.top > pos1.top + height ||
                    pos2.top + height < pos1.top);
        }

    }
}
