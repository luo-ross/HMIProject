using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class TRACKMOUSEEVENT
    {
        public int cbSize = SizeOf();
        public int dwFlags = 0;
        public IntPtr hwndTrack = IntPtr.Zero;
        public int dwHoverTime = 100; // Never set this to field ZERO, or to HOVER_DEFAULT, ever!

        /// <SecurityNote>
        ///  Critical : Calls critical Marshal.SizeOf
        ///  Safe     : Calls method with trusted input (well known safe type)
        /// </SecurityNote>
        private static int SizeOf()
        {
            return Marshal.SizeOf(typeof(TRACKMOUSEEVENT));
        }
    }
}
