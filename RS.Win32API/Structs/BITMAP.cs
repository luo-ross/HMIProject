using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class BITMAP
    {
        public int bmType;

        public int bmWidth;

        public int bmHeight;

        public int bmWidthBytes;

        public short bmPlanes;

        public short bmBitsPixel;

        public int bmBits;
    }

}
