using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using static RS.Win32API.NativeMethods;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public struct WNDCLASSEX_D
    {
        public WNDCLASSEX_D()
        {
            
        }
        public int cbSize = 0;

        public int style = 0;

        public WndProc lpfnWndProc = null;

        public int cbClsExtra = 0;

        public int cbWndExtra = 0;

        public IntPtr hInstance = IntPtr.Zero;

        public IntPtr hIcon = IntPtr.Zero;

        public IntPtr hCursor = IntPtr.Zero;

        public IntPtr hbrBackground = IntPtr.Zero;

        public string lpszMenuName = null;

        public string lpszClassName = null;

        public IntPtr hIconSm = IntPtr.Zero;
    }

}
