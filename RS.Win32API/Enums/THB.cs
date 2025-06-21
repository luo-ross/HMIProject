using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{

    /// <summary>
    /// THUMBBUTTON mask.  THB_*
    /// </summary>
    [Flags]
    public enum THB : uint
    {
        BITMAP = 0x0001,
        ICON = 0x0002,
        TOOLTIP = 0x0004,
        FLAGS = 0x0008,
    }
}
