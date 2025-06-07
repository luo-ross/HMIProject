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
using RS.Widgets.Controls.Helpers;
using ScottPlot.AxisLimitManagers;
namespace RS.Widgets.Controls
{
    public class RSTestWindow : ContentControl
    {
        private int _Style;
        private int _StyleEx;
        private bool _isVisible;
        private ushort _classAtom;
        private bool _isInCreateWindow;
        private IntPtr _ownerHandle = IntPtr.Zero;


        static RSTestWindow()
        {

        }
        public RSTestWindow()
        {



        }
        public unsafe void Show()
        {




            var _GET_STOCK_OBJECT_FLAGS = (GET_STOCK_OBJECT_FLAGS)5;
            var intPtr = Ross.GetStockObject(_GET_STOCK_OBJECT_FLAGS);
            if (intPtr == IntPtr.Zero)
            {
                throw new Win32Exception(Marshal.GetLastWin32Error());
            }

            var cbSize = Marshal.SizeOf(typeof(WNDCLASSEXW));
            var hInstance = Ross.GetModuleHandle(default(string));


            double requestedTop = 0.0;
            double requestedLeft = 0.0;
            double requestedWidth = 0.0;
            double requestedHeight = 0.0;

            string text = ((AppDomain.CurrentDomain.FriendlyName == null || 128 > AppDomain.CurrentDomain.FriendlyName.Length) ? AppDomain.CurrentDomain.FriendlyName : AppDomain.CurrentDomain.FriendlyName.Substring(0, 128));
            string text2 = ((Thread.CurrentThread.Name == null || 64 > Thread.CurrentThread.Name.Length) ? Thread.CurrentThread.Name : Thread.CurrentThread.Name.Substring(0, 64));
            string text3 = Guid.NewGuid().ToString();
            string lpszClassName = string.Format(CultureInfo.InvariantCulture, "HwndWrapper[{0};{1};{2}]", new object[3] { text, text2, text3 });
            WindowStartupTopLeftPointHelper windowStartupTopLeftPointHelper = new WindowStartupTopLeftPointHelper(new Point(requestedLeft, requestedTop));

            HwndSourceParameters parameters = CreateHwndSourceParameters();
            if (windowStartupTopLeftPointHelper.ScreenTopLeft.HasValue)
            {
                Point value = windowStartupTopLeftPointHelper.ScreenTopLeft.Value;
                parameters.SetPosition((int)value.X, (int)value.Y);
            }

            if (EffectivePerPixelOpacity(parameters))
            {
                parameters.ExtendedWindowStyle |= 524288;
            }
            else
            {
                parameters.ExtendedWindowStyle &= -524289;
            }

            IntPtr ptr = Marshal.StringToHGlobalUni(lpszClassName);
            var classStyle = GetStyle();
            try
            {

                WNDCLASSEXW param0 = new WNDCLASSEXW()
                {
                    cbClsExtra = 0,
                    cbSize = (uint)cbSize,
                    cbWndExtra = 0,
                    hbrBackground = new HBRUSH(intPtr),
                    hCursor = new HCURSOR(0),
                    hIcon = new HICON(0),
                    hIconSm = new HICON(0),
                    hInstance = new Windows.Win32.Foundation.HINSTANCE(hInstance.DangerousGetHandle()),
                    lpfnWndProc = SubclassWndProc,
                    lpszClassName = new PCWSTR((char*)ptr),
                    lpszMenuName = new PCWSTR(),
                    style = classStyle,
                };
                _classAtom = Ross.RegisterClassEx(param0);
                _isInCreateWindow = true;

                var exStyle = (WINDOW_EX_STYLE)parameters.ExtendedWindowStyle;
                var name = parameters.WindowName;
                var x = 0;
                var y = 0;
                var width = 700;
                var height = 500;
                var parent = parameters.ParentWindow;
                var style = (WINDOW_STYLE)parameters.WindowStyle;

                var hWnd = Ross.CreateWindowEx(WINDOW_EX_STYLE.WS_EX_APPWINDOW, lpszClassName, name, style, x, y, width, height, new HWND(parent), new SafeProcessHandle(IntPtr.Zero, true), new SafeProcessHandle(IntPtr.Zero, true), null);

                Ross.ShowWindow(hWnd, SHOW_WINDOW_CMD.SW_SHOWDEFAULT);
            }
            finally
            {
                // 释放非托管内存
                Marshal.FreeHGlobal(ptr);
            }


        }


        private int NCmdForShow()
        {
            int num = 0;
            return WindowState switch
            {
                WindowState.Maximized => 3,
                WindowState.Minimized => ShowActivated ? 2 : 7,
                _ => ShowActivated ? 5 : 8,
            };
        }

        public bool EffectivePerPixelOpacity(HwndSourceParameters parameters)
        {

            if (parameters.UsesPerPixelTransparency)
            {
                if (parameters.UsesPerPixelOpacity)
                {
                    throw new InvalidOperationException(("UsesPerPixelOpacityIsObsolete"));
                }

                if (!(Environment.OSVersion.Version >= new Version(6, 2)))
                {
                    return (parameters.WindowStyle & 0x40000000) == 0;
                }
                return true;
            }

            if (parameters.UsesPerPixelOpacity)
            {
                return (parameters.WindowStyle & 0x40000000) == 0;
            }

            return false;
        }



        [SecurityCritical]
        public virtual HwndSourceParameters CreateHwndSourceParameters()
        {
            HwndSourceParameters result = new HwndSourceParameters(Title, int.MinValue, int.MinValue);
            result.UsesPerPixelOpacity = AllowsTransparency;
            result.WindowStyle = _Style;
            result.ExtendedWindowStyle = _StyleEx;
            result.ParentWindow = _ownerHandle;
            result.AdjustSizingForNonClientArea = true;
            result.HwndSourceHook = WindowFilterMessage;
            return result;
        }


        private WNDCLASS_STYLES GetStyle()
        {
            return (WNDCLASS_STYLES)this._Style;
        }


        public virtual void CreateAllStyle()
        {

            _Style = 34078720;
            _StyleEx = 0;
            CreateWindowStyle();
            CreateWindowState();
            if (_isVisible)
            {
                _Style |= 268435456;
            }

            SetTaskbarStatus();
            CreateTopmost();
            CreateResizibility();
            CreateRtl();
        }






        private void CreateWindowStyle()
        {
            _Style &= -12582913;
            _StyleEx &= -641;
            switch (WindowStyle)
            {
                case WindowStyle.None:
                    _Style &= -12582913;
                    break;
                case WindowStyle.SingleBorderWindow:
                    _Style |= 12582912;
                    break;
                case WindowStyle.ThreeDBorderWindow:
                    _Style |= 12582912;
                    _StyleEx |= 512;
                    break;
                case WindowStyle.ToolWindow:
                    _Style |= 12582912;
                    _StyleEx |= 128;
                    break;
            }
        }


        private void CreateWindowState()
        {
            switch (WindowState)
            {
                case WindowState.Maximized:
                    _Style |= 16777216;
                    break;
                case WindowState.Minimized:
                    _Style |= 536870912;
                    break;
                case WindowState.Normal:
                    break;
            }
        }

        private void CreateTopmost()
        {
            if (Topmost)
            {
                _StyleEx |= 8;
            }
            else
            {
                _StyleEx &= -9;
            }
        }

        private void CreateResizibility()
        {
            _Style &= -458753;
            switch (ResizeMode)
            {
                case ResizeMode.CanMinimize:
                    _Style |= 131072;
                    break;
                case ResizeMode.CanResize:
                case ResizeMode.CanResizeWithGrip:
                    _Style |= 458752;
                    break;
                case ResizeMode.NoResize:
                    break;
            }
        }

        private void CreateRtl()
        {
            if (base.FlowDirection == FlowDirection.LeftToRight)
            {
                _StyleEx &= -4194305;
                return;
            }

            if (base.FlowDirection == FlowDirection.RightToLeft)
            {
                _StyleEx |= 4194304;
                return;
            }

            throw new InvalidOperationException("IncorrectFlowDirection");
        }

        private void SetTaskbarStatus()
        {
            //if (!ShowInTaskbar)
            //{
            //    SecurityHelper.DemandUIWindowPermission();
            //    EnsureHiddenWindow();
            //    if (_ownerHandle == IntPtr.Zero)
            //    {
            //        SetOwnerHandle(_hiddenWindow.Handle);
            //        if (!IsSourceWindowNull && !IsCompositionTargetInvalid)
            //        {
            //            UpdateIcon();
            //        }
            //    }

            //    _StyleEx &= -262145;
            //}
            //else
            //{
            //    _StyleEx |= 262144;
            //    if (!IsSourceWindowNull && _hiddenWindow != null && _ownerHandle == _hiddenWindow.Handle)
            //    {
            //        SetOwnerHandle(IntPtr.Zero);
            //    }
            //}
        }



        internal LRESULT SubclassWndProc(HWND param0, uint msg, WPARAM wParam, LPARAM lParam)
        {
            //IntPtr result = IntPtr.Zero;
            //bool flag = false;
            //if (_bond == Bond.Unattached)
            //{
            //    HookWindowProc(hwnd, SubclassWndProc, Marshal.GetFunctionPointerForDelegate((Delegate)DefWndProcStub));
            //}
            //else if (_bond == Bond.Detached)
            //{
            //    throw new InvalidOperationException();
            //}

            //IntPtr oldWndProc = _oldWndProc;
            //if (msg == (int)DetachMessage)
            //{
            //    if (wParam == IntPtr.Zero || wParam == (IntPtr)_gcHandle)
            //    {
            //        int num = (int)lParam;
            //        bool force = num > 0;
            //        result = (CriticalDetach(force) ? new IntPtr(1) : IntPtr.Zero);
            //        flag = num < 2;
            //    }
            //}
            //else
            //{
            //    Dispatcher dispatcher = Dispatcher.FromThread(Thread.CurrentThread);
            //    if (dispatcher != null && !dispatcher.HasShutdownFinished)
            //    {
            //        if (_dispatcherOperationCallback == null)
            //        {
            //            _dispatcherOperationCallback = DispatcherCallbackOperation;
            //        }

            //        if (_paramDispatcherCallbackOperation == null)
            //        {
            //            _paramDispatcherCallbackOperation = new DispatcherOperationCallbackParameter();
            //        }

            //        DispatcherOperationCallbackParameter paramDispatcherCallbackOperation = _paramDispatcherCallbackOperation;
            //        _paramDispatcherCallbackOperation = null;
            //        paramDispatcherCallbackOperation.hwnd = hwnd;
            //        paramDispatcherCallbackOperation.msg = msg;
            //        paramDispatcherCallbackOperation.wParam = wParam;
            //        paramDispatcherCallbackOperation.lParam = lParam;
            //        object obj = dispatcher.Invoke(DispatcherPriority.Send, _dispatcherOperationCallback, paramDispatcherCallbackOperation);
            //        if (obj != null)
            //        {
            //            flag = paramDispatcherCallbackOperation.handled;
            //            result = paramDispatcherCallbackOperation.retVal;
            //        }

            //        _paramDispatcherCallbackOperation = paramDispatcherCallbackOperation;
            //    }

            //    if (msg == 130)
            //    {
            //        CriticalDetach(force: true);
            //        flag = false;
            //    }
            //}

            //if (!flag)
            //{
            //    result = CallOldWindowProc(oldWndProc, hwnd, (WindowMessage)msg, wParam, lParam);
            //}

            //return result;

            return default(LRESULT);
        }

        [SecurityCritical]
        private IntPtr WindowFilterMessage(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            IntPtr refInt = IntPtr.Zero;
            //switch (msg)
            //{
            //    case 36:
            //        handled = WmGetMinMaxInfo(lParam);
            //        break;
            //    case 5:
            //        handled = WmSizeChanged(wParam);
            //        break;
            //}

            //if (_swh != null && _swh.CompositionTarget != null)
            //{
            //    if (msg == (int)WM_TASKBARBUTTONCREATED || msg == (int)WM_APPLYTASKBARITEMINFO)
            //    {
            //        if (_taskbarRetryTimer != null)
            //        {
            //            _taskbarRetryTimer.Stop();
            //        }

            //        ApplyTaskbarItemInfo();
            //    }
            //    else
            //    {
            //        switch (msg)
            //        {
            //            case 16:
            //                handled = WmClose();
            //                break;
            //            case 2:
            //                handled = WmDestroy();
            //                break;
            //            case 6:
            //                handled = WmActivate(wParam);
            //                break;
            //            case 3:
            //                handled = WmMoveChanged();
            //                break;
            //            case 132:
            //                handled = WmNcHitTest(lParam, ref refInt);
            //                break;
            //            case 24:
            //                handled = WmShowWindow(wParam, lParam);
            //                break;
            //            case 273:
            //                handled = WmCommand(wParam, lParam);
            //                break;
            //            default:
            //                handled = false;
            //                break;
            //        }
            //    }
            //}

            return refInt;
        }




        public bool ShowActivated
        {
            get { return (bool)GetValue(ShowActivatedProperty); }
            set { SetValue(ShowActivatedProperty, value); }
        }

        public static readonly DependencyProperty ShowActivatedProperty =
            DependencyProperty.Register("ShowActivated", typeof(bool), typeof(RSTestWindow), new PropertyMetadata(false));




        public WindowStyle WindowStyle
        {
            get { return (WindowStyle)GetValue(WindowStyleProperty); }
            set { SetValue(WindowStyleProperty, value); }
        }

        public static readonly DependencyProperty WindowStyleProperty =
            DependencyProperty.Register("WindowStyle", typeof(WindowStyle), typeof(RSTestWindow), new PropertyMetadata(default));



        public bool ShowInTaskbar
        {
            get { return (bool)GetValue(ShowInTaskbarProperty); }
            set { SetValue(ShowInTaskbarProperty, value); }
        }

        public static readonly DependencyProperty ShowInTaskbarProperty =
            DependencyProperty.Register("ShowInTaskbar", typeof(bool), typeof(RSTestWindow), new PropertyMetadata(true));



        public WindowState WindowState
        {
            get { return (WindowState)GetValue(WindowStateProperty); }
            set { SetValue(WindowStateProperty, value); }
        }

        public static readonly DependencyProperty WindowStateProperty =
            DependencyProperty.Register("WindowState", typeof(WindowState), typeof(RSTestWindow), new PropertyMetadata(default));




        public bool Topmost
        {
            get { return (bool)GetValue(TopmostProperty); }
            set { SetValue(TopmostProperty, value); }
        }

        public static readonly DependencyProperty TopmostProperty =
            DependencyProperty.Register("Topmost", typeof(bool), typeof(RSTestWindow), new PropertyMetadata(false));




        public ResizeMode ResizeMode
        {
            get { return (ResizeMode)GetValue(ResizeModeProperty); }
            set { SetValue(ResizeModeProperty, value); }
        }

        public static readonly DependencyProperty ResizeModeProperty =
            DependencyProperty.Register("ResizeMode", typeof(ResizeMode), typeof(RSTestWindow), new PropertyMetadata(default));




        public string Title
        {
            get { return (string)GetValue(TitleProperty); }
            set { SetValue(TitleProperty, value); }
        }

        public static readonly DependencyProperty TitleProperty =
            DependencyProperty.Register("Title", typeof(string), typeof(RSTestWindow), new PropertyMetadata(default));



        public bool AllowsTransparency
        {
            get { return (bool)GetValue(AllowsTransparencyProperty); }
            set { SetValue(AllowsTransparencyProperty, value); }
        }

        public static readonly DependencyProperty AllowsTransparencyProperty =
            DependencyProperty.Register("AllowsTransparency", typeof(bool), typeof(RSTestWindow), new PropertyMetadata(false));




    }







}
