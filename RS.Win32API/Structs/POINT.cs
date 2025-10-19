using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class POINT
    {
        public int X;
        public int Y;

        public POINT()
        {
        }

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
        }


#if DEBUG
        public override string ToString()
        {
            return "{x=" + X + ", y=" + Y + "}";
        }
#endif
    }

}
