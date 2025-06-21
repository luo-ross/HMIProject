using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media.Media3D;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class RefRECT
    {
        public int left;

        public int top;

        public int right;

        public int bottom;

        public int Width => right - left;

        public int Height => bottom - top;

        public bool IsEmpty
        {
            get
            {
                if (left < right)
                {
                    return top >= bottom;
                }

                return true;
            }
        }

        public RefRECT()
        {
        }

        public RefRECT(int left, int top, int right, int bottom)
        {
            this.left = left;
            this.top = top;
            this.right = right;
            this.bottom = bottom;
        }

        public void Offset(int dx, int dy)
        {
            left += dx;
            top += dy;
            right += dx;
            bottom += dy;
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


      
    }
}
