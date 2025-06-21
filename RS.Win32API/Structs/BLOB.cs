using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    /// <summary>
    /// BLOB, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct BLOB
    {
        public uint cbSize;
        public IntPtr pBlobData;
    }
}
