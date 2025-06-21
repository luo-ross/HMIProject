using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    public struct WINDOWPOS
    {
        public IntPtr hwnd;

        public IntPtr hwndInsertAfter;

        public int x;

        public int y;

        public int cx;

        public int cy;

        public int flags;
    }



}
