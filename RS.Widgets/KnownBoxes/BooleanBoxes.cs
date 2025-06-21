using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.KnownBoxes
{
    public static class BooleanBoxes
    {
        public static object TrueBox = true;

        public static object FalseBox = false;
        public static object Box(bool value)
        {
            if (value)
            {
                return TrueBox;
            }
            return FalseBox;
        }
    }
}
