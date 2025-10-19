using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Drawing;
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
        private int left;
        private int top;
        private int right;
        private int bottom;

        public RECT(int left, int top, int right, int bottom)
        {
            this.Left = left;
            this.Top = top;
            this.Right = right;
            this.Bottom = bottom;
        }

        public int Width
        {
            get { return Right - Left; }
        }

        public int Height
        {
            get { return Bottom - Top; }
        }

        public void Offset(int dx, int dy)
        {
            Left += dx;
            Top += dy;
            Right += dx;
            Bottom += dy;
        }

        public bool IsEmpty
        {
            get
            {
                return Left >= Right || Top >= Bottom;
            }
        }


        public RECT(System.Drawing.Rectangle r)
        {
            this.Left = r.Left;
            this.Top = r.Top;
            this.Right = r.Right;
            this.Bottom = r.Bottom;
        }

        public static RECT FromXYWH(int x, int y, int width, int height)
        {
            return new RECT(x, y, x + width, y + height);
        }


        public int Left
        {
            get { return left; }
            set { left = value; }
        }


        public int Right
        {
            get { return right; }
            set { right = value; }
        }


        public int Top
        {
            get { return top; }
            set { top = value; }
        }


        public int Bottom
        {
            get { return bottom; }
            set { bottom = value; }
        }




        public POINT Position
        {
            get { return new POINT { X = Left, Y = Top }; }
        }


        public SIZE Size
        {
            get { return new SIZE { Width = Width, Height = Height }; }
        }

        public static RECT Union(RECT rect1, RECT rect2)
        {
            return new RECT
            {
                Left = Math.Min(rect1.Left, rect2.Left),
                Top = Math.Min(rect1.Top, rect2.Top),
                Right = Math.Max(rect1.Right, rect2.Right),
                Bottom = Math.Max(rect1.Bottom, rect2.Bottom),
            };
        }

        public override bool Equals(object obj)
        {
            try
            {
                var rc = (RECT)obj;
                return rc.Bottom == Bottom
                && rc.Left == Left
                    && rc.Right == Right
                    && rc.Top == Top;
            }
            catch (InvalidCastException)
            {
                return false;
            }
        }

        public override int GetHashCode()
        {
            return (left << 16 | SystemUtility.LOWORD(right)) ^ (top << 16 | SystemUtility.LOWORD(bottom));
        }


        public bool IsPointInRect(POINT point)
        {
            return point.X >= this.Left &&
                   point.X <= this.Right &&
                   point.Y >= this.Top &&
                   point.Y <= this.Bottom;
        }

        public override string ToString()
        {
            return $"Left:{Left} Top:{Top} Width:{Width} Height:{Height}";
        }
    }
}
