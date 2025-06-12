using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RS.Win32API.NativeMethods;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class WNDCLASS_D
    {
        public int style;
        public WndProc lpfnWndProc;
        public int cbClsExtra = 0;
        public int cbWndExtra = 0;
        public IntPtr hInstance = IntPtr.Zero;
        public IntPtr hIcon = IntPtr.Zero;
        public IntPtr hCursor = IntPtr.Zero;
        public IntPtr hbrBackground = IntPtr.Zero;
        public string lpszMenuName = null;
        public string lpszClassName = null;
    }
}
