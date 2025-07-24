using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
    public  struct OSVERSIONINFOEX
    {
        public OSVERSIONINFOEX()
        {
                
        }
        public int osVersionInfoSize = SizeOf();
        public int majorVersion = 0;
        public int minorVersion = 0;
        public int buildNumber = 0;
        public int platformId = 0;
        [MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
        public string csdVersion = null;
        public short servicePackMajor = 0;
        public short servicePackMinor = 0;
        public short suiteMask = 0;
        public byte productType = 0;
        public byte reserved = 0;

        /// <SecurityNote>
        ///  Critical : Calls critical Marshal.SizeOf
        ///  Safe     : Calls method with trusted input (well known safe type)
        /// </SecurityNote>
        [SecuritySafeCritical]
        private static int SizeOf()
        {
            return Marshal.SizeOf(typeof(OSVERSIONINFOEX));
        }
    }
}
