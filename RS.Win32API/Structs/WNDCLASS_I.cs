using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class WNDCLASS_I
    {
        public int style = 0;
        public IntPtr lpfnWndProc = IntPtr.Zero;
        public int cbClsExtra = 0;
        public int cbWndExtra = 0;
        public IntPtr hInstance = IntPtr.Zero;
        public IntPtr hIcon = IntPtr.Zero;
        public IntPtr hCursor = IntPtr.Zero;
        public IntPtr hbrBackground = IntPtr.Zero;
        public IntPtr lpszMenuName = IntPtr.Zero;
        public IntPtr lpszClassName = IntPtr.Zero;
    }
}
