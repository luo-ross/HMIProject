using RS.Widgets.Standard;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.SafeHandles;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Media;

namespace RS.Widgets.Commons
{
    public static class DpiHelper
    {
        public static List<double> DpiScaleXValues;
        public static List<double> DpiScaleYValues;
        public static object DpiLock;
        public static bool IsGetDpiForSystemFunctionAvailable { get; set; } = true;
        static DpiHelper()
        {
            DpiLock = new object();
            DpiScaleXValues = new List<double>(3);
            DpiScaleYValues = new List<double>(3);
        }


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


        public static HwndDpiInfo GetExtendedDpiInfoForWindow(nint hWnd, bool fallbackToNearestMonitorHeuristic)
        {
            return new HwndDpiInfo(hWnd, fallbackToNearestMonitorHeuristic);
        }

        public static HwndDpiInfo GetExtendedDpiInfoForWindow(nint hWnd)
        {
            return GetExtendedDpiInfoForWindow(hWnd, fallbackToNearestMonitorHeuristic: true);
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
            lock (DpiLock)
            {
                return new DpiScale2(DpiScaleXValues[0], DpiScaleYValues[0]);
            }
        }

        public static void UpdateUIElementCacheForSystemDpi(DpiScale2 systemDpiScale)
        {
            lock (DpiLock)
            {
                DpiScaleXValues.Insert(0, systemDpiScale.DpiScaleX);
                DpiScaleYValues.Insert(0, systemDpiScale.DpiScaleY);
            }
        }

        public static DpiScale2 GetDpiForSystem()
        {
            uint dpiForSystem = NativeMethods.GetDpiForSystem();
            return DpiScale2.FromPixelsPerInch(dpiForSystem, dpiForSystem);
        }


        public static DpiScale2 GetSystemDpiFromDeviceCaps()
        {
            HandleRef hWnd = new HandleRef(nint.Zero, nint.Zero);
            HandleRef hDC = new HandleRef(nint.Zero, NativeMethods.GetDC(hWnd));
            if (hDC.Handle == nint.Zero)
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


        public static DpiFlags UpdateDpiScalesAndGetIndex(double pixelsPerInchX, double pixelsPerInchY)
        {
            lock (DpiLock)
            {
                int num = 0;
                int count = DpiScaleXValues.Count;
                for (num = 0; num < count && (DpiScaleXValues[num] != pixelsPerInchX / 96.0 || DpiScaleYValues[num] != pixelsPerInchY / 96.0); num++)
                {
                }

                if (num == count)
                {
                    DpiScaleXValues.Add(pixelsPerInchX / 96.0);
                    DpiScaleYValues.Add(pixelsPerInchY / 96.0);
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
                    dpiScaleFlag = dpiScaleFlag2 = true;
                }

                return new DpiFlags(dpiScaleFlag, dpiScaleFlag2, num);
            }
        }



        public static bool IsGetDpiForWindowFunctionEnabled { get; set; } = true;

        public static DpiScale2 GetWindowDpi(nint hWnd, bool fallbackToNearestMonitorHeuristic)
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

        public static DpiScale2 GetDpiForWindow(nint hWnd)
        {
            uint dpiForWindow = NativeMethods.GetDpiForWindow(new HandleRef(nint.Zero, hWnd));
            return DpiScale2.FromPixelsPerInch(dpiForWindow, dpiForWindow);
        }
        public static DpiScale2 GetDpiForWindowFromNearestMonitor(nint hWnd)
        {
            nint handle = NativeMethods.MonitorFromWindow(new HandleRef(nint.Zero, hWnd), 2);
            uint dpiX;
            uint dpiY;
            int dpiForMonitor = (int)NativeMethods.GetDpiForMonitor(new HandleRef(nint.Zero, handle), MONITOR_DPI_TYPE.MDT_EFFECTIVE_DPI, out dpiX, out dpiY);
            Marshal.ThrowExceptionForHR(dpiForMonitor);
            return DpiScale2.FromPixelsPerInch(dpiX, dpiY);
        }

    }
}
