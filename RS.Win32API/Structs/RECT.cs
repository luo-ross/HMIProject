using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    // NOTE:  this replaces the RECT struct in NativeMethodsCLR.cs because it adds an extra method IsEmpty
    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int left;
        public int top;
        public int right;
        public int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public int Width
        {
            get { return right - left; }
        }

        public int Height
        {
            get { return bottom - top; }
        }

        public void Offset(int dx, int dy)
        {
            left += dx;
            top += dy;
            right += dx;
            bottom += dy;
        }

        public bool IsEmpty
        {
            get
            {
                return left >= right || top >= bottom;
            }
        }


        public RECT(System.Drawing.Rectangle r)
        {
            this.left = r.Left;
            this.top = r.Top;
            this.right = r.Right;
            this.bottom = r.Bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }

        public System.Drawing.Size Size
        {
            get
            {
                return new System.Drawing.Size(this.right - this.left, this.bottom - this.top);
            }
        }
    }
}
