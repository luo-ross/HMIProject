using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MOUSEQUERY
    {
        public uint uMsg;

        public IntPtr wParam;

        public IntPtr lParam;

        public int ptX;

        public int ptY;

        public IntPtr hwnd;
    }

}
