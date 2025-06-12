using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class TPMPARAMS
    {
        public int cbSize = Marshal.SizeOf(typeof(TPMPARAMS));
        // rcExclude was a by-value RECT structure
        public int rcExclude_left;
        public int rcExclude_top;
        public int rcExclude_right;
        public int rcExclude_bottom;
    }
}
