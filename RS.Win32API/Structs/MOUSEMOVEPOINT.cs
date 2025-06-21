using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Structs
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)] // For GetMouseMovePointsEx
    public struct MOUSEMOVEPOINT
    {
        public int x;                       //Specifies the x-coordinate of the mouse
        public int y;                       //Specifies the x-coordinate of the mouse
        public int time;                    //Specifies the time stamp of the mouse coordinate
        public IntPtr dwExtraInfo;              //Specifies extra information associated with this coordinate.
    }
}
