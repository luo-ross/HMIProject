using Microsoft.Win32.SafeHandles;
using RS.Win32API.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;

namespace RS.Win32API.Helpers
{
    public static class ProcessDpiAwarenessHelper
    {
        public static bool IsGetProcessDpiAwarenessFunctionSupported { get; set; } = true;

        public static PROCESS_DPI_AWARENESS GetLegacyProcessDpiAwareness()
        {
            if (!NativeMethods.IsProcessDPIAware())
            {
                return PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
            }

            return PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE;
        }

        public static PROCESS_DPI_AWARENESS GetProcessDpiAwareness(IntPtr hWnd)
        {
            if (IsGetProcessDpiAwarenessFunctionSupported)
            {
                try
                {
                    try
                    {
                        return GetProcessDpiAwarenessFromWindow(hWnd);
                    }
                    catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
                    {
                        IsGetProcessDpiAwarenessFunctionSupported = false;
                    }
                }
                catch (Exception ex2) when (ex2 is ArgumentException || ex2 is UnauthorizedAccessException || ex2 is COMException)
                {
                }
            }

            return GetLegacyProcessDpiAwareness();
        }

        public static PROCESS_DPI_AWARENESS GetProcessDpiAwarenessFromWindow(IntPtr hWnd)
        {
            int lpdwProcessId = 0;
            if (hWnd != IntPtr.Zero)
            {
                NativeMethods.GetWindowThreadProcessId(new HandleRef(null, hWnd), out lpdwProcessId);
            }
            else
            {
                lpdwProcessId = NativeMethods.GetCurrentProcessId();
            }

            using SafeProcessHandle safeProcessHandle = new SafeProcessHandle(NativeMethods.OpenProcess(2035711, fInherit: false, lpdwProcessId), ownsHandle: true);
            return NativeMethods.GetProcessDpiAwareness(new HandleRef(null, safeProcessHandle.DangerousGetHandle()));
        }
    }
}
