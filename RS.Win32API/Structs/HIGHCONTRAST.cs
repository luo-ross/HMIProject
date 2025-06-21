using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{

    [StructLayout(LayoutKind.Sequential)]
    public struct HIGHCONTRAST
    {
        public int cbSize;
        public HCF dwFlags;
        //[MarshalAs(UnmanagedType.LPWStr, SizeConst=80)]
        //public String lpszDefaultScheme;
        public IntPtr lpszDefaultScheme;
    }
}
