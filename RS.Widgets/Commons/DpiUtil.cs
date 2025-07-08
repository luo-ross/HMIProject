using Microsoft.Win32.SafeHandles;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.SafeHandles;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Commons
{
    public class DpiUtil
    {
        public static class DpiAwarenessContextHelper
        {
            private static bool IsGetWindowDpiAwarenessContextMethodSupported { get; set; } = true;


            [SecurityCritical]
            internal static DpiAwarenessContextHandle GetDpiAwarenessContext(IntPtr hWnd)
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

            [SecurityCritical]
            private static DpiAwarenessContextHandle GetProcessDpiAwarenessContext(IntPtr hWnd)
            {
                PROCESS_DPI_AWARENESS processDpiAwareness = ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
                return GetProcessDpiAwarenessContext(processDpiAwareness);
            }

            internal static DpiAwarenessContextHandle GetProcessDpiAwarenessContext(PROCESS_DPI_AWARENESS dpiAwareness)
            {
                return dpiAwareness switch
                {
                    PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE => NativeMethods.DPI_AWARENESS_CONTEXT_SYSTEM_AWARE,
                    PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE => NativeMethods.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE,
                    _ => NativeMethods.DPI_AWARENESS_CONTEXT_UNAWARE,
                };
            }

            private static DpiAwarenessContextHandle GetWindowDpiAwarenessContext(IntPtr hWnd)
            {
                return NativeMethods.GetWindowDpiAwarenessContext(hWnd);
            }
        }

        public class DpiAwarenessScope : IDisposable
        {
            private static bool OperationSupported { get; set; } = true;


            private bool IsThreadInMixedHostingBehavior
            {
                get
                {
                    return NativeMethods.GetThreadDpiHostingBehavior() == DPI_HOSTING_BEHAVIOR.DPI_HOSTING_BEHAVIOR_MIXED;
                }
            }

            private DpiAwarenessContextHandle OldDpiAwarenessContext { get; set; }

            public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue)
                : this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode: false, updateIfWindowIsSystemAwareOrUnaware: false, IntPtr.Zero)
            {
            }

            public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue, bool updateIfThreadInMixedHostingMode)
                : this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode, updateIfWindowIsSystemAwareOrUnaware: false, IntPtr.Zero)
            {
            }

            public DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextEnumValue, bool updateIfThreadInMixedHostingMode, IntPtr hWnd)
                : this(dpiAwarenessContextEnumValue, updateIfThreadInMixedHostingMode, updateIfWindowIsSystemAwareOrUnaware: true, hWnd)
            {
            }


            private DpiAwarenessScope(DpiAwarenessContextValue dpiAwarenessContextValue, bool updateIfThreadInMixedHostingMode, bool updateIfWindowIsSystemAwareOrUnaware, IntPtr hWnd)
            {
                if (dpiAwarenessContextValue == DpiAwarenessContextValue.Invalid || !OperationSupported || (updateIfThreadInMixedHostingMode && !IsThreadInMixedHostingBehavior) || (updateIfWindowIsSystemAwareOrUnaware && (hWnd == IntPtr.Zero || !IsWindowUnawareOrSystemAware(hWnd))))
                {
                    return;
                }

                try
                {
                    OldDpiAwarenessContext = NativeMethods.SetThreadDpiAwarenessContext(new DpiAwarenessContextHandle(dpiAwarenessContextValue));
                }
                catch (Exception ex) when (ex is EntryPointNotFoundException || ex is MissingMethodException || ex is DllNotFoundException)
                {
                    OperationSupported = false;
                }
            }


            public void Dispose()
            {
                if (OldDpiAwarenessContext != null)
                {
                    NativeMethods.SetThreadDpiAwarenessContext(OldDpiAwarenessContext);
                    OldDpiAwarenessContext = null;
                }
            }


            private bool IsWindowUnawareOrSystemAware(IntPtr hWnd)
            {
                DpiAwarenessContextHandle dpiAwarenessContext = GetDpiAwarenessContext(hWnd);
                if (!dpiAwarenessContext.Equals(DpiAwarenessContextValue.Unaware))
                {
                    return dpiAwarenessContext.Equals(DpiAwarenessContextValue.SystemAware);
                }

                return true;
            }
        }

        public class HwndDpiInfo : Tuple<DpiAwarenessContextValue, DpiScale2>
        {
            internal RECT ContainingMonitorScreenRect { get; }

            internal DpiAwarenessContextValue DpiAwarenessContextValue => base.Item1;

            internal DpiScale2 DpiScale => base.Item2;


            internal HwndDpiInfo(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
                : base((DpiAwarenessContextValue)GetDpiAwarenessContext(hWnd), GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic))
            {
                ContainingMonitorScreenRect = NearestMonitorInfoFromWindow(hWnd).rcMonitor;
            }

            internal HwndDpiInfo(DpiAwarenessContextValue dpiAwarenessContextValue, DpiScale2 dpiScale)
                : base(dpiAwarenessContextValue, dpiScale)
            {
                ContainingMonitorScreenRect = NearestMonitorInfoFromWindow(IntPtr.Zero).rcMonitor;
            }


            private static MONITORINFOEX NearestMonitorInfoFromWindow(IntPtr hwnd)
            {
                IntPtr intPtr = NativeMethods.MonitorFromWindow(new HandleRef(null, hwnd), 2);
                if (intPtr == IntPtr.Zero)
                {
                    throw new Win32Exception();
                }

                MONITORINFOEX mONITORINFOEX = new MONITORINFOEX();
                NativeMethods.GetMonitorInfo(new HandleRef(null, intPtr), mONITORINFOEX);
                return mONITORINFOEX;
            }
        }

        private static class ProcessDpiAwarenessHelper
        {
            private static bool IsGetProcessDpiAwarenessFunctionSupported { get; set; } = true;



            internal static PROCESS_DPI_AWARENESS GetLegacyProcessDpiAwareness()
            {
                if (!NativeMethods.IsProcessDPIAware())
                {
                    return PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
                }

                return PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE;
            }


            internal static PROCESS_DPI_AWARENESS GetProcessDpiAwareness(IntPtr hWnd)
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


            private static PROCESS_DPI_AWARENESS GetProcessDpiAwarenessFromWindow(IntPtr hWnd)
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

        private static class SystemDpiHelper
        {
            public static object DpiLock;
            public static List<double> DpiScaleXValues;

            public static List<double> DpiScaleYValues;

            private static double _dpiScaleX;

            private static double _dpiScaleY;
            static SystemDpiHelper()
            {
                DpiScaleXValues = new List<double>(3);
                DpiScaleYValues = new List<double>(3);
                DpiLock = new object();
                _dpiScaleX = 1.0;
                _dpiScaleY = 1.0;
            }
            private static bool IsGetDpiForSystemFunctionAvailable { get; set; } = true;


            internal static DpiScale2 GetSystemDpi()
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

            internal static DpiScale2 GetSystemDpiFromUIElementCache()
            {
                lock (SystemDpiHelper.DpiLock)
                {
                    return new DpiScale2(SystemDpiHelper.DpiScaleXValues[0], SystemDpiHelper.DpiScaleYValues[0]);
                }
            }

            internal static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
            {
                lock (SystemDpiHelper.DpiLock)
                {
                    SystemDpiHelper.DpiScaleXValues.Insert(0, systemDpiScale.DpiScaleX);
                    SystemDpiHelper.DpiScaleYValues.Insert(0, systemDpiScale.DpiScaleY);
                }
            }

            private static DpiScale2 GetDpiForSystem()
            {
                uint dpiForSystem = NativeMethods.GetDpiForSystem();
                return DpiScale2.FromPixelsPerInch(dpiForSystem, dpiForSystem);
            }


            private static DpiScale2 GetSystemDpiFromDeviceCaps()
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

        private static class WindowDpiScaleHelper
        {
            private static bool IsGetDpiForWindowFunctionEnabled { get; set; } = true;


            internal static DpiScale2 GetWindowDpi(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
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

            private static DpiScale2 GetDpiForWindow(IntPtr hWnd)
            {
                uint dpiForWindow = NativeMethods.GetDpiForWindow(new HandleRef(IntPtr.Zero, hWnd));
                return DpiScale2.FromPixelsPerInch(dpiForWindow, dpiForWindow);
            }


            private static DpiScale2 GetDpiForWindowFromNearestMonitor(IntPtr hWnd)
            {
                IntPtr handle = NativeMethods.MonitorFromWindow(new HandleRef(IntPtr.Zero, hWnd), 2);
                uint dpiX;
                uint dpiY;
                int dpiForMonitor = (int)NativeMethods.GetDpiForMonitor(new HandleRef(IntPtr.Zero, handle), MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
                Marshal.ThrowExceptionForHR(dpiForMonitor);
                return DpiScale2.FromPixelsPerInch(dpiX, dpiY);
            }
        }

        internal const double DefaultPixelsPerInch = 96.0;

        [SecurityCritical]
        internal static DpiAwarenessContextHandle GetDpiAwarenessContext(IntPtr hWnd)
        {
            return DpiAwarenessContextHelper.GetDpiAwarenessContext(hWnd);
        }


        internal static PROCESS_DPI_AWARENESS GetProcessDpiAwareness(IntPtr hWnd)
        {
            return ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
        }

        internal static DpiAwarenessContextValue GetProcessDpiAwarenessContextValue(IntPtr hWnd)
        {
            PROCESS_DPI_AWARENESS processDpiAwareness = ProcessDpiAwarenessHelper.GetProcessDpiAwareness(hWnd);
            return (DpiAwarenessContextValue)DpiAwarenessContextHelper.GetProcessDpiAwarenessContext(processDpiAwareness);
        }

        internal static PROCESS_DPI_AWARENESS GetLegacyProcessDpiAwareness()
        {
            return ProcessDpiAwarenessHelper.GetLegacyProcessDpiAwareness();
        }

        internal static DpiScale2 GetSystemDpi()
        {
            return SystemDpiHelper.GetSystemDpi();
        }

        internal static DpiScale2 GetSystemDpiFromUIElementCache()
        {
            return SystemDpiHelper.GetSystemDpiFromUIElementCache();
        }

        internal static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
        {
            SystemDpiHelper.UpdateUIElementCacheForSystemDpi(systemDpiScale);
        }

        internal static DpiScale2 GetWindowDpi(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
        {
            return WindowDpiScaleHelper.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic);
        }


        internal static HwndDpiInfo GetExtendedDpiInfoForWindow(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
        {
            return new HwndDpiInfo(hWnd, fallbackToNearestMonitorHeuristic);
        }


        internal static HwndDpiInfo GetExtendedDpiInfoForWindow(IntPtr hWnd)
        {
            return GetExtendedDpiInfoForWindow(hWnd, fallbackToNearestMonitorHeuristic: true);
        }

        internal static IDisposable WithDpiAwarenessContext(DpiAwarenessContextValue dpiAwarenessContext)
        {
            return new DpiAwarenessScope(dpiAwarenessContext);
        }

        internal static DpiFlags UpdateDpiScalesAndGetIndex(double pixelsPerInchX, double pixelsPerInchY)
        {
            lock (SystemDpiHelper.DpiLock)
            {
                int num = 0;
                int count = SystemDpiHelper.DpiScaleXValues.Count;
                for (num = 0; num < count && (SystemDpiHelper.DpiScaleXValues[num] != pixelsPerInchX / 96.0 || SystemDpiHelper.DpiScaleYValues[num] != pixelsPerInchY / 96.0); num++)
                {
                }

                if (num == count)
                {
                    SystemDpiHelper.DpiScaleXValues.Add(pixelsPerInchX / 96.0);
                    SystemDpiHelper.DpiScaleYValues.Add(pixelsPerInchY / 96.0);
                }

                bool dpiScaleFlag;
                bool dpiScaleFlag2;
                if (num < 3)
                {
                    dpiScaleFlag = (num & 1) != 0;
                    dpiScaleFlag2 = (num & 2) != 0;
                }
                else
                {
                    dpiScaleFlag = (dpiScaleFlag2 = true);
                }

                return new DpiFlags(dpiScaleFlag, dpiScaleFlag2, num);
            }
        }
    }
}
