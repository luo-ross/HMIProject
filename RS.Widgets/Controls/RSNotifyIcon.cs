using RS.Widgets.Controls.Helpers;
using RS.Widgets.Events;
using RS.Widgets.Structs;
using RS.Win32API;
using RS.Win32API.Handles;
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
namespace RS.Widgets.Controls
{

    public class RSNotifyIcon : FrameworkElement, IDisposable
    {

        private class RSNotifyIconWindow : RSNativeWindow
        {
            internal RSNotifyIcon reference;

            private GCHandle rootRef;


            internal RSNotifyIconWindow(RSNotifyIcon component)
            {
                reference = component;
            }


            ~RSNotifyIconWindow()
            {
                if (base.Handle != IntPtr.Zero)
                {
                    NativeMethods.PostMessage(new HandleRef(this, base.Handle), NativeMethods.WM_CLOSE, 0, 0);
                }
            }

            public void LockReference(bool locked)
            {
                if (locked)
                {
                    if (!rootRef.IsAllocated)
                    {
                        rootRef = GCHandle.Alloc(reference, GCHandleType.Normal);
                    }
                }
                else if (rootRef.IsAllocated)
                {
                    rootRef.Free();
                }
            }

            protected override void OnThreadException(Exception e)
            {

            }

            protected override void WndProc(ref Message m)
            {
                reference.WndProc(ref m);
            }
        }


        private bool DesignMode;
        private int id;
        private static int nextId = 0;

        private object syncObj = new object();
        private const int WM_TRAYMOUSEMESSAGE = NativeMethods.WM_USER + 1024;
        private static int WM_TASKBARCREATED = NativeMethods.RegisterWindowMessage("TaskbarCreated");

        private RSNotifyIconWindow window;
        private bool added;
        private ImageSource _icon;
        #region 获取Icon方法  需要主动管理并且回收
        private IconHandle _defaultLargeIconHandle;
        private IconHandle _defaultSmallIconHandle;
        private IconHandle _currentLargeIconHandle;
        private IconHandle _currentSmallIconHandle;
        #endregion

        private bool doubleClick;
        public RSNotifyIcon()
        {


            DesignMode = DesignerProperties.GetIsInDesignMode(this);
            this.IsVisibleChanged += RSNotifyIcon_IsVisibleChanged;
            id = ++nextId;
            window = new RSNotifyIconWindow(this);
            this.UpdateNotifyIcon(this.IsVisible);
        }



        ~RSNotifyIcon()
        {
            Dispose(disposing: false);
        }


        public void Dispose()
        {
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }

        protected void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }

            if (disposing)
            {
                if (window != null)
                {
                    _icon = null;
                    _defaultLargeIconHandle?.Dispose();
                    _defaultSmallIconHandle?.Dispose();
                    _currentLargeIconHandle?.Dispose();
                    _currentSmallIconHandle?.Dispose();
                    Text = string.Empty;
                    this.UpdateNotifyIcon(showIconInTray: false);
                    window.DestroyHandle();
                    window = null;
                    this.ContextMenu = null;

                }
            }
            else if (window != null && window.Handle != IntPtr.Zero)
            {
                NativeMethods.PostMessage(new HandleRef(window, window.Handle), NativeMethods.WM_CLOSE, 0, 0);
                window.ReleaseHandle();
            }

            this.Dispose(disposing);
        }


        private void RSNotifyIcon_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            this.UpdateNotifyIcon(this.IsVisible);
        }

        private void UpdateNotifyIcon(bool showIconInTray)
        {
            lock (syncObj)
            {
                if (this.DesignMode)
                {
                    return;
                }

                window.LockReference(showIconInTray);

                NOTIFYICONDATA nOTIFYICONDATA = new NOTIFYICONDATA();
                //这里uCallbackMessage 存在风险 待处理
                nOTIFYICONDATA.uCallbackMessage = WM_TRAYMOUSEMESSAGE;
                nOTIFYICONDATA.uFlags = NativeMethods.NIF_MESSAGE;
                if (showIconInTray && window.Handle == IntPtr.Zero)
                {
                    window.CreateHandle(new CreateParams());
                }

                nOTIFYICONDATA.hWnd = window.Handle;
                nOTIFYICONDATA.uID = id;
                nOTIFYICONDATA.hIcon = IntPtr.Zero;
                nOTIFYICONDATA.szTip = null;


                if (_currentSmallIconHandle != null)
                {
                    nOTIFYICONDATA.uFlags |= NativeMethods.NIF_ICON;
                    nOTIFYICONDATA.hIcon = _currentSmallIconHandle.CriticalGetHandle();
                }
                nOTIFYICONDATA.uFlags |= NativeMethods.NIF_TIP;
                nOTIFYICONDATA.szTip = this.Text;

                if (showIconInTray && _currentSmallIconHandle != null)
                {
                    if (!added)
                    {
                        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_ADD, nOTIFYICONDATA);
                        added = true;
                    }
                    else
                    {
                        NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, nOTIFYICONDATA);
                    }
                }
                else if (added)
                {
                    NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_DELETE, nOTIFYICONDATA);
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
            _icon = newIcon;
            this.UpdateIcon();
            this.UpdateNotifyIcon(this.IsVisible);
        }


        private void UpdateIcon()
        {
            IconHandle largeIconHandle;
            IconHandle smallIconHandle;
            if (_icon != null)
            {
                IconHelper.GetIconHandlesFromImageSource(_icon, out largeIconHandle, out smallIconHandle);
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

            HandleRef[] array = new HandleRef[2]
            {
            new HandleRef(this, window.Handle),
            default(HandleRef)
            };

            //这里是Window Icon更改通知方法
            //int num = 1;
            //if (_hiddenWindow != null)
            //{
            //    array[1] = new HandleRef(_hiddenWindow, _hiddenWindow.Handle);
            //    num++;
            //}

            //for (int i = 0; i < num; i++)
            //{
            //    HandleRef hWnd = array[i];
            //    NativeMethods.SendMessage(hWnd, NativeMethods.WM_SETICON, (IntPtr)1, largeIconHandle);
            //    NativeMethods.SendMessage(hWnd, NativeMethods.WM_SETICON, (IntPtr)0, smallIconHandle);
            //}

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

        private void WndProc(ref Message msg)
        {

            switch (msg.Msg)
            {
                case WM_TRAYMOUSEMESSAGE:
                    switch ((int)msg.LParam)
                    {
                        case NativeMethods.WM_LBUTTONDBLCLK:
                            WmMouseDown(ref msg, MouseButton.Left, 2);
                            break;
                        case NativeMethods.WM_LBUTTONDOWN:
                            WmMouseDown(ref msg, MouseButton.Left, 1);
                            break;
                        case NativeMethods.WM_LBUTTONUP:
                            WmMouseUp(ref msg, MouseButton.Left);
                            break;
                        case NativeMethods.WM_MBUTTONDBLCLK:
                            WmMouseDown(ref msg, MouseButton.Middle, 2);
                            break;
                        case NativeMethods.WM_MBUTTONDOWN:
                            WmMouseDown(ref msg, MouseButton.Middle, 1);
                            break;
                        case NativeMethods.WM_MBUTTONUP:
                            WmMouseUp(ref msg, MouseButton.Left);
                            break;
                        case NativeMethods.WM_MOUSEMOVE:
                            WmMouseMove(ref msg);
                            break;
                        case NativeMethods.WM_RBUTTONDBLCLK:
                            WmMouseDown(ref msg, MouseButton.Right, 2);
                            break;
                        case NativeMethods.WM_RBUTTONDOWN:
                            WmMouseDown(ref msg, MouseButton.Right, 1);
                            break;

                        case NativeMethods.WM_CONTEXTMENU:



                            break;


                        case NativeMethods.WM_RBUTTONUP:
                            if (this.ContextMenu != null)
                            {
                                ShowContextMenu();
                            }
                            WmMouseUp(ref msg, MouseButton.Right);
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
                case NativeMethods.WM_COMMAND:
                    //if (IntPtr.Zero == msg.LParam)
                    //{
                    //    if (Command.DispatchID((int)msg.WParam & 0xFFFF)) return;
                    //}
                    //else
                    //{
                    //    window.DefWndProc(ref msg);
                    //}
                    break;
                case NativeMethods.WM_DRAWITEM:
                    // If the wparam is zero, then the message was sent by a menu.
                    // See WM_DRAWITEM in MSDN.
                    if (msg.WParam == IntPtr.Zero)
                    {
                        WmDrawItemMenuItem(ref msg);
                    }
                    break;
                case NativeMethods.WM_MEASUREITEM:
                    // If the wparam is zero, then the message was sent by a menu.
                    if (msg.WParam == IntPtr.Zero)
                    {
                        WmMeasureMenuItem(ref msg);
                    }
                    break;

                case NativeMethods.WM_INITMENUPOPUP:
                    WmInitMenuPopup(ref msg);
                    break;

                case NativeMethods.WM_DESTROY:
                    // Remove the icon from the taskbar
                    UpdateNotifyIcon(false);
                    break;

                default:
                    if (msg.Msg == WM_TASKBARCREATED)
                    {
                        WmTaskbarCreated(ref msg);
                    }
                    window.DefWndProc(ref msg);
                    break;
            }
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




        public static readonly RoutedEvent ClickEvent = EventManager.RegisterRoutedEvent(
          "Click",
          RoutingStrategy.Direct,
          typeof(RSMouseButtonEventHandler),
          typeof(RSNotifyIcon));

        public event RSMouseButtonEventHandler Click
        {
            add { AddHandler(ClickEvent, value); }
            remove { RemoveHandler(ClickEvent, value); }
        }


        public static readonly RoutedEvent DoubleClickEvent = EventManager.RegisterRoutedEvent(
        "DoubleClick",
        RoutingStrategy.Direct,
        typeof(RSMouseButtonEventHandler),
        typeof(RSNotifyIcon));

        public event RSMouseButtonEventHandler DoubleClick
        {
            add { AddHandler(DoubleClickEvent, value); }
            remove { RemoveHandler(DoubleClickEvent, value); }
        }


        public static readonly RoutedEvent MouseClickEvent = EventManager.RegisterRoutedEvent(
        "MouseClick",
        RoutingStrategy.Direct,
        typeof(RSMouseButtonEventHandler),
        typeof(RSNotifyIcon));

        public event RSMouseButtonEventHandler MouseClick
        {
            add { AddHandler(MouseClickEvent, value); }
            remove { RemoveHandler(MouseClickEvent, value); }
        }

        public static readonly RoutedEvent MouseDoubleClickEvent = EventManager.RegisterRoutedEvent(
      "MouseDoubleClick",
      RoutingStrategy.Direct,
      typeof(RSMouseButtonEventHandler),
      typeof(RSNotifyIcon));

        public event RSMouseButtonEventHandler MouseDoubleClick
        {
            add { AddHandler(MouseDoubleClickEvent, value); }
            remove { RemoveHandler(MouseDoubleClickEvent, value); }
        }


        /// <devdoc>
        ///    <para>
        ///       This method raised the BalloonTipClicked event. 
        ///    </para>
        /// </devdoc>
        private void OnBalloonTipClicked()
        {
            // 触发事件
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = BalloonTipClickedEvent
            });
        }

        /// <devdoc>
        ///    <para>
        ///       This method raised the BalloonTipClosed event. 
        ///    </para>
        /// </devdoc>
        private void OnBalloonTipClosed()
        {
            // 触发事件
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = BalloonTipClosedEvent
            });
        }

        /// <devdoc>
        ///    <para>
        ///       This method raised the BalloonTipShown event. 
        ///    </para>
        /// </devdoc>
        private void OnBalloonTipShown()
        {
            // 触发事件
            RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
            {
                RoutedEvent = BalloonTipShownEvent
            });
        }



        /// <devdoc>
        ///    <para>
        ///       Displays a balloon tooltip in the taskbar.
        /// 
        ///       The system enforces minimum and maximum timeout values. Timeout 
        ///       values that are too large are set to the maximum value and values 
        ///       that are too small default to the minimum value. The operating system's 
        ///       default minimum and maximum timeout values are 10 seconds and 30 seconds, 
        ///       respectively.
        ///       
        ///       No more than one balloon ToolTip at at time is displayed for the taskbar. 
        ///       If an application attempts to display a ToolTip when one is already being displayed, 
        ///       the ToolTip will not appear until the existing balloon ToolTip has been visible for at 
        ///       least the system minimum timeout value. For example, a balloon ToolTip with timeout 
        ///       set to 30 seconds has been visible for seven seconds when another application attempts 
        ///       to display a balloon ToolTip. If the system minimum timeout is ten seconds, the first 
        ///       ToolTip displays for an additional three seconds before being replaced by the second ToolTip.
        ///    </para>
        /// </devdoc>
        public void ShowBalloonTip(int timeout)
        {
           this. ShowBalloonTip(timeout, this.BalloonTipTitle, this.BalloonTipText, this.BalloonTipIcon, IntPtr.Zero);
        }

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon)
        {
            this.ShowBalloonTip(timeout, tipTitle, tipText, tipIcon, IntPtr.Zero);
        }


        /// <devdoc>
        ///    <para>
        ///       Displays a balloon tooltip in the taskbar with the specified title,
        ///       text, and icon for a duration of the specified timeout value.
        /// 
        ///       The system enforces minimum and maximum timeout values. Timeout 
        ///       values that are too large are set to the maximum value and values 
        ///       that are too small default to the minimum value. The operating system's 
        ///       default minimum and maximum timeout values are 10 seconds and 30 seconds, 
        ///       respectively.
        ///       
        ///       No more than one balloon ToolTip at at time is displayed for the taskbar. 
        ///       If an application attempts to display a ToolTip when one is already being displayed, 
        ///       the ToolTip will not appear until the existing balloon ToolTip has been visible for at 
        ///       least the system minimum timeout value. For example, a balloon ToolTip with timeout 
        ///       set to 30 seconds has been visible for seven seconds when another application attempts 
        ///       to display a balloon ToolTip. If the system minimum timeout is ten seconds, the first 
        ///       ToolTip displays for an additional three seconds before being replaced by the second ToolTip.
        ///    </para>
        /// </devdoc>

        public void ShowBalloonTip(int timeout, string tipTitle, string tipText, ToolTipIcon tipIcon,IntPtr balloonIconHandle)
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
                NOTIFYICONDATA nOTIFYICONDATA = new NOTIFYICONDATA();
                if (window.Handle == IntPtr.Zero)
                {
                    window.CreateHandle(new CreateParams());
                }

                nOTIFYICONDATA.hWnd = window.Handle;
                nOTIFYICONDATA.uID = id;
                nOTIFYICONDATA.uFlags = NativeMethods.NIF_INFO;
                nOTIFYICONDATA.uTimeoutOrVersion = timeout;
                nOTIFYICONDATA.szInfoTitle = tipTitle;
                nOTIFYICONDATA.szInfo = tipText;

                //获取自定义iconHandle


                nOTIFYICONDATA.hBalloonIcon = balloonIconHandle;
                switch (tipIcon)
                {
                    case ToolTipIcon.Info:
                        nOTIFYICONDATA.dwInfoFlags = 1;
                        break;
                    case ToolTipIcon.Warning:
                        nOTIFYICONDATA.dwInfoFlags = 2;
                        break;
                    case ToolTipIcon.Error:
                        nOTIFYICONDATA.dwInfoFlags = 3;
                        break;
                    case ToolTipIcon.None:
                        nOTIFYICONDATA.dwInfoFlags = 0;
                        break;
                }
                NativeMethods.Shell_NotifyIcon(NativeMethods.NIM_MODIFY, nOTIFYICONDATA);
            }
        }

        /// <devdoc>
        ///     Shows the context menu for the tray icon.
        /// </devdoc>
        /// <internalonly/>
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
                NativeMethods.SetForegroundWindow(new HandleRef(window, window.Handle));

                // 获取 PopupRoot 的句柄
                var hwndSource = (HwndSource)PresentationSource.FromVisual(this.ContextMenu);
                var contextMenuHandle = hwndSource?.Handle ?? IntPtr.Zero;

                //NativeMethods.TrackPopupMenuEx(new HandleRef(null, contextMenuHandle),
                //                         NativeMethods.TPM_VERTICAL | NativeMethods.TPM_RIGHTALIGN,
                //                         (int)pOINT.x,
                //                         (int)pOINT.y,
                //                         new HandleRef(window, window.Handle),
                //                         null);

                // Force task switch (see above)
                NativeMethods.PostMessage(new HandleRef(window, window.Handle), NativeMethods.WM_NULL, (int)IntPtr.Zero, (int)IntPtr.Zero);
            }

        }








        private void WmMouseDown(ref Message m, MouseButton button, int clicks)
        {
            if (clicks == 2)
            {
                this.RaiseEvent(new RSMouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button, clicks)
                {
                    RoutedEvent = DoubleClickEvent,
                });

                this.RaiseEvent(new RSMouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button, clicks)
                {
                    RoutedEvent = MouseDoubleClickEvent,
                });

                doubleClick = true;
            }

            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
            {
                RoutedEvent = MouseDownEvent,
            });
        }

        /// <devdoc>
        ///     Handles the mouse-move event
        /// </devdoc>
        /// <internalonly/>
        private void WmMouseMove(ref Message m)
        {
            this.RaiseEvent(new MouseEventArgs(Mouse.PrimaryDevice, Environment.TickCount)
            {
                RoutedEvent = MouseMoveEvent,
            });
        }

        /// <devdoc>
        ///     Handles the mouse-up event
        /// </devdoc>
        /// <internalonly/>
        private void WmMouseUp(ref Message m, MouseButton button)
        {
            this.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
            {
                RoutedEvent = MouseUpEvent,
            });

            //subhag
            if (!doubleClick)
            {
                RaiseEvent(new RSMouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                {
                    RoutedEvent = ClickEvent,
                });

                RaiseEvent(new RSMouseButtonEventArgs(Mouse.PrimaryDevice, Environment.TickCount, button)
                {
                    RoutedEvent = MouseClickEvent,
                });

            }
            doubleClick = false;
        }

        private void WmTaskbarCreated(ref Message m)
        {
            added = false;
            UpdateNotifyIcon(this.IsVisible);
        }



        private void WmInitMenuPopup(ref Message m)
        {
            //if (contextMenu != null)
            //{
            //    if (contextMenu.ProcessInitMenuPopup(m.WParam))
            //    {
            //        return;
            //    }
            //}

            //window.DefWndProc(ref m);
        }

        private void WmMeasureMenuItem(ref Message m)
        {
            // // Obtain the menu item object
            //MEASUREITEMSTRUCT mis = (MEASUREITEMSTRUCT)m.GetLParam(typeof(MEASUREITEMSTRUCT));

            // Debug.Assert(m.LParam != IntPtr.Zero, "m.lparam is null");

            // // A pointer to the correct MenuItem is stored in the measure item
            // // information sent with the message.
            // // (See MenuItem.CreateMenuItemInfo)
            // MenuItem menuItem = MenuItem.GetMenuItemFromItemData(mis.itemData);
            // Debug.Assert(menuItem != null, "UniqueID is not associated with a menu item");

            // // Delegate this message to the menu item
            // if (menuItem != null)
            // {
            //     menuItem.WmMeasureItem(ref m);
            // }
        }

        private void WmDrawItemMenuItem(ref Message m)
        {
            //// Obtain the menu item object
            //NativeMethods.DRAWITEMSTRUCT dis = (NativeMethods.DRAWITEMSTRUCT)m.GetLParam(typeof(NativeMethods.DRAWITEMSTRUCT));

            //// A pointer to the correct MenuItem is stored in the draw item
            //// information sent with the message.
            //// (See MenuItem.CreateMenuItemInfo)
            //MenuItem menuItem = MenuItem.GetMenuItemFromItemData(dis.itemData);

            //// Delegate this message to the menu item
            //if (menuItem != null)
            //{
            //    menuItem.WmDrawItem(ref m);
            //}
        }




    }






















}
