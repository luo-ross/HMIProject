using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using static Windows.Win32.Ross;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
using System.Windows;

namespace Windows.Win32API
{
    public static partial class Ross
    {
        ///<SecurityNote>
        /// Critical - calls SetFocusWrapper (the real PInvoke method)
        ///</SecurityNote>
        [SecurityCritical]
        public static bool TrySetFocus(HandleRef hWnd, ref IntPtr result)
        {
            result = SetFocus(new HWND(hWnd.Handle));
            int errorCode = Marshal.GetLastWin32Error();
            if (result == IntPtr.Zero && errorCode != 0)
            {
                return false;
            }
            return true;
        }

    }
}
