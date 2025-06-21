using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential)]
    public struct BLENDFUNCTION
    {
        // Must be AC_SRC_OVER
        public AC BlendOp;
        // Must be 0.
        public byte BlendFlags;
        // Alpha transparency between 0 (transparent) - 255 (opaque)
        public byte SourceConstantAlpha;
        // Must be AC_SRC_ALPHA
        public AC AlphaFormat;
    }
}
