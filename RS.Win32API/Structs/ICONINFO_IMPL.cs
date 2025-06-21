using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class ICONINFO_IMPL
    {
        public bool fIcon;

        public int xHotspot;

        public int yHotspot;

        public IntPtr hbmMask = IntPtr.Zero;

        public IntPtr hbmColor = IntPtr.Zero;
    }

}
