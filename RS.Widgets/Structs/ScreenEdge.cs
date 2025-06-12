using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Structs
{
    /// <summary>
    /// A value that specifies an edge of the screen.
    /// </summary>
    public enum ScreenEdge
    {
        /// <summary>
        /// Undefined
        /// </summary>
        Undefined = -1,
        /// <summary>
        /// Left edge.
        /// </summary>
        Left = 0,
        /// <summary>
        /// Top edge.
        /// </summary>
        Top = 1,
        /// <summary>
        /// Right edge.
        /// </summary>
        Right = 2,
        /// <summary>
        /// Bottom edge.
        /// </summary>
        Bottom = 3
    }
}
