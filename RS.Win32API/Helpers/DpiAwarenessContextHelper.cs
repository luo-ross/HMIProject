using RS.Win32API.Enums;
using RS.Win32API.SafeHandles;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Helpers
{
    public class DpiAwarenessContextHelper
    {

        public static bool IsGetWindowDpiAwarenessContextMethodSupported { get; set; } = true;

        public static DpiAwarenessContextHandle GetDpiAwarenessContext(IntPtr hWnd)
        {
            if (IsGetWindowDpiAwarenessContextMethodSupported)
            {
                try
                {
                    return GetWindowDpiAwarenessContext(hWnd);
                }
                catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
                {
                    IsGetWindowDpiAwarenessContextMethodSupported = false;
                }
            }

            return GetProcessDpiAwarenessContext(hWnd);
        }


        public static DpiAwarenessContextHandle GetProcessDpiAwarenessContext(IntPtr hWnd)
        {
            PROCESS_DPI_AWARENESS processDpiAwareness = ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
            return GetProcessDpiAwarenessContext(processDpiAwareness);
        }

        public static DpiAwarenessContextHandle GetProcessDpiAwarenessContext(PROCESS_DPI_AWARENESS dpiAwareness)
        {
            return dpiAwareness switch
            {
                PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE => NativeMethods.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE,
                PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE => NativeMethods.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE,
                _ => NativeMethods.DPI_AWARENESS_CONTEXT_UNAWARE,
            };
        }

        public static DpiAwarenessContextHandle GetWindowDpiAwarenessContext(IntPtr hWnd)
        {
            return NativeMethods.GetWindowDpiAwarenessContext(hWnd);
        }
    }
}
