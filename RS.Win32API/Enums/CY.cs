using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    /// <summary>
    /// CY, used in PropVariantUnion.
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct CY
    {
        public uint Lo;
        public int Hi;
    }
}
