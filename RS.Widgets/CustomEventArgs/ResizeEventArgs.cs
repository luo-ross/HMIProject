using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Shell;

namespace RS.Widgets.CustomEventArgs
{

    public class ResizeEventArgs : EventArgs
    {
        public ResizeGripDirection Direction { get; }
        public Vector Delta { get; }

        public ResizeEventArgs(ResizeGripDirection direction, Vector delta)
        {
            Direction = direction;
            Delta = delta;
        }
    }
}
