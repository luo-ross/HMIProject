using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct CREATESTRUCT
    {
        public IntPtr lpCreateParams;
        public IntPtr hInstance;
        public IntPtr hMenu;
        public IntPtr hwndParent;
        public int cy;
        public int cx;
        public int y;
        public int x;
        public WS style;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszName;
        [MarshalAs(UnmanagedType.LPWStr)]
        public string lpszClass;
        public WS_EX dwExStyle;
    }
}
