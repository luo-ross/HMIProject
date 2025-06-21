using RS.Widgets.Structs;
using RS.Win32API;
using RS.Win32API.Enums;
using RS.Win32API.Helpers;
using RS.Win32API.Standard;
using RS.Win32API.Structs;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;
using System.Windows.Threading;

namespace RS.Widgets.Shell
{
    public delegate IntPtr MessageHandler(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled);

    public class MyChromeWorker : DependencyObject
    {

        private delegate void _Action();

        public const SWP _SwpFlags = SWP.DRAWFRAME
            | SWP.NOACTIVATE
            | SWP.NOMOVE
            | SWP.NOOWNERZORDER
            | SWP.NOSIZE
            | SWP.NOZORDER;

        private readonly List<KeyValuePair<WM, MessageHandler>> _messageTable;

        private Window _window;


        private IntPtr _hwnd;


        private HwndSource _hwndSource;

        private bool _isHooked;

        private bool _isFixedUp;

        private bool _isUserResizing;

        private bool _hasUserMovedWindow;

        private Point _windowPosAtStartOfUserMove;

        private MyChrome _chromeInfo;

        private WindowState _lastRoundingState;

        private WindowState _lastMenuState;

        private bool _isGlassEnabled;

        public static readonly DependencyProperty MyChromeWorkerProperty;

        private static readonly HT[,] _HitTestBorders;

        private bool _IsWindowDocked
        {

            get
            {
               
                if (_window.WindowState != 0)
                {
                    return false;
                }

                DpiScale dpi = DpiHelper.GetSystemDpi();
                RECT rECT = _GetAdjustedWindowRect(new RECT
                {
                    Bottom = 100,
                    Right = 100
                });
                Point point = new Point(_window.Left, _window.Top);
                point -= (Vector)DpiHelper.DevicePixelsToLogical(new Point(rECT.Left, rECT.Top), dpi.DpiScaleX, dpi.DpiScaleY);
                return _window.RestoreBounds.Location != point;
            }
        }

        static MyChromeWorker()
        {
            MyChromeWorkerProperty = DependencyProperty.RegisterAttached("MyChromeWorker", typeof(MyChromeWorker), typeof(MyChromeWorker), new PropertyMetadata(null, _OnChromeWorkerChanged));
            _HitTestBorders = new HT[3, 3]
            {
            {
                HT.TOPLEFT,
                HT.TOP,
                HT.TOPRIGHT
            },
            {
                HT.LEFT,
                HT.CLIENT,
                HT.RIGHT
            },
            {
                HT.BOTTOMLEFT,
                HT.BOTTOM,
                HT.BOTTOMRIGHT
            }
            };
        }

        public MyChromeWorker()
        {
            _messageTable = new List<KeyValuePair<WM, MessageHandler>>
        {
            new KeyValuePair<WM, MessageHandler>(WM.SETTEXT, _HandleSetTextOrIcon),
            new KeyValuePair<WM, MessageHandler>(WM.SETICON, _HandleSetTextOrIcon),
            new KeyValuePair<WM, MessageHandler>(WM.NCACTIVATE, _HandleNCActivate),
            new KeyValuePair<WM, MessageHandler>(WM.NCCALCSIZE, _HandleNCCalcSize),
            new KeyValuePair<WM, MessageHandler>(WM.NCHITTEST, _HandleNCHitTest),
            new KeyValuePair<WM, MessageHandler>(WM.NCRBUTTONUP, _HandleNCRButtonUp),
            new KeyValuePair<WM, MessageHandler>(WM.SIZE, _HandleSize),
            new KeyValuePair<WM, MessageHandler>(WM.WINDOWPOSCHANGED, _HandleWindowPosChanged),
            new KeyValuePair<WM, MessageHandler>(WM.DWMCOMPOSITIONCHANGED, _HandleDwmCompositionChanged)
        };
            if (Utility.IsPresentationFrameworkVersionLessThan4)
            {
                _messageTable.AddRange(new KeyValuePair<WM, MessageHandler>[4]
                {
                new KeyValuePair<WM, MessageHandler>(WM.WININICHANGE, _HandleSettingChange),
                new KeyValuePair<WM, MessageHandler>(WM.ENTERSIZEMOVE, _HandleEnterSizeMove),
                new KeyValuePair<WM, MessageHandler>(WM.EXITSIZEMOVE, _HandleExitSizeMove),
                new KeyValuePair<WM, MessageHandler>(WM.MOVE, _HandleMove)
                });
            }
        }

        public void SetMyChrome(MyChrome newChrome)
        {
            VerifyAccess();
            if (newChrome != _chromeInfo)
            {
                if (_chromeInfo != null)
                {
                    _chromeInfo.PropertyChangedThatRequiresRepaint -= _OnChromePropertyChangedThatRequiresRepaint;
                }

                _chromeInfo = newChrome;
                if (_chromeInfo != null)
                {
                    _chromeInfo.PropertyChangedThatRequiresRepaint += _OnChromePropertyChangedThatRequiresRepaint;
                }

                _ApplyNewCustomChrome();
            }
        }



        private void _OnChromePropertyChangedThatRequiresRepaint(object sender, EventArgs e)
        {
            _UpdateFrameState(force: true);
        }



        private static void _OnChromeWorkerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Window window = (Window)d;
            MyChromeWorker MyChromeWorker = (MyChromeWorker)e.NewValue;
            MyChromeWorker._SetWindow(window);
        }


        private void _SetWindow(Window window)
        {
            UnsubscribeWindowEvents();
            _window = window;
            _hwnd = new WindowInteropHelper(_window).Handle;
            Utility.AddDependencyPropertyChangeListener(_window, Control.TemplateProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
            Utility.AddDependencyPropertyChangeListener(_window, FrameworkElement.FlowDirectionProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
            _window.Closed += _UnsetWindow;
            if (IntPtr.Zero != _hwnd)
            {
                _hwndSource = HwndSource.FromHwnd(_hwnd);
                _window.ApplyTemplate();
                if (_chromeInfo != null)
                {
                    _ApplyNewCustomChrome();
                }
            }
            else
            {
                _window.SourceInitialized += _WindowSourceInitialized;
            }
        }



        private void _WindowSourceInitialized(object sender, EventArgs e)
        {
            _hwnd = new WindowInteropHelper(_window).Handle;
            _hwndSource = HwndSource.FromHwnd(_hwnd);
            if (_chromeInfo != null)
            {
                _ApplyNewCustomChrome();
            }
        }


        private void UnsubscribeWindowEvents()
        {
            if (_window != null)
            {
                Utility.RemoveDependencyPropertyChangeListener(_window, Control.TemplateProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
                Utility.RemoveDependencyPropertyChangeListener(_window, FrameworkElement.FlowDirectionProperty, _OnWindowPropertyChangedThatRequiresTemplateFixup);
                _window.SourceInitialized -= _WindowSourceInitialized;
                _window.StateChanged -= _FixupRestoreBounds;
            }
        }



        private void _UnsetWindow(object sender, EventArgs e)
        {
            UnsubscribeWindowEvents();
            if (_chromeInfo != null)
            {
                _chromeInfo.PropertyChangedThatRequiresRepaint -= _OnChromePropertyChangedThatRequiresRepaint;
            }

            _RestoreStandardChromeState(isClosing: true);
        }

        public static MyChromeWorker GetMyChromeWorker(Window window)
        {
            Verify.IsNotNull(window, "window");
            return (MyChromeWorker)window.GetValue(MyChromeWorkerProperty);
        }

        public static void SetMyChromeWorker(Window window, MyChromeWorker chrome)
        {
            Verify.IsNotNull(window, "window");
            window.SetValue(MyChromeWorkerProperty, chrome);
        }



        private void _OnWindowPropertyChangedThatRequiresTemplateFixup(object sender, EventArgs e)
        {
            if (_chromeInfo != null && _hwnd != IntPtr.Zero)
            {
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, new _Action(_FixupTemplateIssues));
            }
        }


        private void _ApplyNewCustomChrome()
        {
            if (_hwnd == IntPtr.Zero || _hwndSource.IsDisposed)
            {
                return;
            }

            if (_chromeInfo == null)
            {
                _RestoreStandardChromeState(isClosing: false);
                return;
            }

            if (!_isHooked)
            {
                _hwndSource.AddHook(_WndProc);
                _isHooked = true;
            }

            _FixupTemplateIssues();
            _UpdateSystemMenu(_window.WindowState);
            _UpdateFrameState(force: true);
            NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0, SWP.DRAWFRAME
                | SWP.NOACTIVATE
                | SWP.NOMOVE
                | SWP.NOOWNERZORDER
                | SWP.NOSIZE
                | SWP.NOZORDER);
        }



        private void RetryFixupTemplateIssuesOnVisualChildrenAdded(object sender, EventArgs e)
        {
            if (sender == _window)
            {
                //_window.VisualChildrenChanged -= RetryFixupTemplateIssuesOnVisualChildrenAdded;
                _window.Dispatcher.BeginInvoke(DispatcherPriority.Render, new _Action(_FixupTemplateIssues));
            }
        }


        private void _FixupTemplateIssues()
        {
            if (_window.Template == null)
            {
                return;
            }

            if (VisualTreeHelper.GetChildrenCount(_window) == 0)
            {
                //_window.VisualChildrenChanged += RetryFixupTemplateIssuesOnVisualChildrenAdded;
                return;
            }

            Thickness margin = default(Thickness);
            Transform transform = null;
            FrameworkElement frameworkElement = (FrameworkElement)VisualTreeHelper.GetChild(_window, 0);
            if (_chromeInfo.NonClientFrameEdges != 0)
            {
                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 2))
                {
                    margin.Top -= SystemParameters.WindowResizeBorderThickness.Top;
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 1))
                {
                    margin.Left -= SystemParameters.WindowResizeBorderThickness.Left;
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 8))
                {
                    margin.Bottom -= SystemParameters.WindowResizeBorderThickness.Bottom;
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 4))
                {
                    margin.Right -= SystemParameters.WindowResizeBorderThickness.Right;
                }
            }

            if (Utility.IsPresentationFrameworkVersionLessThan4)
            {
                DpiScale dpi = DpiHelper.GetSystemDpi();
                RECT windowRect = NativeMethods.GetWindowRect(_hwnd);
                RECT rECT = _GetAdjustedWindowRect(windowRect);
                Rect rect = DpiHelper.DeviceRectToLogical(new Rect(windowRect.Left, windowRect.Top, windowRect.Width, windowRect.Height), dpi.DpiScaleX, dpi.DpiScaleY);
                Rect rect2 = DpiHelper.DeviceRectToLogical(new Rect(rECT.Left, rECT.Top, rECT.Width, rECT.Height), dpi.DpiScaleX, dpi.DpiScaleY);
                if (!Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 1))
                {
                    margin.Right -= SystemParameters.WindowResizeBorderThickness.Left;
                }

                if (!Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 4))
                {
                    margin.Right -= SystemParameters.WindowResizeBorderThickness.Right;
                }

                if (!Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 2))
                {
                    margin.Bottom -= SystemParameters.WindowResizeBorderThickness.Top;
                }

                if (!Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 8))
                {
                    margin.Bottom -= SystemParameters.WindowResizeBorderThickness.Bottom;
                }

                margin.Bottom -= SystemParameters.WindowCaptionHeight;
                if (_window.FlowDirection == FlowDirection.RightToLeft)
                {
                    Thickness thickness = new Thickness(rect.Left - rect2.Left, rect.Top - rect2.Top, rect2.Right - rect.Right, rect2.Bottom - rect.Bottom);
                    transform = new MatrixTransform(1.0, 0.0, 0.0, 1.0, 0.0 - (thickness.Left + thickness.Right), 0.0);
                }
                else
                {
                    transform = null;
                }

                frameworkElement.RenderTransform = transform;
            }

            frameworkElement.Margin = margin;
            if (Utility.IsPresentationFrameworkVersionLessThan4 && !_isFixedUp)
            {
                _hasUserMovedWindow = false;
                _window.StateChanged += _FixupRestoreBounds;
                _isFixedUp = true;
            }
        }



        private void _FixupRestoreBounds(object sender, EventArgs e)
        {
            if ((_window.WindowState == WindowState.Maximized || _window.WindowState == WindowState.Minimized) && _hasUserMovedWindow)
            {
                DpiScale dpi = DpiHelper.GetSystemDpi();
                _hasUserMovedWindow = false;
                WINDOWPLACEMENT windowPlacement = NativeMethods.GetWindowPlacement(_hwnd);
                RECT rECT = _GetAdjustedWindowRect(new RECT
                {
                    Bottom = 100,
                    Right = 100
                });
                Point point = DpiHelper.DevicePixelsToLogical(new Point(windowPlacement.rcNormalPosition.Left - rECT.Left, windowPlacement.rcNormalPosition.Top - rECT.Top), dpi.DpiScaleX, dpi.DpiScaleY);
                _window.Top = point.Y;
                _window.Left = point.X;
            }
        }


        private RECT _GetAdjustedWindowRect(RECT rcWindow)
        {
            var dwStyle = NativeMethods.GetWindowLongPtr(_hwnd, NativeMethods.GWL_STYLE);
            var dwExStyle = NativeMethods.GetWindowLongPtr(_hwnd, NativeMethods.GWL_EXSTYLE);
            return NativeMethods.AdjustWindowRectEx(rcWindow, dwStyle, bMenu: false, dwExStyle);
        }


        private IntPtr _WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            foreach (KeyValuePair<WM, MessageHandler> item in _messageTable)
            {
                if (item.Key == (WM)msg)
                {
                    return item.Value((WM)msg, wParam, lParam, out handled);
                }
            }

            return IntPtr.Zero;
        }


        private IntPtr _HandleSetTextOrIcon(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            bool flag = _ModifyStyle(WS.VISIBLE, WS.OVERLAPPED);
            IntPtr result = NativeMethods.DefWindowProc(_hwnd, uMsg, wParam, lParam);
            if (flag)
            {
                _ModifyStyle(WS.OVERLAPPED, WS.VISIBLE);
            }

            handled = true;
            return result;
        }


        private IntPtr _HandleNCActivate(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            IntPtr result = NativeMethods.DefWindowProc(_hwnd, WM.NCACTIVATE, wParam, new IntPtr(-1));
            handled = true;
            return result;
        }


        private IntPtr _HandleNCCalcSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if (_chromeInfo.NonClientFrameEdges != 0)
            {
                DpiScale dpi = DpiHelper.GetSystemDpi();
                Thickness thickness = DpiHelper.LogicalThicknessToDevice(SystemParameters.WindowResizeBorderThickness, dpi.DpiScaleX, dpi.DpiScaleY);
                RECT rECT = (RECT)Marshal.PtrToStructure(lParam, typeof(RECT));
                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 2))
                {
                    rECT.Top += (int)thickness.Top;
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 1))
                {
                    rECT.Left += (int)thickness.Left;
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 8))
                {
                    rECT.Bottom -= (int)thickness.Bottom;
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 4))
                {
                    rECT.Right -= (int)thickness.Right;
                }

                Marshal.StructureToPtr((object)rECT, lParam, fDeleteOld: false);
            }

            handled = true;
            IntPtr result = IntPtr.Zero;
            if (wParam.ToInt32() != 0)
            {
                result = new IntPtr(768);
            }

            return result;
        }



        private HT _GetHTFromResizeGripDirection(ResizeGripDirection direction)
        {
            bool flag = _window.FlowDirection == FlowDirection.RightToLeft;
            switch (direction)
            {
                case ResizeGripDirection.Bottom:
                    return HT.BOTTOM;
                case ResizeGripDirection.BottomLeft:
                    if (!flag)
                    {
                        return HT.BOTTOMLEFT;
                    }

                    return HT.BOTTOMRIGHT;
                case ResizeGripDirection.BottomRight:
                    if (!flag)
                    {
                        return HT.BOTTOMRIGHT;
                    }

                    return HT.BOTTOMLEFT;
                case ResizeGripDirection.Left:
                    if (!flag)
                    {
                        return HT.LEFT;
                    }

                    return HT.RIGHT;
                case ResizeGripDirection.Right:
                    if (!flag)
                    {
                        return HT.RIGHT;
                    }

                    return HT.LEFT;
                case ResizeGripDirection.Top:
                    return HT.TOP;
                case ResizeGripDirection.TopLeft:
                    if (!flag)
                    {
                        return HT.TOPLEFT;
                    }

                    return HT.TOPRIGHT;
                case ResizeGripDirection.TopRight:
                    if (!flag)
                    {
                        return HT.TOPRIGHT;
                    }

                    return HT.TOPLEFT;
                default:
                    return HT.NOWHERE;
            }
        }


        private IntPtr _HandleNCHitTest(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            DpiScale dpi = DpiHelper.GetSystemDpi();
            Point point = new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam));
            Rect deviceRectangle = _GetWindowRect();
            Point devicePoint = point;
            devicePoint.Offset(0.0 - deviceRectangle.X, 0.0 - deviceRectangle.Y);
            devicePoint = DpiHelper.DevicePixelsToLogical(devicePoint, dpi.DpiScaleX, dpi.DpiScaleY);
            IInputElement inputElement = _window.InputHitTest(devicePoint);
            if (inputElement != null)
            {
                if (MyChrome.GetIsHitTestVisibleInChrome(inputElement))
                {
                    handled = true;
                    return new IntPtr(1);
                }

                ResizeGripDirection resizeGripDirection = MyChrome.GetResizeGripDirection(inputElement);
                if (resizeGripDirection != 0)
                {
                    handled = true;
                    return new IntPtr((int)_GetHTFromResizeGripDirection(resizeGripDirection));
                }
            }

            if (_chromeInfo.UseAeroCaptionButtons && Utility.IsOSVistaOrNewer && _chromeInfo.GlassFrameThickness != default(Thickness) && _isGlassEnabled)
            {
                handled = NativeMethods.DwmDefWindowProc(_hwnd, uMsg, wParam, lParam, out var plResult);
                if (IntPtr.Zero != plResult)
                {
                    return plResult;
                }
            }

            HT value = _HitTestNca(DpiHelper.DeviceRectToLogical(deviceRectangle, dpi.DpiScaleX, dpi.DpiScaleY), DpiHelper.DevicePixelsToLogical(point, dpi.DpiScaleX, dpi.DpiScaleY));
            handled = true;
            return new IntPtr((int)value);
        }


        private IntPtr _HandleNCRButtonUp(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if (2 == wParam.ToInt32())
            {
                SystemCommands.ShowSystemMenu(_window, new Point(Utility.GET_X_LPARAM(lParam), Utility.GET_Y_LPARAM(lParam)));
            }

            handled = false;
            return IntPtr.Zero;
        }


        private IntPtr _HandleSize(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            WindowState? assumeState = null;
            if (wParam.ToInt32() == 2)
            {
                assumeState = WindowState.Maximized;
            }

            _UpdateSystemMenu(assumeState);
            handled = false;
            return IntPtr.Zero;
        }


        private IntPtr _HandleWindowPosChanged(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            WINDOWPOS value = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));
            if (!Utility.IsFlagSet(value.flags, 1))
            {
                _UpdateSystemMenu(null);
                if (!_isGlassEnabled)
                {
                    _SetRoundingRegion(value);

                  
                }
            }

            handled = false;
            return IntPtr.Zero;
        }


        private IntPtr _HandleDwmCompositionChanged(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            _UpdateFrameState(force: false);
            handled = false;
            return IntPtr.Zero;
        }


        private IntPtr _HandleSettingChange(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            _FixupTemplateIssues();
            handled = false;
            return IntPtr.Zero;
        }


        private IntPtr _HandleEnterSizeMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            _isUserResizing = true;
            if (_window.WindowState != WindowState.Maximized && !_IsWindowDocked)
            {
                _windowPosAtStartOfUserMove = new Point(_window.Left, _window.Top);
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleExitSizeMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            _isUserResizing = false;
            if (_window.WindowState == WindowState.Maximized)
            {
                _window.Top = _windowPosAtStartOfUserMove.Y;
                _window.Left = _windowPosAtStartOfUserMove.X;
            }

            handled = false;
            return IntPtr.Zero;
        }

        private IntPtr _HandleMove(WM uMsg, IntPtr wParam, IntPtr lParam, out bool handled)
        {
            if (_isUserResizing)
            {
                _hasUserMovedWindow = true;
            }

            handled = false;
            return IntPtr.Zero;
        }


        private bool _ModifyStyle(WS removeStyle, WS addStyle)
        {
            WS wS = (WS)NativeMethods.GetWindowLongPtr(_hwnd, NativeMethods.GWL_STYLE).ToInt32();
            WS wS2 = (wS & ~removeStyle) | addStyle;
            if (wS == wS2)
            {
                return false;
            }

            NativeMethods.SetWindowLongPtr(_hwnd, NativeMethods.GWL_STYLE, new IntPtr((int)wS2));
            return true;
        }


        private WindowState _GetHwndState()
        {
            WINDOWPLACEMENT windowPlacement = NativeMethods.GetWindowPlacement(_hwnd);
            return windowPlacement.showCmd switch
            {
                SW.SHOWMINIMIZED => WindowState.Minimized,
                SW.SHOWMAXIMIZED => WindowState.Maximized,
                _ => WindowState.Normal,
            };
        }


        private Rect _GetWindowRect()
        {
            RECT windowRect = NativeMethods.GetWindowRect(_hwnd);
            return new Rect(windowRect.Left, windowRect.Top, windowRect.Width, windowRect.Height);
        }


        private void _UpdateSystemMenu(WindowState? assumeState)
        {
            WindowState windowState = assumeState ?? _GetHwndState();
            if (!assumeState.HasValue && _lastMenuState == windowState)
            {
                return;
            }

            _lastMenuState = windowState;
            bool flag = _ModifyStyle(WS.VISIBLE, WS.OVERLAPPED);
            IntPtr systemMenu = NativeMethods.GetSystemMenu(_hwnd, bRevert: false);
            if (IntPtr.Zero != systemMenu)
            {
                WS value = (WS)NativeMethods.GetWindowLongPtr(_hwnd, GWL.STYLE).ToInt32();
                bool flag2 = Utility.IsFlagSet((int)value, 131072);
                bool flag3 = Utility.IsFlagSet((int)value, 65536);
                bool flag4 = Utility.IsFlagSet((int)value, 262144);
                switch (windowState)
                {
                    case WindowState.Maximized:
                        NativeMethods.EnableMenuItem(systemMenu, SC.RESTORE, MF.ENABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MOVE, MF.GRAYED | MF.DISABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.SIZE, MF.GRAYED | MF.DISABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MINIMIZE, (!flag2) ? (MF.GRAYED | MF.DISABLED) : MF.ENABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MAXIMIZE, MF.GRAYED | MF.DISABLED);
                        break;
                    case WindowState.Minimized:
                        NativeMethods.EnableMenuItem(systemMenu, SC.RESTORE, MF.ENABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MOVE, MF.GRAYED | MF.DISABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.SIZE, MF.GRAYED | MF.DISABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MINIMIZE, MF.GRAYED | MF.DISABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MAXIMIZE, (!flag3) ? (MF.GRAYED | MF.DISABLED) : MF.ENABLED);
                        break;
                    default:
                        NativeMethods.EnableMenuItem(systemMenu, SC.RESTORE, MF.GRAYED | MF.DISABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MOVE, MF.ENABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.SIZE, (!flag4) ? (MF.GRAYED | MF.DISABLED) : MF.ENABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MINIMIZE, (!flag2) ? (MF.GRAYED | MF.DISABLED) : MF.ENABLED);
                        NativeMethods.EnableMenuItem(systemMenu, SC.MAXIMIZE, (!flag3) ? (MF.GRAYED | MF.DISABLED) : MF.ENABLED);
                        break;
                }
            }

            if (flag)
            {
                _ModifyStyle(WS.OVERLAPPED, WS.VISIBLE);
            }
        }


        private void _UpdateFrameState(bool force)
        {
            if (IntPtr.Zero == _hwnd || _hwndSource.IsDisposed)
            {
                return;
            }

            bool flag = NativeMethods.DwmIsCompositionEnabled();
            if (force || flag != _isGlassEnabled)
            {
                _isGlassEnabled = flag && _chromeInfo.GlassFrameThickness != default(Thickness);
                if (!_isGlassEnabled)
                {
                    _SetRoundingRegion(null);
                }
                else
                {
                    _ClearRoundingRegion();
                    _ExtendGlassFrame();
                }

                NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0,SWP.DRAWFRAME | SWP.NOACTIVATE | SWP.NOMOVE | SWP.NOOWNERZORDER | SWP.NOSIZE | SWP.NOZORDER);
            }
        }


        private void _ClearRoundingRegion()
        {
            NativeMethods.SetWindowRgn(_hwnd, IntPtr.Zero, NativeMethods.IsWindowVisible(_hwnd));
        }


        private void _SetRoundingRegion(WINDOWPOS? wp)
        {
            WINDOWPLACEMENT windowPlacement = NativeMethods.GetWindowPlacement(_hwnd);
            if (windowPlacement.showCmd == SW.SHOWMAXIMIZED)
            {
                int num;
                int num2;
                if (wp.HasValue)
                {
                    num = wp.Value.x;
                    num2 = wp.Value.y;
                }
                else
                {
                    Rect rect = _GetWindowRect();
                    num = (int)rect.Left;
                    num2 = (int)rect.Top;
                }

                IntPtr hMonitor = NativeMethods.MonitorFromWindow(_hwnd, 2u);
                MONITORINFO monitorInfo = NativeMethods.GetMonitorInfo(hMonitor);
                RECT rcWork = monitorInfo.rcWork;
                rcWork.Offset(-num, -num2);
                IntPtr gdiObject = IntPtr.Zero;
                try
                {
                    gdiObject = NativeMethods.CreateRectRgnIndirect(rcWork);
                    NativeMethods.SetWindowRgn(_hwnd, gdiObject, NativeMethods.IsWindowVisible(_hwnd));
                    gdiObject = IntPtr.Zero;
                    return;
                }
                finally
                {
                    Utility.SafeDeleteObject(ref gdiObject);
                }
            }

            Size size;
            if (wp.HasValue && !Utility.IsFlagSet(wp.Value.flags, 1))
            {
                size = new Size(wp.Value.cx, wp.Value.cy);
            }
            else
            {
                if (wp.HasValue && _lastRoundingState == _window.WindowState)
                {
                    return;
                }

                size = _GetWindowRect().Size;
            }

            _lastRoundingState = _window.WindowState;
            IntPtr gdiObject2 = IntPtr.Zero;
            try
            {
                DpiScale dpi =DpiHelper.GetSystemDpi();
                double num3 = Math.Min(size.Width, size.Height);
                double x = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.TopLeft, 0.0), dpi.DpiScaleX, dpi.DpiScaleY).X;
                x = Math.Min(x, num3 / 2.0);
                if (_IsUniform(_chromeInfo.CornerRadius))
                {
                    gdiObject2 = _CreateRoundRectRgn(new Rect(size), x);
                }
                else
                {
                    gdiObject2 = _CreateRoundRectRgn(new Rect(0.0, 0.0, size.Width / 2.0 + x, size.Height / 2.0 + x), x);
                    double x2 = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.TopRight, 0.0), dpi.DpiScaleX, dpi.DpiScaleY).X;
                    x2 = Math.Min(x2, num3 / 2.0);
                    Rect region = new Rect(0.0, 0.0, size.Width / 2.0 + x2, size.Height / 2.0 + x2);
                    region.Offset(size.Width / 2.0 - x2, 0.0);
                    _CreateAndCombineRoundRectRgn(gdiObject2, region, x2);
                    double x3 = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.BottomLeft, 0.0), dpi.DpiScaleX, dpi.DpiScaleY).X;
                    x3 = Math.Min(x3, num3 / 2.0);
                    Rect region2 = new Rect(0.0, 0.0, size.Width / 2.0 + x3, size.Height / 2.0 + x3);
                    region2.Offset(0.0, size.Height / 2.0 - x3);
                    _CreateAndCombineRoundRectRgn(gdiObject2, region2, x3);
                    double x4 = DpiHelper.LogicalPixelsToDevice(new Point(_chromeInfo.CornerRadius.BottomRight, 0.0), dpi.DpiScaleX, dpi.DpiScaleY).X;
                    x4 = Math.Min(x4, num3 / 2.0);
                    Rect region3 = new Rect(0.0, 0.0, size.Width / 2.0 + x4, size.Height / 2.0 + x4);
                    region3.Offset(size.Width / 2.0 - x4, size.Height / 2.0 - x4);
                    _CreateAndCombineRoundRectRgn(gdiObject2, region3, x4);
                }

                NativeMethods.SetWindowRgn(_hwnd, gdiObject2, NativeMethods.IsWindowVisible(_hwnd));
                gdiObject2 = IntPtr.Zero;
                
            }
            finally
            {
                Utility.SafeDeleteObject(ref gdiObject2);
            }
        }


        private static IntPtr _CreateRoundRectRgn(Rect region, double radius)
        {
            if (DoubleUtil.AreClose(0.0, radius))
            {
                return NativeMethods.CreateRectRgn((int)Math.Floor(region.Left), (int)Math.Floor(region.Top), (int)Math.Ceiling(region.Right), (int)Math.Ceiling(region.Bottom));
            }

            return NativeMethods.CreateRoundRectRgn((int)Math.Floor(region.Left), (int)Math.Floor(region.Top), (int)Math.Ceiling(region.Right) + 1, (int)Math.Ceiling(region.Bottom) + 1, (int)Math.Ceiling(radius), (int)Math.Ceiling(radius));
        }


        private static void _CreateAndCombineRoundRectRgn(IntPtr hrgnSource, Rect region, double radius)
        {
            IntPtr gdiObject = IntPtr.Zero;
            try
            {
                gdiObject = _CreateRoundRectRgn(region, radius);
                if (NativeMethods.CombineRgn(hrgnSource, hrgnSource, gdiObject, RGN.OR) == CombineRgnResult.ERROR)
                {
                    throw new InvalidOperationException("Unable to combine two HRGNs.");
                }
            }
            catch
            {
                Utility.SafeDeleteObject(ref gdiObject);
                throw;
            }
        }

        private static bool _IsUniform(CornerRadius cornerRadius)
        {
            if (!DoubleUtil.AreClose(cornerRadius.BottomLeft, cornerRadius.BottomRight))
            {
                return false;
            }

            if (!DoubleUtil.AreClose(cornerRadius.TopLeft, cornerRadius.TopRight))
            {
                return false;
            }

            if (!DoubleUtil.AreClose(cornerRadius.BottomLeft, cornerRadius.TopRight))
            {
                return false;
            }

            return true;
        }


        private void _ExtendGlassFrame()
        {
            if (!Utility.IsOSVistaOrNewer || IntPtr.Zero == _hwnd)
            {
                return;
            }

            if (!NativeMethods.DwmIsCompositionEnabled())
            {
                _hwndSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;
                return;
            }

            DpiScale dpi =DpiHelper.GetSystemDpi();
            _hwndSource.CompositionTarget.BackgroundColor = Colors.Transparent;
            Thickness thickness = DpiHelper.LogicalThicknessToDevice(_chromeInfo.GlassFrameThickness, dpi.DpiScaleX, dpi.DpiScaleY);
            if (_chromeInfo.NonClientFrameEdges != 0)
            {
                Thickness thickness2 = DpiHelper.LogicalThicknessToDevice(SystemParameters.WindowResizeBorderThickness, dpi.DpiScaleX, dpi.DpiScaleY);
                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 2))
                {
                    thickness.Top -= thickness2.Top;
                    thickness.Top = Math.Max(0.0, thickness.Top);
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 1))
                {
                    thickness.Left -= thickness2.Left;
                    thickness.Left = Math.Max(0.0, thickness.Left);
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 8))
                {
                    thickness.Bottom -= thickness2.Bottom;
                    thickness.Bottom = Math.Max(0.0, thickness.Bottom);
                }

                if (Utility.IsFlagSet((int)_chromeInfo.NonClientFrameEdges, 4))
                {
                    thickness.Right -= thickness2.Right;
                    thickness.Right = Math.Max(0.0, thickness.Right);
                }
            }

            MARGINS mARGINS = default(MARGINS);
            mARGINS.cxLeftWidth = (int)Math.Ceiling(thickness.Left);
            mARGINS.cxRightWidth = (int)Math.Ceiling(thickness.Right);
            mARGINS.cyTopHeight = (int)Math.Ceiling(thickness.Top);
            mARGINS.cyBottomHeight = (int)Math.Ceiling(thickness.Bottom);
            MARGINS pMarInset = mARGINS;
            NativeMethods.DwmExtendFrameIntoClientArea(_hwnd, ref pMarInset);
        }

        private HT _HitTestNca(Rect windowPosition, Point mousePosition)
        {
            int num = 1;
            int num2 = 1;
            bool flag = false;
            if (mousePosition.Y >= windowPosition.Top && mousePosition.Y < windowPosition.Top + _chromeInfo.ResizeBorderThickness.Top + _chromeInfo.CaptionHeight)
            {
                flag = mousePosition.Y < windowPosition.Top + _chromeInfo.ResizeBorderThickness.Top;
                num = 0;
            }
            else if (mousePosition.Y < windowPosition.Bottom && mousePosition.Y >= windowPosition.Bottom - (double)(int)_chromeInfo.ResizeBorderThickness.Bottom)
            {
                num = 2;
            }

            if (mousePosition.X >= windowPosition.Left && mousePosition.X < windowPosition.Left + (double)(int)_chromeInfo.ResizeBorderThickness.Left)
            {
                num2 = 0;
            }
            else if (mousePosition.X < windowPosition.Right && mousePosition.X >= windowPosition.Right - _chromeInfo.ResizeBorderThickness.Right)
            {
                num2 = 2;
            }

            if (num == 0 && num2 != 1 && !flag)
            {
                num = 1;
            }

            HT hT = _HitTestBorders[num, num2];
            if (hT == HT.TOP && !flag)
            {
                hT = HT.CAPTION;
            }

            return hT;
        }


        private bool GetEffectiveClientArea(ref RECT rcClient)
        {
            if (_window == null || _chromeInfo == null)
            {
                return false;
            }

            DpiScale dpi = DpiHelper.GetSystemDpi() ;
            double captionHeight = _chromeInfo.CaptionHeight;
            Thickness resizeBorderThickness = _chromeInfo.ResizeBorderThickness;
            RECT windowRect = NativeMethods.GetWindowRect(_hwnd);
            Size size = DpiHelper.DeviceSizeToLogical(new Size(windowRect.Width, windowRect.Height), dpi.DpiScaleX, dpi.DpiScaleY);
            Point logicalPoint = new Point(resizeBorderThickness.Left, resizeBorderThickness.Top + captionHeight);
            Point logicalPoint2 = new Point(size.Width - resizeBorderThickness.Right, size.Height - resizeBorderThickness.Bottom);
            Point point = DpiHelper.LogicalPixelsToDevice(logicalPoint, dpi.DpiScaleX, dpi.DpiScaleY);
            Point point2 = DpiHelper.LogicalPixelsToDevice(logicalPoint2, dpi.DpiScaleX, dpi.DpiScaleY);
            rcClient.Left = (int)point.X;
            rcClient.Top = (int)point.Y;
            rcClient.Right = (int)point2.X;
            rcClient.Bottom = (int)point2.Y;
            return true;
        }


        private void _RestoreStandardChromeState(bool isClosing)
        {
            VerifyAccess();
            _UnhookCustomChrome();
            if (!isClosing && !_hwndSource.IsDisposed)
            {
                _RestoreFrameworkIssueFixups();
                _RestoreGlassFrame();
                _RestoreHrgn();
                _window.InvalidateMeasure();
            }
        }


        private void _UnhookCustomChrome()
        {
            if (_isHooked)
            {
                _hwndSource.RemoveHook(_WndProc);
                _isHooked = false;
            }
        }


        private void _RestoreFrameworkIssueFixups()
        {
            FrameworkElement frameworkElement = (FrameworkElement)VisualTreeHelper.GetChild(_window, 0);
            frameworkElement.Margin = default(Thickness);
            if (Utility.IsPresentationFrameworkVersionLessThan4)
            {
                _window.StateChanged -= _FixupRestoreBounds;
                _isFixedUp = false;
            }
        }


        private void _RestoreGlassFrame()
        {
            if (Utility.IsOSVistaOrNewer && !(_hwnd == IntPtr.Zero))
            {
                _hwndSource.CompositionTarget.BackgroundColor = SystemColors.WindowColor;
                if (NativeMethods.DwmIsCompositionEnabled())
                {
                    MARGINS pMarInset = default(MARGINS);
                    NativeMethods.DwmExtendFrameIntoClientArea(_hwnd, ref pMarInset);
                }
            }
        }


        private void _RestoreHrgn()
        {
            _ClearRoundingRegion();
            NativeMethods.SetWindowPos(_hwnd, IntPtr.Zero, 0, 0, 0, 0,SWP. DRAWFRAME | SWP.NOACTIVATE | SWP.NOMOVE | SWP.NOOWNERZORDER | SWP.NOSIZE | SWP.NOZORDER);
        }
    }
}
