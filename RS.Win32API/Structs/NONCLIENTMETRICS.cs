using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class NONCLIENTMETRICS
    {
        public int cbSize = SizeOf();
        public int iBorderWidth = 0;
        public int iScrollWidth = 0;
        public int iScrollHeight = 0;
        public int iCaptionWidth = 0;
        public int iCaptionHeight = 0;
        [MarshalAs(UnmanagedType.Struct)]
        public LOGFONT lfCaptionFont = null;
        public int iSmCaptionWidth = 0;
        public int iSmCaptionHeight = 0;
        [MarshalAs(UnmanagedType.Struct)]
        public LOGFONT lfSmCaptionFont = null;
        public int iMenuWidth = 0;
        public int iMenuHeight = 0;
        [MarshalAs(UnmanagedType.Struct)]
        public LOGFONT lfMenuFont = null;
        [MarshalAs(UnmanagedType.Struct)]
        public LOGFONT lfStatusFont = null;
        [MarshalAs(UnmanagedType.Struct)]
        public LOGFONT lfMessageFont = null;

        /// <SecurityNote>
        ///  Critical : Calls critical Marshal.SizeOf
        ///  Safe     : Calls method with trusted input (well known safe type)
        /// </SecurityNote>
        private static int SizeOf()
        {
            return Marshal.SizeOf(typeof(NONCLIENTMETRICS));
        }
    }
}
