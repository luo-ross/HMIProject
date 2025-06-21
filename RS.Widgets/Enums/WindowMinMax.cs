using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Enums
{
    public struct WindowMinMax
    {
        public double minWidth;

        public double maxWidth;

        public double minHeight;

        public double maxHeight;

        public WindowMinMax(double minSize, double maxSize)
        {
            minWidth = minSize;
            maxWidth = maxSize;
            minHeight = minSize;
            maxHeight = maxSize;
        }
    }
}
