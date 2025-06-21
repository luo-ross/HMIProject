using RS.Widgets.Interop;
using RS.Widgets.Structs;
using RS.Win32API;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using RS.Win32API.SafeHandles;

namespace RS.Widgets.Controls
{
    public class RSNotifyIcon : ContentControl
    {
        private HwndSource hwndSource;
        private object syncObj = new object();
        private const int WM_TRAYMOUSEMESSAGE = NativeMethods.WM_USER + 1024;
        private int id;
        private static int nextId = 0;

        #region 获取Icon方法  需要主动管理并且回收
        private IconHandle _defaultLargeIconHandle;
        private IconHandle _defaultSmallIconHandle;
        private IconHandle _currentLargeIconHandle;
        private IconHandle _currentSmallIconHandle;
        #endregion
        private NOTIFYICONDATA nOTIFYICONDATA;
        private bool DesignMode;
        private bool added;

        public RSNotifyIcon()
        {
            id = ++nextId;
            this.IsVisibleChanged += RSNotifyIcon_IsVisibleChanged;
            this.CreateHwndSource();
        }

        private void RSNotifyIcon_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateNotifyIcon(this.IsVisible);
        }

        public virtual void CreateHwndSource()
        {
            lock (this)
            {
                // 创建 HwndSourceParameters 这里创建的就是Native Window
                HwndSourceParameters parameters = new HwndSourceParameters(@$"RSNotifyIcon_{Guid.NewGuid().ToString()}");
                //这里控制窗体样式
                parameters.WindowClassStyle = 0;
                parameters.WindowStyle = 0;
                parameters.ExtendedWindowStyle = 0;
                // 创建 HwndSource
                hwndSource = new HwndSource(parameters);
                // 设置窗口过程处理
                hwndSource.AddHook(WndProc);
            }
        }

        /// <summary>
        /// Win32 消息监听
        /// </summary>
        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            switch (msg)
            {
                case WM_TRAYMOUSEMESSAGE:
                    switch (lParam)
                    {
                        case NativeMethods.WM_LBUTTONDBLCLK:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                            {
                                RoutedEvent = MouseDoubleClickEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_LBUTTONDOWN:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                            {
                                RoutedEvent = MouseLeftButtonDownEvent,
                                Source = this,
                            });
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                            {
                                RoutedEvent = MouseDownEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_LBUTTONUP:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                            {
                                RoutedEvent = MouseLeftButtonUpEvent,
                                Source = this,
                            });
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Left)
                            {
                                RoutedEvent = MouseUpEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_MBUTTONDBLCLK:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                            {
                                RoutedEvent = MouseDoubleClickEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_MBUTTONDOWN:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                            {
                                RoutedEvent = MouseDownEvent,
                                Source = this,
                            });
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                            {
                                RoutedEvent = MouseDownEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_MBUTTONUP:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Middle)
                            {
                                RoutedEvent = MouseUpEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_MOUSEMOVE:
                            this.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
                            {
                                RoutedEvent = MouseMoveEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_RBUTTONDBLCLK:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                            {
                                RoutedEvent = MouseDoubleClickEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_RBUTTONDOWN:
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                            {
                                RoutedEvent = MouseDownEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.WM_CONTEXTMENU:
                            if (this.ContextMenu != null)
                            {
                                ShowContextMenu();
                            }
                            break;
                        case NativeMethods.WM_RBUTTONUP:
                            if (this.ContextMenu != null)
                            {
                                ShowContextMenu();
                            }
                            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, MouseButton.Right)
                            {
                                RoutedEvent = MouseRightButtonUpEvent,
                                Source = this,
                            });
                            break;
                        case NativeMethods.NIN_BALLOONSHOW:
                            OnBalloonTipShown();
                            break;
                        case NativeMethods.NIN_BALLOONHIDE:
                            OnBalloonTipClosed();
                            break;
                        case NativeMethods.NIN_BALLOONTIMEOUT:
                            OnBalloonTipClosed();
                            break;
                        case NativeMethods.NIN_BALLOONUSERCLICK:
                            OnBalloonTipClicked();
                            break;
                    }
                    break;
                case NativeMethods.WM_DESTROY:
                    UpdateNotifyIcon(false);
                    break;
            }

            return IntPtr.Zero;
        }


        private void UpdateNotifyIcon(bool showIconInTray)
        {
            if (this.DesignMode)
            {
                return;
            }
            lock (syncObj)
            {
                if (this.nOTIFYICONDATA == null)
                {
                    this.nOTIFYICONDATA = new NOTIFYICONDATA();
                }
                this.nOTIFYICONDATA.uCallbackMessage = WM_TRAYMOUSEMESSAGE;
                this.nOTIFYICONDATA.uFlags = NativeMethods.NIF_MESSAGE;
                if (showIconInTray && this.hwndSource.Handle == IntPtr.Zero)
                {
                    this.CreateHwndSource();
                }

                this.nOTIFYICONDATA.hWnd = this.hwndSource.Handle;
                this.nOTIFYICONDATA.uID = id;
                this.nOTIFYICONDATA.hIcon = IntPtr.Zero;
                if (_currentSmallIconHandle != null)
                {
                    this.nOTIFYICONDATA.uFlags |= NativeMethods.NIF_ICON;
                    this.nOTIFYICONDATA.hIcon = _currentSmallIconHandle.CriticalGetHandle();
                }
                this.nOTIFYICONDATA.uFlags |= NativeMethods.NIF_TIP;
                this.nOTIFYICONDATA.szTip = this.Text;

                if (showIconInTray)
                {
                    if (!added)
                    {
                        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, this.nOTIFYICONDATA);
                        added = true;
                    }
                    else
                    {
                        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, this.nOTIFYICONDATA);
                    }
                }
                else if (added)
                {
                    NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, this.nOTIFYICONDATA);
                    added = false;
                }
            }
        }

        public ImageSource Icon
        {
            get { return (ImageSource)GetValue(IconProperty); }
            set { SetValue(IconProperty, value); }
        }

        public static readonly DependencyProperty IconProperty =
            DependencyProperty.Register("Icon", typeof(ImageSource), typeof(RSNotifyIcon), new PropertyMetadata(null, OnIconPropertyChanged));

        private static void OnIconPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            RSNotifyIcon rsNotifyIcon = (RSNotifyIcon)d;
            rsNotifyIcon.OnIconChanged(e.NewValue as ImageSource);
        }

        private void OnIconChanged(ImageSource newIcon)
        {
            this.UpdateIcon(newIcon);
            this.UpdateNotifyIcon(this.IsVisible);
        }


        private void UpdateIcon(ImageSource newIcon)
        {
            IconHandle largeIconHandle;
            IconHandle smallIconHandle;
            if (newIcon != null)
            {
                IconHelper.GetIconHandlesFromImageSource(newIcon, out largeIconHandle, out smallIconHandle);
            }
            //这里获取默认的Icon
            else if (_defaultLargeIconHandle == null && _defaultSmallIconHandle == null)
            {
                IconHelper.GetDefaultIconHandles(out largeIconHandle, out smallIconHandle);
                _defaultLargeIconHandle = largeIconHandle;
                _defaultSmallIconHandle = smallIconHandle;
            }
            else
            {
                largeIconHandle = _defaultLargeIconHandle;
                smallIconHandle = _defaultSmallIconHandle;
            }

            if (_currentLargeIconHandle != null && _currentLargeIconHandle != _defaultLargeIconHandle)
            {
                _currentLargeIconHandle.Dispose();
            }

            if (_currentSmallIconHandle != null && _currentSmallIconHandle != _defaultSmallIconHandle)
            {
                _currentSmallIconHandle.Dispose();
            }

            _currentLargeIconHandle = largeIconHandle;
            _currentSmallIconHandle = smallIconHandle;
        }


        public string BalloonTipText
        {
            get { return (string)GetValue(BalloonTipTextProperty); }
            set { SetValue(BalloonTipTextProperty, value); }
        }

        public static readonly DependencyProperty BalloonTipTextProperty =
            DependencyProperty.Register("BalloonTipText", typeof(string), typeof(RSNotifyIcon), new PropertyMetadata(default));

        public ToolTipIcon BalloonTipIcon
        {
            get { return (ToolTipIcon)GetValue(BalloonTipIconProperty); }
            set { SetValue(BalloonTipIconProperty, value); }
        }

        public static readonly DependencyProperty BalloonTipIconProperty =
            DependencyProperty.Register("BalloonTipIcon", typeof(ToolTipIcon), typeof(RSNotifyIcon), new PropertyMetadata(ToolTipIcon.None));

        public string BalloonTipTitle
        {
            get { return (string)GetValue(BalloonTipTitleProperty); }
            set { SetValue(BalloonTipTitleProperty, value); }
        }

        public static readonly DependencyProperty BalloonTipTitleProperty =
            DependencyProperty.Register("BalloonTipTitle", typeof(string), typeof(RSNotifyIcon), new PropertyMetadata(default));

        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(RSNotifyIcon), new PropertyMetadata(null, OnTextPropertyChanged));

        private static void OnTextPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsNotifyIcon = d as RSNotifyIcon;
            rsNotifyIcon.HandleTextPropertyChanged();
        }
        public void HandleTextPropertyChanged()
        {
            if (this.Text != null)
            {
                if (this.Text != null && this.Text.Length > 63)
                {
                    throw new ArgumentOutOfRangeException("Text", this.Text, "TrayIcon_TextTooLong");
                }
                if (this.added)
                {
                    this.UpdateNotifyIcon(true);
                }
            }
        }


        public static readonly RoutedEvent BalloonTipClickedEvent = EventManager.RegisterRoutedEvent(
            "BalloonTipClicked",
            RoutingStrategy.Direct,
            typeof(EventHandler),
            typeof(RSNotifyIcon));

        public event EventHandler BalloonTipClicked
        {
            add { AddHandler(BalloonTipClickedEvent, value); }
            remove { RemoveHandler(BalloonTipClickedEvent, value); }
        }


        public static readonly RoutedEvent BalloonTipClosedEvent = EventManager.RegisterRoutedEvent(
            "BalloonTipClosed",
            RoutingStrategy.Direct,
            typeof(EventHandler),
            typeof(RSNotifyIcon));

        public event EventHandler BalloonTipClosed
        {
            add { AddHandler(BalloonTipClosedEvent, value); }
            remove { RemoveHandler(BalloonTipClosedEvent, value); }
        }


        public static readonly RoutedEvent BalloonTipShownEvent = EventManager.RegisterRoutedEvent(
          "BalloonTipShown",
          RoutingStrategy.Direct,
          typeof(EventHandler),
          typeof(RSNotifyIcon));

        public event EventHandler BalloonTipShown
        {
            add { AddHandler(BalloonTipShownEvent, value); }
            remove { RemoveHandler(BalloonTipShownEvent, value); }
        }

        private void OnBalloonTipClicked()
        {
            // 触发事件
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = BalloonTipClickedEvent
            });
        }

        private void OnBalloonTipClosed()
        {
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = BalloonTipClosedEvent
            });
        }

        private void OnBalloonTipShown()
        {
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = BalloonTipShownEvent
            });
        }

        public void ShowBalloonTip(int timeout)
        {
            this.ShowBalloonTip(timeout, this.BalloonTipTitle, this.BalloonTipText, this.BalloonTipIcon, IntPtr.Zero);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            this.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon, IntPtr.Zero);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon, IntPtr balloonIconHandle)
        {

            if (timeout < 0)
            {
                throw new ArgumentOutOfRangeException("timeout", @$"InvalidArgument timeout {timeout.ToString(CultureInfo.CurrentCulture)}");
            }

            if (string.IsNullOrEmpty(tipText))
            {
                throw new ArgumentException("NotifyIconEmptyOrNullTipText");
            }

            //valid values are 0x0 to 0x3
            if (!ClientUtils.IsEnumValid(tipIcon, (int)tipIcon, (int)ToolTipIcon.None, (int)ToolTipIcon.Error))
            {
                throw new InvalidEnumArgumentException("tipIcon", (int)tipIcon, typeof(ToolTipIcon));
            }

            if (added && !this.DesignMode)
            {
                NOTIFYICONDATA data = new NOTIFYICONDATA();
                if (this.hwndSource == null)
                {
                    this.CreateHwndSource();
                }

                data.hWnd = this.hwndSource.Handle;
                data.uID = id;
                data.uFlags = NativeMethods.NIF_INFO;
                data.uTimeoutOrVersion = timeout;
                data.szInfoTitle = tipTitle;
                data.szInfo = tipText;
                data.hBalloonIcon = balloonIconHandle;
                switch (tipIcon)
                {
                    case ToolTipIcon.Info:
                        data.dwInfoFlags = 1;
                        break;
                    case ToolTipIcon.Warning:
                        data.dwInfoFlags = 2;
                        break;
                    case ToolTipIcon.Error:
                        data.dwInfoFlags = 3;
                        break;
                    case ToolTipIcon.None:
                        data.dwInfoFlags = 0;
                        break;
                }
                NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, data);
            }
        }

        private void ShowContextMenu()
        {
            if (this.ContextMenu != null)
            {
                POINT pOINT = new POINT();
                NativeMethods.GetCursorPos(pOINT);
                this.ContextMenu.IsOpen = true;
                this.ContextMenu.Placement = PlacementMode.AbsolutePoint;
                this.ContextMenu.HorizontalOffset = pOINT.x;
                this.ContextMenu.VerticalOffset = pOINT.y;

                // VS7 #38994
                // The solution to this problem was found in MSDN Article ID: Q135788.
                // Summary: the current window must be made the foreground window
                // before calling TrackPopupMenuEx, and a task switch must be
                // forced after the call.
                //NativeMethods.SetForegroundWindow(new HandleRef(this, this.hwndSource.Handle));
                //// 获取 PopupRoot 的句柄
                //var hwndSource = (HwndSource)PresentationSource.FromVisual(this.ContextMenu);
                //var contextMenuHandle = hwndSource?.Handle ?? IntPtr.Zero;
                //NativeMethods.TrackPopupMenuEx(new HandleRef(null, contextMenuHandle),
                //                         NativeMethods.TPM_VERTICAL | NativeMethods.TPM_RIGHTALIGN,
                //                         (int)pOINT.x,
                //                         (int)pOINT.y,
                //                         new HandleRef(window, window.Handle),
                //                         null);
                // Force task switch (see above)
                //NativeMethods.PostMessage(new HandleRef(this, this.hwndSource.Handle), NativeMethods.WM_NULL, (int)IntPtr.Zero, (int)IntPtr.Zero);
            }

        }
      
    }
}
