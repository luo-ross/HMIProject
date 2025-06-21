using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    public class DpiFlags
    {
        public bool DpiScaleFlag1 { get; set; }

        public bool DpiScaleFlag2 { get; set; }

        public int Index { get; set; }

        public DpiFlags(bool dpiScaleFlag1, bool dpiScaleFlag2, int index)
        {
            DpiScaleFlag1 = dpiScaleFlag1;
            DpiScaleFlag2 = dpiScaleFlag2;
            Index = index;
        }
    }
}
