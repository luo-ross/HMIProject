using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace RS.Widgets
{
    public class GlobalMouseEventArgs : EventArgs
    {
        public int X { get; }
        public int Y { get; }
        public MouseButton Button { get; }
        public int Clicks { get; }
        public int Delta { get; }
        public Point Location => new Point(X, Y);

        public GlobalMouseEventArgs(int x, int y)
        {
            X = x;
            Y = y;
        }

        public GlobalMouseEventArgs(int x, int y, MouseButton button) : this(x, y)
        {
            Button = button;
        }

        public GlobalMouseEventArgs(int x, int y, MouseButton button, int clicks) : this(x, y, button)
        {
            Clicks = clicks;
        }

        public GlobalMouseEventArgs(int x, int y, int delta) : this(x, y)
        {
            Delta = delta;
        }
    }
}
