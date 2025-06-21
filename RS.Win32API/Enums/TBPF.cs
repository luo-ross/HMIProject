using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Enums
{
    [Flags]
    public enum TBPF
    {
        NOPROGRESS = 0x00000000,
        INDETERMINATE = 0x00000001,
        NORMAL = 0x00000002,
        ERROR = 0x00000004,
        PAUSED = 0x00000008,
    }
}
