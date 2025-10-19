using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows;
using RS.Widgets.Interop;
using RS.Win32API.Structs;
using RS.Win32API;

namespace RS.Widgets.Controls
{
    public class SourceWindowHelper
    {
        private HwndSource _sourceWindow;

        private HwndPanningFeedback _panningFeedback;

        public bool IsSourceWindowNull
        {
            get
            {
                return _sourceWindow == null;
            }
        }

        public bool IsCompositionTargetInvalid
        {
            get
            {
                return CompositionTarget == null;
            }
        }

        public nint CriticalHandle
        {
            get
            {
                if (_sourceWindow != null)
                {
                    return _sourceWindow.Handle;
                }

                return nint.Zero;
            }
        }

        public RECT WorkAreaBoundsForNearestMonitor
        {

            get
            {
                MONITORINFOEX mONITORINFOEX = new MONITORINFOEX();
                mONITORINFOEX.cbSize = Marshal.SizeOf(typeof(MONITORINFOEX));
                nint intPtr = NativeMethods.MonitorFromWindow(new HandleRef(this, CriticalHandle), 2);
                if (intPtr != nint.Zero)
                {
                    NativeMethods.GetMonitorInfo(new HandleRef(this, intPtr), mONITORINFOEX);
                }

                return mONITORINFOEX.rcWork;
            }
        }

        public RECT ClientBounds
        {
            get
            {
                RECT rect = new RECT(0, 0, 0, 0);
                NativeMethods.GetClientRect(new HandleRef(this, CriticalHandle), ref rect);
                return rect;
            }
        }

        public RECT WindowBounds
        {
            get
            {
                RECT rect = new RECT(0, 0, 0, 0);
                NativeMethods.GetWindowRect(new HandleRef(this, CriticalHandle), ref rect);
                return rect;
            }
        }

        public SizeToContent HwndSourceSizeToContent
        {


            get
            {
                return _sourceWindow.SizeToContent;
            }


            set
            {
                _sourceWindow.SizeToContent = value;
            }
        }

        public Visual RootVisual
        {

            set
            {
                _sourceWindow.RootVisual = value;
            }
        }

        public bool IsActiveWindow
        {


            get
            {
                return _sourceWindow.Handle == NativeMethods.GetActiveWindow();
            }
        }

        public HwndSource HwndSourceWindow
        {

            get
            {
                return _sourceWindow;
            }
        }

        public HwndTarget CompositionTarget
        {

            get
            {
                if (_sourceWindow != null)
                {
                    HwndTarget compositionTarget = _sourceWindow.CompositionTarget;
                    if (compositionTarget != null && !_sourceWindow.IsDisposed)
                    {
                        return compositionTarget;
                    }
                }

                return null;
            }
        }

        public Size WindowSize
        {
            get
            {
                RECT windowBounds = WindowBounds;
                return new Size(windowBounds.Right - windowBounds.Left, windowBounds.Bottom - windowBounds.Top);
            }
        }

        public int StyleExFromHwnd
        {
            get
            {
                return (int)NativeMethods.GetWindowLong(new HandleRef(this, CriticalHandle), -20);
            }
        }

        public int StyleFromHwnd
        {
            get
            {
                return (int)NativeMethods.GetWindowLong(new HandleRef(this, CriticalHandle), -16);
            }
        }


        public SourceWindowHelper(HwndSource sourceWindow)
        {
            _sourceWindow = sourceWindow;
        }


        private POINT GetWindowScreenLocation(FlowDirection flowDirection)
        {
            POINT pOINT = new POINT(0, 0);
            if (flowDirection == FlowDirection.RightToLeft)
            {
                RECT rect = new RECT(0, 0, 0, 0);
                NativeMethods.GetClientRect(new HandleRef(this, CriticalHandle), ref rect);
                pOINT = new POINT(rect.Right, rect.Top);
            }

            NativeMethods.ClientToScreen(new HandleRef(this, _sourceWindow.Handle), pOINT);
            return pOINT;
        }



        public POINT GetPointRelativeToWindow(int x, int y, FlowDirection flowDirection)
        {
            POINT windowScreenLocation = GetWindowScreenLocation(flowDirection);
            return new POINT(x - windowScreenLocation.X, y - windowScreenLocation.Y);
        }



        public Size GetSizeFromHwndInMeasureUnits()
        {
            Point point = new Point(0.0, 0.0);
            RECT windowBounds = WindowBounds;
            point.X = windowBounds.Right - windowBounds.Left;
            point.Y = windowBounds.Bottom - windowBounds.Top;
            point = _sourceWindow.CompositionTarget.TransformFromDevice.Transform(point);
            return new Size(point.X, point.Y);
        }



        public Size GetHwndNonClientAreaSizeInMeasureUnits()
        {
            RECT clientBounds = ClientBounds;
            RECT windowBounds = WindowBounds;
            Point point = new Point(windowBounds.Right - windowBounds.Left - (clientBounds.Right - clientBounds.Left), windowBounds.Bottom - windowBounds.Top - (clientBounds.Bottom - clientBounds.Top));
            point = _sourceWindow.CompositionTarget.TransformFromDevice.Transform(point);
            return new Size(Math.Max(0.0, point.X), Math.Max(0.0, point.Y));
        }



        public void ClearRootVisual()
        {
            if (_sourceWindow.RootVisual != null)
            {
                _sourceWindow.RootVisual = null;
            }
        }


        public void AddDisposedHandler(EventHandler theHandler)
        {
            if (_sourceWindow != null)
            {
                _sourceWindow.Disposed += theHandler;
            }
        }


        public void RemoveDisposedHandler(EventHandler theHandler)
        {
            if (_sourceWindow != null)
            {
                _sourceWindow.Disposed -= theHandler;
            }
        }



        public void UpdatePanningFeedback(Vector totalOverpanOffset, bool animate)
        {
            if (_panningFeedback == null && _sourceWindow != null)
            {
                _panningFeedback = new HwndPanningFeedback(_sourceWindow);
            }

            if (_panningFeedback != null)
            {
                _panningFeedback.UpdatePanningFeedback(totalOverpanOffset, animate);
            }
        }



        public void EndPanningFeedback(bool animateBack)
        {
            if (_panningFeedback != null)
            {
                _panningFeedback.EndPanningFeedback(animateBack);
                _panningFeedback = null;
            }
        }
    }

}
