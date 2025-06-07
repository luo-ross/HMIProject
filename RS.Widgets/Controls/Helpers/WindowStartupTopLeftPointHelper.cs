using ScottPlot.Colormaps;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shell;
using Windows.Win32;
using Windows.Win32.UI.WindowsAndMessaging;
using Windows.Win32.Graphics.Gdi;
using RS.Win32API;
using System.Windows.Documents;
using System.Windows.Threading;
using Windows.Win32.Foundation;
using System.Globalization;
using CommunityToolkit.HighPerformance;
using NPOI.POIFS.Properties;
using System.Windows.Interop;
using System.Security;
using ZXing.Client.Result;
using Microsoft.Win32.SafeHandles;
using Windows.Win32.System.Threading;
using Windows.Win32.UI.HiDpi;

namespace RS.Widgets.Controls.Helpers
{
    public class WindowStartupTopLeftPointHelper
    {
        internal Point LogicalTopLeft { get; }

        internal Point? ScreenTopLeft { get; private set; }

        private bool IsHelperNeeded
        {
            [SecuritySafeCritical]
            get
            {
                //if (CoreAppContextSwitches.DoNotUsePresentationDpiCapabilityTier2OrGreater)
                //{
                //    return false;
                //}

                //if (!HwndTarget.IsPerMonitorDpiScalingEnabled)
                //{
                //    return false;
                //}

                //if (HwndTarget.IsProcessPerMonitorDpiAware.HasValue)
                //{
                //    return HwndTarget.IsProcessPerMonitorDpiAware.Value;
                //}

                return GetProcessDpiAwarenessFromWindow(IntPtr.Zero) == PROCESS_DPI_AWARENESS.PROCESS_PER_MONITOR_DPI_AWARE;
            }
        }



        [SecuritySafeCritical]
        private unsafe PROCESS_DPI_AWARENESS GetProcessDpiAwarenessFromWindow(IntPtr hWnd)
        {
            uint lpdwProcessId = 0;
            if (hWnd != IntPtr.Zero)
            {
                Ross.GetWindowThreadProcessId(new HWND(hWnd), &lpdwProcessId);
            }
            else
            {
                lpdwProcessId = Ross.GetCurrentProcessId();
            }
            var _PROCESS_ACCESS_RIGHTS = (PROCESS_ACCESS_RIGHTS)2035711;
            var openProcessHandle = Ross.OpenProcess(_PROCESS_ACCESS_RIGHTS, false, lpdwProcessId);
            using SafeProcessHandle safeProcessHandle = new SafeProcessHandle(openProcessHandle, ownsHandle: true);
            return GetProcessDpiAwareness(safeProcessHandle);
        }

        private PROCESS_DPI_AWARENESS GetProcessDpiAwareness(SafeHandle hProcess)
        {

            var processDpiAwareness = Ross.GetProcessDpiAwareness(hProcess, out PROCESS_DPI_AWARENESS awareness);
            if (processDpiAwareness != 0)
            {
                Marshal.ThrowExceptionForHR(processDpiAwareness);
            }
            return awareness;
        }



        internal WindowStartupTopLeftPointHelper(Point topLeft)
        {
            LogicalTopLeft = topLeft;
            if (IsHelperNeeded)
            {
                IdentifyScreenTopLeft();
            }
        }

        [SecuritySafeCritical]
        private unsafe void IdentifyScreenTopLeft()
        {
            var hWnd = new HWND(IntPtr.Zero);
            HDC _HDC = Ross.GetDC(new HWND(IntPtr.Zero));
            SafeProcessHandle safeProcessHandle = new SafeProcessHandle(_HDC.Value, true);
            Ross.EnumDisplayMonitors(safeProcessHandle, default, MonitorEnumProc, IntPtr.Zero);
            Ross.ReleaseDC(hWnd, _HDC);
        }


        [SecurityCritical]
        private unsafe BOOL MonitorEnumProc(HMONITOR hMonitor, HDC hdcMonitor, RECT* lprcMonitor, LPARAM dwData)
        {
            bool result = true;

            if (Ross.GetDpiForMonitor(hMonitor, MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out var dpiX, out var dpiY) == 0)
            {
                double num = (double)dpiX * 1.0 / 96.0;
                double num2 = (double)dpiY * 1.0 / 96.0;
                Rect rect = default(Rect);
                rect.X = (double)lprcMonitor->left / num;
                rect.Y = (double)lprcMonitor->top / num2;
                rect.Width = (double)(lprcMonitor->right - lprcMonitor->left) / num;
                rect.Height = (double)(lprcMonitor->bottom - lprcMonitor->top) / num2;
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
