using RS.Win32API.Standard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Win32API.Helpers
{
    public static class SystemDpiHelper
    {
        public static List<double> DpiScaleXValues;

        public static List<double> DpiScaleYValues;
        public static object DpiLock;
        public static bool IsGetDpiForSystemFunctionAvailable { get; set; } = true;
        static SystemDpiHelper()
        {
            DpiLock = new object();
            DpiScaleXValues = new List<double>(3);
            DpiScaleYValues = new List<double>(3);
        }
        public static DpiScale2 GetSystemDpi()
        {
            if (IsGetDpiForSystemFunctionAvailable)
            {
                try
                {
                    return GetDpiForSystem();
                }
                catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
                {
                    IsGetDpiForSystemFunctionAvailable = false;
                }
            }

            return GetSystemDpiFromDeviceCaps();
        }

        public static DpiScale2 GetSystemDpiFromUIElementCache()
        {
            lock (SystemDpiHelper.DpiLock)
            {
                return new DpiScale2(SystemDpiHelper.DpiScaleXValues[0], SystemDpiHelper.DpiScaleYValues[0]);
            }
        }

        public static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
        {
            lock (SystemDpiHelper.DpiLock)
            {
                SystemDpiHelper.DpiScaleXValues.Insert(0, systemDpiScale.DpiScaleX);
                SystemDpiHelper.DpiScaleYValues.Insert(0, systemDpiScale.DpiScaleY);
            }
        }

        public static DpiScale2 GetDpiForSystem()
        {
            uint dpiForSystem = NativeMethods.GetDpiForSystem();
            return DpiScale2.FromPixelsPerInch(dpiForSystem, dpiForSystem);
        }


        public static DpiScale2 GetSystemDpiFromDeviceCaps()
        {
            HandleRef hWnd = new HandleRef(IntPtr.Zero, IntPtr.Zero);
            HandleRef hDC = new HandleRef(IntPtr.Zero, NativeMethods.GetDC(hWnd));
            if (hDC.Handle == IntPtr.Zero)
            {
                return null;
            }

            try
            {
                int deviceCaps = NativeMethods.GetDeviceCaps(hDC, 88);
                int deviceCaps2 = NativeMethods.GetDeviceCaps(hDC, 90);
                return DpiScale2.FromPixelsPerInch(deviceCaps, deviceCaps2);
            }
            finally
            {
                NativeMethods.ReleaseDC(hWnd, hDC);
            }
        }
    }
}
