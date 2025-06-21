using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    /// <summary>
    /// Flags for SetTabProperties.  STPF_*
    /// </summary>
    /// <remarks>The native enum was called STPFLAG.</remarks>
    [Flags]
    public enum STPF
    {
        NONE = 0x00000000,
        USEAPPTHUMBNAILALWAYS = 0x00000001,
        USEAPPTHUMBNAILWHENACTIVE = 0x00000002,
        USEAPPPEEKALWAYS = 0x00000004,
        USEAPPPEEKWHENACTIVE = 0x00000008,
    }
}
