using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    /// <summary>
    /// CArray, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct CArray
    {
        public uint cElems;
        public IntPtr pElems;
    }
}
