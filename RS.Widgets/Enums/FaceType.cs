using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Enums
{
    [Flags]
    public enum FaceType
    {
        None = 0,
        Front = 1 << 0,
        Back = 1 << 1,
    };
}
