using NPOI.SS.Formula.Functions;
using RS.Widgets.Commons;
using RS.Widgets.Interop;
using RS.Widgets.Standard;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Helper;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Automation.Provider;
using System.Windows.Interop;
using System.Windows.Media;
using ZXing.PDF417.Internal;

namespace RS.Widgets.Controls
{
    /// <summary>
    /// 测试使用
    /// </summary>
    public class RSHwndHost : HwndHost
    {
        private HandleRef _hwnd;

        public FrameworkElement Child
        {
            get { return (FrameworkElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child", typeof(FrameworkElement), typeof(RSHwndHost), new PropertyMetadata(null, OnChildPropertyChanged));

        private static void OnChildPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dockHwndHost = (RSHwndHost)d;
            dockHwndHost.WindowCore.Content = dockHwndHost.Child;
        }


        public Window WindowCore
        {
            get { return (Window)GetValue(WindowCoreProperty); }
            set { SetValue(WindowCoreProperty, value); }
        }

        public static readonly DependencyProperty WindowCoreProperty =
            DependencyProperty.Register("WindowCore", typeof(Window), typeof(RSHwndHost), new PropertyMetadata(null));



        private bool _hasDpiAwarenessContextTransition = false;
        protected override HandleRef BuildWindowCore(HandleRef hwndParent)
        {
            this.WindowCore = new Window();
            var hwnd = new WindowInteropHelper(this.WindowCore).EnsureHandle();
            this.WindowCore.Show();
            _hwnd = new HandleRef(this, hwnd);
            NativeMethods.SetWindowLong(_hwnd, NativeMethods.GWL_STYLE, new HandleRef(this, NativeMethods.GetWindowLong(_hwnd, NativeMethods.GWL_STYLE) | NativeMethods.WS_CHILD | NativeMethods.WS_CLIPSIBLINGS));
            NativeMethods.SetParent(hwnd, hwndParent.Handle);

            return _hwnd;
        }


        protected override void DestroyWindowCore(HandleRef hwnd)
        {
            var window = HwndSource.FromHwnd(hwnd.Handle)?.RootVisual as Window;
            window?.Close();
        }



        protected override void OnWindowPositionChanged(Rect rcBoundingBox)
        {
            //NativeMethods.SetWindowPos(_hwnd,
            //                    new HandleRef(null, IntPtr.Zero),
            //                    (int)0,
            //                    (int)0,
            //                    (int)500,
            //                    (int)400, 0);
        }


        protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            //switch ((WM)msg)
            //{
            //    case WM.NCDESTROY:
            //        _hwnd = new HandleRef(null, IntPtr.Zero);
            //        break;

            //    // When layout happens, we first calculate the right size/location then call SetWindowPos.
            //    // We only allow the changes that are coming from Avalon layout. The hwnd is not allowed to change by itself.
            //    // So the size of the hwnd should always be RenderSize and the position be where layout puts it.
            //    case WM.WINDOWPOSCHANGING:
            //        PresentationSource source = PresentationSource.FromDependencyObject(this);

            //        if (source != null)
            //        {
            //            // Get the rect assigned by layout to us.
            //            RECT assignedRC = CalculateAssignedRC(source);

            //            // The lParam is a pointer to a WINDOWPOS structure
            //            // that contains information about the size and
            //            // position that the window is changing to.  Note that
            //            // modifying this structure during WM_WINDOWPOSCHANGING
            //            // will change what happens to the window.
            //            unsafe
            //            {
            //                WINDOWPOS* windowPos = (WINDOWPOS*)lParam;

            //                // Always force the size of the window to be the
            //                // size of our assigned rectangle.  Note that we
            //                // have to always clear the SWP_NOSIZE flag.
            //                windowPos->cx = assignedRC.Right - assignedRC.Left;
            //                windowPos->cy = assignedRC.Bottom - assignedRC.Top;
            //                windowPos->flags &= ~NativeMethods.SWP_NOSIZE;

            //                // Always force the position of the window to be
            //                // the upper-left corner of our assigned rectangle.
            //                // Note that we have to always clear the
            //                // SWP_NOMOVE flag.
            //                windowPos->x = assignedRC.Left;
            //                windowPos->y = assignedRC.Top;
            //                windowPos->flags &= ~NativeMethods.SWP_NOMOVE;

            //                // Windows has an optimization to copy pixels
            //                // around to reduce the amount of repainting
            //                // needed when moving or resizing a window.
            //                // Unfortunately, this is not compatible with WPF
            //                // in many cases due to our use of DirectX for
            //                // rendering from our rendering thread.
            //                // To be safe, we disable this optimization and
            //                // pay the cost of repainting.
            //                windowPos->flags |= NativeMethods.SWP_NOCOPYBITS;
            //            }
            //        }
            //        break;
            //    case WM.GETOBJECT:
            //        handled = true;
            //        return OnWmGetObject(wParam, lParam);
            //}
            return IntPtr.Zero;
        }



        private IntPtr OnWmGetObject(IntPtr wparam, IntPtr lparam)
        {
            IntPtr result = IntPtr.Zero;

            AutomationPeer containerPeer = UIElementAutomationPeer.CreatePeerForElement(this);
            if (containerPeer != null)
            {
                // get the element proxy
                IRawElementProviderSimple el = containerPeer.ReflectionCall<IRawElementProviderSimple>("GetInteropChild");
                result = AutomationInteropProvider.ReturnRawElementProvider(_hwnd.Handle, wparam, lparam, el);
            }
            return result;
        }


        private RECT CalculateAssignedRC(PresentationSource source)
        {
            Rect rectElement = new Rect(RenderSize);
            Rect rectRoot = PointHelper.ElementToRoot(rectElement, this, source);
            Rect rectClient = PointHelper.RootToClient(rectRoot, source);

            // Adjust for Right-To-Left oriented windows
            IntPtr hwndParent = NativeMethods.GetParent(_hwnd.Handle);
            RECT rcClient = PointHelper.FromRect(rectClient);
            RECT rcClientRTLAdjusted = PointHelper.AdjustForRightToLeft(rcClient, new HandleRef(null, hwndParent));

            if (!CoreAppContextSwitches.DoNotUsePresentationDpiCapabilityTier2OrGreater)
            {
                //Adjust for differences in DPI between _hwnd and hwndParent
                rcClientRTLAdjusted = AdjustRectForDpi(rcClientRTLAdjusted);
            }

            return rcClientRTLAdjusted;
        }

        private RECT AdjustRectForDpi(RECT rcRect)
        {
            if (_hasDpiAwarenessContextTransition)
            {
                double dpiRatio = DpiParentToChildRatio;
                rcRect.Left = (int)(rcRect.Left / dpiRatio);
                rcRect.Top = (int)(rcRect.Top / dpiRatio);
                rcRect.Right = (int)(rcRect.Right / dpiRatio);
                rcRect.Bottom = (int)(rcRect.Bottom / dpiRatio);
            }

            return rcRect;
        }

        private double DpiParentToChildRatio
        {
            get
            {
                if (!_hasDpiAwarenessContextTransition) return 1;
                DpiScale2 dpi = DpiHelper.GetWindowDpi(_hwnd.Handle, fallbackToNearestMonitorHeuristic: false);
                DpiScale2 dpiParent = DpiHelper.GetWindowDpi(NativeMethods.GetParent(_hwnd), fallbackToNearestMonitorHeuristic: false);
                if (dpi == null || dpiParent == null)
                {
                    // if DPI of the window can not be queried directly, then the platform
                    // is too old to support mixed mode DPI. The DPI ratio is 1.0
                    return 1.0d;
                }

                return dpiParent.DpiScaleX / dpi.DpiScaleX;
            }
        }
    }
}
