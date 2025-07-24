using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.SafeHandles;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;

namespace RS.Win32API.Helper
{
    public static class SystemDpiHelper
    {
        public const double DefaultPixelsPerInch = 96.0;
        public static bool IsAreDpiAwarenessContextsEqualMethodSupported { get; set; } = true;

        public static bool AreDpiAwarenessContextsEqual(IntPtr dpiContextA, IntPtr dpiContextB)
        {
            if (IsAreDpiAwarenessContextsEqualMethodSupported)
            {
                try
                {
                    return NativeMethods.AreDpiAwarenessContextsEqual(dpiContextA, dpiContextB);
                }
                catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
                {
                    IsAreDpiAwarenessContextsEqualMethodSupported = false;
                }
            }

            return AreDpiAwarenessContextsTriviallyEqual(dpiContextA, dpiContextB);
        }

        public static bool AreDpiAwarenessContextsTriviallyEqual(IntPtr dpiContextA, IntPtr dpiContextB)
        {
            return dpiContextA == dpiContextB;
        }




        public static DpiAwarenessContextHandle GetDpiAwarenessContext(IntPtr hWnd)
        {
            return DpiAwarenessContextHelper.GetDpiAwarenessContext(hWnd);
        }


        public static PROCESS_DPI_AWARENESS GetProcessDpiAwareness(IntPtr hWnd)
        {
            return ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
        }

        public static DpiAwarenessContextValue GetProcessDpiAwarenessContextValue(IntPtr hWnd)
        {
            PROCESS_DPI_AWARENESS processDpiAwareness = ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
            return (DpiAwarenessContextValue)DpiAwarenessContextHelper.GetProcessDpiAwarenessContext(processDpiAwareness);
        }

        public static PROCESS_DPI_AWARENESS GetLegacyProcessDpiAwareness()
        {
            return ProcessDpiAwarenessHelper.GetLegacyProcessDpiAwareness();
        }

        public static IDisposable WithDpiAwarenessContext(DpiAwarenessContextValue dpiAwarenessContext)
        {
            return new DpiAwarenessScope(dpiAwarenessContext);
        }
    }
}
