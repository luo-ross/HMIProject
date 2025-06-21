using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, Pack = 0)]
    public struct PROPVARIANT
    {
        /// <summary>
        /// Variant type
        /// </summary>
        public VARTYPE vt;

        /// <summary>
        /// unused
        /// </summary>
        public ushort wReserved1;

        /// <summary>
        /// unused
        /// </summary>
        public ushort wReserved2;

        /// <summary>
        /// unused
        /// </summary>
        public ushort wReserved3;

        /// <summary>
        /// union where the actual variant value lives
        /// </summary>
        public PropVariantUnion union;
    }
}
