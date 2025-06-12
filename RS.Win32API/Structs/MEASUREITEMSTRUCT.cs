using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public class MEASUREITEMSTRUCT
    {
        public int CtlType = 0;
        public int CtlID = 0;
        public int itemID = 0;
        public int itemWidth = 0;
        public int itemHeight = 0;
        public IntPtr itemData = IntPtr.Zero;
    }
}
