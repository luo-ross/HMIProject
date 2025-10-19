using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Windows.Win32.Ross;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Gdi;
using Windows.Win32.UI.WindowsAndMessaging;
namespace Windows.Win32API
{
    public static partial class Ross
    {
        public const uint SC_MOUSEMOVE = SC_MOVE + 0x02;
        public static readonly HRESULT ERROR_TIMEOUT = new HRESULT(1460);
        public static readonly HRESULT ERROR_INVALID_WINDOW_HANDLE = new HRESULT(1400);
    }
}
