using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Commons
{
    public unsafe class UnmanagedImageAccessor
    {
        private readonly byte* _imagePtr;
        private readonly int _width;
        private readonly int _height;
        private readonly int _bytesPerPixel;

        public UnmanagedImageAccessor(IntPtr unmanagedPtr, int width, int height, int bytesPerPixel)
        {
            _imagePtr = (byte*)unmanagedPtr.ToPointer();
            _width = width;
            _height = height;
            _bytesPerPixel = bytesPerPixel;
        }
        public byte GetPixel(int x, int y, int channel)
        {
            int offset = (y * _width + x) * _bytesPerPixel + channel;
            return _imagePtr[offset];
        }

        public void SetPixel(int x, int y, int channel, byte value)
        {
            int offset = (y * _width + x) * _bytesPerPixel + channel;
            _imagePtr[offset] = value;
        }

        public Span<byte> GetPixelRow(int y)
        {
            int rowStart = y * _width * _bytesPerPixel;
            return new Span<byte>(_imagePtr + rowStart, _width * _bytesPerPixel);
        }

        [Obsolete("Don't use this function,just for test")]
        public void HowToUseDemo(IntPtr imagePtr, int w, int h, int channels)
        {
            unsafe
            {
                // 创建访问器
                var accessor = new UnmanagedImageAccessor(imagePtr, w, h, channels);
                // 直接操作像素 - 在图像中心画一个红色十字
                int centerX = w / 2;
                int centerY = h / 2;
                // 画水平线
                for (int x = centerX - 50; x < centerX + 50; x++)
                {
                    if (x >= 0 && x < w)
                    {
                        accessor.SetPixel(x, centerY, 0, 0);     // Blue
                        accessor.SetPixel(x, centerY, 1, 0);     // Green  
                        accessor.SetPixel(x, centerY, 2, 255);   // Red
                    }
                }
                // 画垂直线
                for (int y = centerY - 50; y < centerY + 50; y++)
                {
                    if (y >= 0 && y < h)
                    {
                        accessor.SetPixel(centerX, y, 0, 0);     // Blue
                        accessor.SetPixel(centerX, y, 1, 0);     // Green
                        accessor.SetPixel(centerX, y, 2, 255);   // Red
                    }
                }
                // 批量处理整行 - 将第200行设置为红色
                if (200 < h)
                {
                    Span<byte> row = accessor.GetPixelRow(200);
                    for (int x = 0; x < row.Length; x += 3)
                    {
                        row[x] = 0;       // Blue
                        row[x + 1] = 0;   // Green
                        row[x + 2] = 255; // Red
                    }
                }
            }
        }

    }




}
