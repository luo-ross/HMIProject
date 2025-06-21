using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct BSTRBLOB
    {
        public uint cbSize;
        public IntPtr pData;
    }
}
