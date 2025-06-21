using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    /// <summary>
    /// THUMBBUTTON flags.  THBF_*
    /// </summary>
    [Flags]
    public enum THBF : uint
    {
        ENABLED = 0x0000,
        DISABLED = 0x0001,
        DISMISSONCLICK = 0x0002,
        NOBACKGROUND = 0x0004,
        HIDDEN = 0x0008,
        // Added post-beta
        NONINTERACTIVE = 0x0010,
    }
}
