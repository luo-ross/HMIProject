using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct ANIMATIONINFO
    {
        public ANIMATIONINFO()
        {
            
        }

        public int cbSize = SizeOf();
        public int iMinAnimate = 0;

        /// <SecurityNote>
        ///  Critical : Calls critical Marshal.SizeOf
        ///  Safe     : Calls method with trusted input (well known safe type)
        /// </SecurityNote>
        [SecuritySafeCritical]
        private static int SizeOf()
        {
            return Marshal.SizeOf(typeof(ANIMATIONINFO));
        }
    }
}
