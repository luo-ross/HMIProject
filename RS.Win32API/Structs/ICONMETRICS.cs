using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ICONMETRICS
    {
        public ICONMETRICS()
        {
            
        }
        public int cbSize = SizeOf();
        public int iHorzSpacing = 0;
        public int iVertSpacing = 0;
        public int iTitleWrap = 0;
        [MarshalAs(UnmanagedType.Struct)]
        public LOGFONT? lfFont = null;

        /// <SecurityNote>
        ///  Critical : Calls critical Marshal.SizeOf
        ///  Safe     : Calls method with trusted input (well known safe type)
        /// </SecurityNote>
        private static int SizeOf()
        {
            return Marshal.SizeOf(typeof(ICONMETRICS));
        }
    }
}
