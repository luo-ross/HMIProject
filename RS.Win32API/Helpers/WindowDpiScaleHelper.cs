using RS.Win32API.Enums;
using RS.Win32API.Standard;
using System.Runtime.InteropServices;

namespace RS.Win32API.Helpers
{
    public static class WindowDpiScaleHelper
    {
        public static bool IsGetDpiForWindowFunctionEnabled { get; set; } = true;


        public static DpiScale2 GetWindowDpi(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
        {
            if (IsGetDpiForWindowFunctionEnabled)
            {
                try
                {
                    try
                    {
                        return GetDpiForWindow(hWnd);
                    }
                    catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
                    {
                        IsGetDpiForWindowFunctionEnabled = false;
                    }
                }
                catch (Exception ex2) when (ex2 is COMException)
                {
                }
            }

            if (fallbackToNearestMonitorHeuristic)
            {
                try
                {
                    return GetDpiForWindowFromNearestMonitor(hWnd);
                }
                catch (Exception ex3) when (ex3 is COMException)
                {
                }
            }

            return null;
        }

        public static DpiScale2 GetDpiForWindow(IntPtr hWnd)
        {
            uint dpiForWindow = NativeMethods.GetDpiForWindow(new HandleRef(IntPtr.Zero, hWnd));
            return DpiScale2.FromPixelsPerInch(dpiForWindow, dpiForWindow);
        }
        public static DpiScale2 GetDpiForWindowFromNearestMonitor(IntPtr hWnd)
        {
            IntPtr handle = NativeMethods.MonitorFromWindow(new HandleRef(IntPtr.Zero, hWnd), 2);
            uint dpiX;
            uint dpiY;
            int dpiForMonitor = (int)NativeMethods.GetDpiForMonitor(new HandleRef(IntPtr.Zero, handle), MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
            Marshal.ThrowExceptionForHR(dpiForMonitor);
            return DpiScale2.FromPixelsPerInch(dpiX, dpiY);
        }
    }

}
