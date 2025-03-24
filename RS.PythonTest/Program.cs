using Python.Runtime;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;

namespace RS.PythonTest
{
    internal class Program
    {
        public static dynamic cv2 { get; set; }
        public static dynamic np { get; set; }
        public static dynamic Tk { get; set; }
        public static dynamic askopenfilename
        { get; set; }
        static void Main(string[] args)
        {
            try
            {
                //配置自己的python虚拟环境
                string pythonPath = @"D:\anaconda3\envs\ross";
                //虚拟环境下的phython的版本dll
                Runtime.PythonDLL = @$"{pythonPath}\python39.dll";
                PythonEngine.PythonHome = pythonPath;
                PythonEngine.Initialize();
                using (Py.GIL())
                {
                    try
                    {
                        using (var scope = Py.CreateScope())
                        {
                            scope.Exec(@$"import cv2");
                            scope.Exec(@$"import numpy as np");
                            scope.Exec(@$"from tkinter import Tk");
                            scope.Exec(@$"import ctypes");
                            scope.Exec(@$"from tkinter.filedialog import askopenfilename");
                            np = scope.Get("np");
                            Tk = scope.Get("Tk");
                            askopenfilename = scope.Get("askopenfilename");
                            cv2 = scope.Get("cv2");
                            dynamic ctypes = scope.Get("ctypes");
                            // 读取图像
                            dynamic img = cv2.imread(@"D:\Users\Administrator\Desktop\f9198618367adab47e48e6c399d4b31c8601e492.png");
                            // 获取图像信息
                            int height = (int)img.shape[0];
                            int width = (int)img.shape[1];
                            int channels = (int)img.shape[2];
                            int totalSize = img.size;

                            #region 核心代码如何在.NET里通过指针访问Python里的图像数据或者是所有数组数据
                            // 确保数据连续
                            dynamic contiguous_img = np.ascontiguousarray(img);
                            // 获取数据指针------->关键代码
                            long dataPtr = (contiguous_img.__array_interface__["data"][0]).As<long>();
                            // 创建托管数组并复制数据
                            byte[] managedArray = new byte[totalSize];
                            Marshal.Copy(new IntPtr(dataPtr), managedArray, 0, totalSize);
                            #endregion

                            // 拿到Buffer后可以创建Bitmap 或者其他数据
                            Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb);
                            BitmapData bmpData = bitmap.LockBits(
                                new Rectangle(0, 0, width, height),
                                ImageLockMode.WriteOnly,
                                PixelFormat.Format24bppRgb);
                            try
                            {
                                Marshal.Copy(managedArray, 0, bmpData.Scan0, totalSize);
                            }
                            finally
                            {
                                bitmap.UnlockBits(bmpData);
                            }

                            bitmap.Save("test.jpg");
                        }
                    }
                    catch (PythonException ex)
                    {
                       
                    }
                }
            }
            catch (Exception)
            {

            }
        }

        public static dynamic select_image()
        {
            using (Py.GIL())
            {
                using (var scope = Py.CreateScope())
                {
                    scope.Set("Tk", Tk);
                    scope.Set("askopenfilename", askopenfilename);
                    scope.Exec(@$"Tk().withdraw()");
                    scope.Exec(@$"file_path = askopenfilename()");
                    return scope.Get("file_path");
                }
            }
        }


        public static dynamic segment_portrait(dynamic image_path)
        {

            try
            {

                using (Py.GIL())
                {
                    using (var scope = Py.CreateScope())
                    {
                        scope.Set("cv2", cv2);
                        scope.Set("np", np);
                        scope.Set("Tk", Tk);
                        scope.Set("askopenfilename", askopenfilename);
                        scope.Set("image_path", image_path);
                        scope.Exec(@$"img = cv2.imread(image_path)");
                        scope.Exec(@$"mask = np.zeros(img.shape[:2], np.uint8)");
                        var img = scope.Get("img");
                        //if (img == PyObject.None)
                        //{
                        //    return PyObject.None;
                        //}
                        scope.Exec(@$"bgdModel = np.zeros((1, 65), np.float64)");
                        scope.Exec(@$"fgdModel = np.zeros((1, 65), np.float64)");
                        // 定义一个矩形区域，该区域包含前景对象
                        scope.Exec(@$"rect = (50, 50, img.shape[1] - 100, img.shape[0] - 100)");

                        // 应用 GrabCut 算法
                        scope.Exec(@$"cv2.grabCut(img, mask, rect, bgdModel, fgdModel, 1, cv2.GC_INIT_WITH_RECT)");

                        scope.Exec(@$"mask2 = np.where((mask == 2) | (mask == 0), 0, 1).astype('uint8')");

                        // 将图像与掩码相乘，提取前景
                        scope.Exec(@$"img = img * mask2[:, :, np.newaxis]");

                        scope.Exec(@$"img = cv2.resize(img, (450, 450))");

                        return scope.Get("img");

                    }
                }

            }
            catch (Exception)
            {
                return PyObject.None;
            }
        }
    }
}
