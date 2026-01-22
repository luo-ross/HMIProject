using RS.Win32API;
using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets
{
    public class GlobalKeyboardEventArgs : EventArgs
    {
        public Keys KeyCode { get; }
        public int ScanCode { get; }
        public bool Alt { get; }
        public bool Control { get; }
        public bool Shift { get; }
        public bool IsSystemKey { get; }

        public GlobalKeyboardEventArgs(Keys keyCode, int scanCode, bool isSystemKey)
        {
            KeyCode = keyCode;
            ScanCode = scanCode;
            IsSystemKey = isSystemKey;

            Control = NativeMethods.IsKeyDown(Keys.ControlKey);
            Shift = NativeMethods.IsKeyDown(Keys.ShiftKey);
            Alt = NativeMethods.IsKeyDown(Keys.Menu);
        }

    }
}