using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [Flags]
    public enum SPIF
    {
        None = 0,
        UPDATEINIFILE = 0x01,
        SENDCHANGE = 0x02,
        SENDWININICHANGE = SENDCHANGE,
    }
}
