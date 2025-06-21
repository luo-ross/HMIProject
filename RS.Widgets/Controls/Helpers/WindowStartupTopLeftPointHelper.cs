using RS.Widgets.Interop;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

namespace RS.Widgets.Controls
{
    public class WindowStartupTopLeftPointHelper
    {
        private static readonly object s_lockObject;
        private static PROCESS_DPI_AWARENESS? AppManifestProcessDpiAwareness { get; set; }
        private static PROCESS_DPI_AWARENESS? ProcessDpiAwareness { get; set; }
        private DpiAwarenessContextValue DpiAwarenessContext { get; set; }
        private nint _hWnd;
        private DpiScale2 _currentDpiScale;
        static WindowStartupTopLeftPointHelper()
        {
            s_lockObject = new object();
        }
        public WindowStartupTopLeftPointHelper(nint hwnd)
        {
            _hWnd = HWND.Cast(hwnd);
            InitializeDpiAwarenessAndDpiScales();
        }

        private void InitializeDpiAwarenessAndDpiScales()
        {
            lock (s_lockObject)
            {
                if (!AppManifestProcessDpiAwareness.HasValue)
                {
                    GetProcessDpiAwareness(_hWnd, out var appManifestProcessDpiAwareness, out var processDpiAwareness);
                    AppManifestProcessDpiAwareness = appManifestProcessDpiAwareness;
                    ProcessDpiAwareness = processDpiAwareness;
                    DpiHelper.UpdateUIElementCacheForSystemDpi(DpiHelper.GetSystemDpi());
                }
            }

            DpiAwarenessContext = (DpiAwarenessContextValue)DpiHelper.GetDpiAwarenessContext(_hWnd);
            _currentDpiScale = GetDpiScaleForWindow(_hWnd);
        }

        public static bool? IsProcessSystemAware
        {
            get
            {
                if (ProcessDpiAwareness.HasValue)
                {
                    return ProcessDpiAwareness.Value == PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE;
                }

                return null;
            }
        }

        public static bool? IsProcessUnaware
        {
            get
            {
                if (ProcessDpiAwareness.HasValue)
                {
                    return ProcessDpiAwareness.Value == PROCESS_DPI_AWARENESS.PROCESS_DPI_UNAWARE;
                }

                return null;
            }
        }


        private static DpiScale2 GetDpiScaleForWindow(nint hWnd)
        {
            DpiScale2 dpiScale = null;
            if (IsPerMonitorDpiScalingEnabled)
            {
                dpiScale = DpiHelper.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic: false);
            }
            else if (ProcessDpiAwareness.HasValue)
            {
                if (IsProcessSystemAware == true)
                {
                    dpiScale = DpiHelper.GetSystemDpiFromUIElementCache();
                }
                else if (IsProcessUnaware == true)
                {
                    dpiScale = DpiScale2.FromPixelsPerInch(96.0, 96.0);
                }
            }

            if (dpiScale == null)
            {
                dpiScale = DpiHelper.GetLegacyProcessDpiAwareness() switch
                {
                    PROCESS_DPI_AWARENESS.PROCESS_SYSTEM_DPI_AWARE => DpiHelper.GetSystemDpi(),
                    PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE => IsPerMonitorDpiScalingEnabled ? DpiHelper.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic: false) : DpiHelper.GetSystemDpi(),
                    _ => DpiScale2.FromPixelsPerInch(96.0, 96.0),
                };
            }

            return dpiScale;
        }


        private static void GetProcessDpiAwareness(nint hWnd, out PROCESS_DPI_AWARENESS appManifestProcessDpiAwareness, out PROCESS_DPI_AWARENESS processDpiAwareness)
        {
            appManifestProcessDpiAwareness = DpiHelper.GetProcessDpiAwareness(hWnd);
            if (IsPerMonitorDpiScalingEnabled)
            {
                processDpiAwareness = appManifestProcessDpiAwareness;
            }
            else
            {
                processDpiAwareness = DpiHelper.GetLegacyProcessDpiAwareness();
            }
        }

        public static bool IsPerMonitorDpiScalingSupportedOnCurrentPlatform => OSVersionHelper.IsOsWindows10RS1OrGreater;
        public static bool IsPerMonitorDpiScalingEnabled
        {
            get
            {
                if (!CoreAppContextSwitches.DoNotScaleForDpiChanges)
                {
                    return IsPerMonitorDpiScalingSupportedOnCurrentPlatform;
                }

                return false;
            }
        }



        public Point LogicalTopLeft { get; }

        public Point? ScreenTopLeft { get; private set; }

        private bool IsHelperNeeded
        {
            [SecuritySafeCritical]
            get
            {
                if (CoreAppContextSwitches.DoNotUsePresentationDpiCapabilityTier2OrGreater)
                {
                    return false;
                }

                if (!IsPerMonitorDpiScalingEnabled)
                {
                    return false;
                }

                if (IsProcessPerMonitorDpiAware.HasValue)
                {
                    return IsProcessPerMonitorDpiAware.Value;
                }

                return DpiHelper.GetProcessDpiAwareness(nint.Zero) == PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
            }
        }

        public static bool? IsProcessPerMonitorDpiAware
        {
            get
            {
                if (ProcessDpiAwareness.HasValue)
                {
                    return ProcessDpiAwareness.Value == PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
                }

                return null;
            }
        }

        public WindowStartupTopLeftPointHelper(Point topLeft)
        {
            LogicalTopLeft = topLeft;
            if (IsHelperNeeded)
            {
                IdentifyScreenTopLeft();
            }
        }

        [SecuritySafeCritical]
        private void IdentifyScreenTopLeft()
        {
            HandleRef hWnd = new HandleRef(null, nint.Zero);
            nint dC = NativeMethods.GetDC(hWnd);
            NativeMethods.EnumDisplayMonitors(dC, nint.Zero, MonitorEnumProc, nint.Zero);
            NativeMethods.ReleaseDC(hWnd, new HandleRef(null, dC));
        }

       
        private bool MonitorEnumProc(nint hMonitor, nint hdcMonitor, ref RECT lprcMonitor, nint dwData)
        {
            bool result = true;
            if (NativeMethods.GetDpiForMonitor(new HandleRef(null, hMonitor), MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out var dpiY) == 0)
            {
                double num = dpiX * 1.0 / 96.0;
                double num2 = dpiY * 1.0 / 96.0;
                Rect rect = default;
                rect.X = lprcMonitor.Left / num;
                rect.Y = lprcMonitor.Top / num2;
                rect.Width = (lprcMonitor.Right - lprcMonitor.Left) / num;
                rect.Height = (lprcMonitor.Bottom - lprcMonitor.Top) / num2;
                Rect rect2 = rect;
                if (rect2.Contains(LogicalTopLeft))
                {
                    ScreenTopLeft = new Point
                    {
                        X = LogicalTopLeft.X * num,
                        Y = LogicalTopLeft.Y * num2
                    };
                    result = false;
                }
            }

            return result;
        }
    }

}
