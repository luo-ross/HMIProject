using RS.Widgets.Structs;
using RS.Win32API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Threading;

namespace RS.Widgets.Interop
{
    public class HwndPanningFeedback
    {
        private int _deviceOffsetX;

        private int _deviceOffsetY;

        private bool _inInertia;

        private DispatcherOperation _updatePanningOperation;

        private bool _isProvidingPanningFeedback;

        private HwndSource _hwndSource;

        public static bool IsSupported => OperatingSystemVersionCheck.IsVersionOrLater(OperatingSystemVersion.Windows7);

        public HandleRef Handle
        {
            [SecurityCritical]
            get
            {
                if (_hwndSource != null)
                {
                    IntPtr criticalHandle = _hwndSource.Handle;
                    if (criticalHandle != IntPtr.Zero)
                    {
                        return new HandleRef(_hwndSource, criticalHandle);
                    }
                }

                return default(HandleRef);
            }
        }

        public HwndPanningFeedback(HwndSource hwndSource)
        {
            if (hwndSource == null)
            {
                throw new ArgumentNullException("hwndSource");
            }

            _hwndSource = hwndSource;
        }

       
        public void UpdatePanningFeedback(Vector totalOverpanOffset, bool inInertia)
        {
            if (_hwndSource == null || !IsSupported)
            {
                return;
            }

            if (!_isProvidingPanningFeedback)
            {
                _isProvidingPanningFeedback = NativeMethods.BeginPanningFeedback(Handle);
            }

            if (_isProvidingPanningFeedback)
            {
               
                Point point = _hwndSource.CompositionTarget.TransformToDevice.Transform((Point)totalOverpanOffset);
                _deviceOffsetX = (int)point.X;
                _deviceOffsetY = (int)point.Y;
                _inInertia = inInertia;
                if (_updatePanningOperation == null)
                {
                    _updatePanningOperation = _hwndSource.Dispatcher.BeginInvoke(DispatcherPriority.Background, new DispatcherOperationCallback(OnUpdatePanningFeedback), this);
                }
            }
        }

        public object OnUpdatePanningFeedback(object args)
        {
            HwndPanningFeedback hwndPanningFeedback = (HwndPanningFeedback)args;
            _updatePanningOperation = null;
            NativeMethods.UpdatePanningFeedback(hwndPanningFeedback.Handle, hwndPanningFeedback._deviceOffsetX, hwndPanningFeedback._deviceOffsetY, hwndPanningFeedback._inInertia);
            return null;
        }

        public void EndPanningFeedback(bool animateBack)
        {
            if (_hwndSource != null && _isProvidingPanningFeedback)
            {
                _isProvidingPanningFeedback = false;
                if (_updatePanningOperation != null)
                {
                    _updatePanningOperation.Abort();
                    _updatePanningOperation = null;
                }
                NativeMethods.EndPanningFeedback(Handle, animateBack);
            }
        }
    }
}
