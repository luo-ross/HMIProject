using RS.Win32API.Enums;
using RS.Win32API.Helpers;
using RS.Win32API.SafeHandles;
using RS.Win32API.Structs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Windows;
using System.Windows.Media;

namespace RS.Win32API.Standard
{
    public static class DpiHelper
    {
        [ThreadStatic]
        private static Matrix _transformToDevice;
        [ThreadStatic]
        private static Matrix _transformToDip;

        /// <summary>
        /// Convert a point in device independent pixels (1/96") to a point in the system coordinates.
        /// </summary>
        /// <param name="logicalPoint">A point in the logical coordinate system.</param>
        /// <returns>Returns the parameter converted to the system's coordinates.</returns>
        public static Point LogicalPixelsToDevice(Point logicalPoint, double dpiScaleX, double dpiScaleY)
        {
            _transformToDevice = Matrix.Identity;
            _transformToDevice.Scale(dpiScaleX, dpiScaleY);
            return _transformToDevice.Transform(logicalPoint);
        }

        /// <summary>
        /// Convert a point in system coordinates to a point in device independent pixels (1/96").
        /// </summary>
        /// <param name="logicalPoint">A point in the physical coordinate system.</param>
        /// <returns>Returns the parameter converted to the device independent coordinate system.</returns>
        public static Point DevicePixelsToLogical(Point devicePoint, double dpiScaleX, double dpiScaleY)
        {
            _transformToDip = Matrix.Identity;
            _transformToDip.Scale(1d / dpiScaleX, 1d / dpiScaleY);
            return _transformToDip.Transform(devicePoint);
        }

        public static Rect LogicalRectToDevice(Rect logicalRectangle, double dpiScaleX, double dpiScaleY)
        {
            Point topLeft = LogicalPixelsToDevice(new Point(logicalRectangle.Left, logicalRectangle.Top), dpiScaleX, dpiScaleY);
            Point bottomRight = LogicalPixelsToDevice(new Point(logicalRectangle.Right, logicalRectangle.Bottom), dpiScaleX, dpiScaleY);

            return new Rect(topLeft, bottomRight);
        }

        public static Rect DeviceRectToLogical(Rect deviceRectangle, double dpiScaleX, double dpiScaleY)
        {
            Point topLeft = DevicePixelsToLogical(new Point(deviceRectangle.Left, deviceRectangle.Top), dpiScaleX, dpiScaleY);
            Point bottomRight = DevicePixelsToLogical(new Point(deviceRectangle.Right, deviceRectangle.Bottom), dpiScaleX, dpiScaleY);

            return new Rect(topLeft, bottomRight);
        }

        public static Size LogicalSizeToDevice(Size logicalSize, double dpiScaleX, double dpiScaleY)
        {
            Point pt = LogicalPixelsToDevice(new Point(logicalSize.Width, logicalSize.Height), dpiScaleX, dpiScaleY);

            return new Size { Width = pt.X, Height = pt.Y };
        }

        public static Size DeviceSizeToLogical(Size deviceSize, double dpiScaleX, double dpiScaleY)
        {
            Point pt = DevicePixelsToLogical(new Point(deviceSize.Width, deviceSize.Height), dpiScaleX, dpiScaleY);

            return new Size(pt.X, pt.Y);
        }

        public static Thickness LogicalThicknessToDevice(Thickness logicalThickness, double dpiScaleX, double dpiScaleY)
        {
            Point topLeft = LogicalPixelsToDevice(new Point(logicalThickness.Left, logicalThickness.Top), dpiScaleX, dpiScaleY);
            Point bottomRight = LogicalPixelsToDevice(new Point(logicalThickness.Right, logicalThickness.Bottom), dpiScaleX, dpiScaleY);

            return new Thickness(topLeft.X, topLeft.Y, bottomRight.X, bottomRight.Y);
        }



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


        public const double DefaultPixelsPerInch = 96.0;


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

        public static DpiScale2 GetSystemDpi()
        {
            return SystemDpiHelper.GetSystemDpi();
        }

        public static DpiScale2 GetSystemDpiFromUIElementCache()
        {
            return SystemDpiHelper.GetSystemDpiFromUIElementCache();
        }

        public static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
        {
            SystemDpiHelper.UpdateUIElementCacheForSystemDpi(systemDpiScale);
        }

        public static DpiScale2 GetWindowDpi(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
        {
            return WindowDpiScaleHelper.GetWindowDpi(hWnd, fallbackToNearestMonitorHeuristic);
        }

        public static HwndDpiInfo GetExtendedDpiInfoForWindow(IntPtr hWnd, bool fallbackToNearestMonitorHeuristic)
        {
            return new HwndDpiInfo(hWnd, fallbackToNearestMonitorHeuristic);
        }

        public static HwndDpiInfo GetExtendedDpiInfoForWindow(IntPtr hWnd)
        {
            return GetExtendedDpiInfoForWindow(hWnd, fallbackToNearestMonitorHeuristic: true);
        }

        public static IDisposable WithDpiAwarenessContext(DpiAwarenessContextValue dpiAwarenessContext)
        {
            return new DpiAwarenessScope(dpiAwarenessContext);
        }

        public static DpiFlags UpdateDpiScalesAndGetIndex(double pixelsPerInchX, double pixelsPerInchY)
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
