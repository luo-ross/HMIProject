using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public struct HIGHCONTRAST_I
    {
        public int cbSize;
        public int dwFlags;
        public IntPtr lpszDefaultScheme;
    }
}
