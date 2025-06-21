using RS.Win32API.Enums;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Runtime.InteropServices;

namespace RS.Win32API.Standard
{
    public class HwndDpiInfo : Tuple<DpiAwarenessContextValue, DpiScale2>
    {
        public RECT ContainingMonitorScreenRect { get; }

        public DpiAwarenessContextValue DpiAwarenessContextValue => Item1;

        public DpiScale2 DpiScale => Item2;


        public HwndDpiInfo(nint hWnd, bool fallbackToNearestMonitorHeuristic)
            : base((DpiAwarenessContextValue)DpiHelper.GetDpiAwarenessContext(hWnd), DpiHelper.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic))
        {
            ContainingMonitorScreenRect = NearestMonitorInfoFromWindow(hWnd).rcMonitor;
        }

        public HwndDpiInfo(DpiAwarenessContextValue dpiAwarenessContextValue, DpiScale2 dpiScale)
            : base(dpiAwarenessContextValue, dpiScale)
        {
            ContainingMonitorScreenRect = NearestMonitorInfoFromWindow(nint.Zero).rcMonitor;
        }


        public static MONITORINFOEX NearestMonitorInfoFromWindow(nint hwnd)
        {
            nint intPtr = NativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2);
            if (intPtr == nint.Zero)
            {
                throw new Win32Exception();
            }

            MONITORINFOEX mONITORINFOEX = new MONITORINFOEX();
            NativeMethods.GetMonitorInfo(new HandleRef(null, intPtr), mONITORINFOEX);
            return mONITORINFOEX;
        }
    }
}
