using NPOI.OpenXmlFormats.Wordprocessing;
using Org.BouncyCastle.Pqc.Crypto.Lms;
using RS.Commons.Helper;
using RS.Widgets.Extensions;
using RS.Widgets.Models;
using RS.Win32API;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shell;

namespace RS.Widgets.Controls
{

    public class RSDragPreview : Window
    {
        private RSDockGrid DockGrid;

     

        /// <summary>
        /// Dock位置参考窗体类型1
        /// </summary>
        private RSDockPositionWindow DockPositionWindow1 { get; set; }

        /// <summary>
        /// Dock位置参考窗体类型2
        /// </summary>
        private RSDockPositionWindow DockPositionWindow2 { get; set; }

        /// <summary>
        /// Dock位置参考窗体类型3
        /// </summary>
        private RSDockPositionWindow DockPositionWindow3 { get; set; }

        ///// <summary>
        ///// 窗体位置信息
        ///// </summary>
        //private HashSet<DockWindowInfo> DockWindowInfoList = new HashSet<DockWindowInfo>();

        public HwndSource HwndSource { get; private set; }

        /// <summary>
        /// 当前窗体的句柄
        /// </summary>
        public IntPtr Handle { get; private set; }

        /// <summary>
        /// 拖拽窗体工作目标DockGrid
        /// </summary>
        public RSDockGrid TargetDockGrid { get; private set; }

        /// <summary>
        /// 拖拽窗体工作目标DockPanel
        /// </summary>
        public RSDockPanel TargetDockPanel { get; private set; }

        /// <summary>
        /// 当前拖拽窗口信息
        /// </summary>
        private RSDockPanel TargetDockPanelHistory { get; set; }
   


        static RSDragPreview()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDragPreview), new FrameworkPropertyMetadata(typeof(RSDragPreview)));
        }

        private RSDragPreview()
        {
            //this.WindowStyle = WindowStyle.None;
            //this.AllowsTransparency = true;

            this.Left = int.MinValue;
            this.Top = int.MinValue;
            this.ShowInTaskbar = false;

            //this.PreviewMouseLeftButtonDown += RSDragPreview_PreviewMouseLeftButtonDown;
            //this.PreviewMouseLeftButtonUp += RSDragPreview_PreviewMouseLeftButtonUp;
            this.Loaded += RSDragDropPreview_Loaded;
        }



        private void RSDragDropPreview_Loaded(object sender, RoutedEventArgs e)
        {
            this.SetHitTestable(this.IsHitTestVisible);
        }


        public RSDragPreview(RSDockPanel dockPanel,
         double visualWith,
         double visualHeight) : this()
        {
            DockGrid = new RSDockGrid()
            {
                Margin = new Thickness(0)
            };
            DockGrid.Children.Add(dockPanel);
            this.Content = DockGrid;
            this.Width = visualWith;
            this.Height = visualHeight;
            dockPanel.IsWindowDockPanel = true;
        }

        public RSDragPreview(UIElement visual, double visualWith,
         double visualHeight) : this()
        {
            this.IsHitTestVisible = false;
            this.Content = visual;
            this.Width = visualWith;
            this.Height = visualHeight;
            this.Opacity = 0.8;
        }



        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HwndSource = (HwndSource)PresentationSource.FromDependencyObject(this);
            this.Handle = this.HwndSource.Handle;
            this.HwndSource.AddHook(WndProc);
        }


        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            int hitTestResult = 0;
            switch (msg)
            {
                case NativeMethods.WM_ENTERSIZEMOVE:
                    this.OnWindowDragStarted();
                    break;
                case NativeMethods.WM_EXITSIZEMOVE:
                    this.OnWindowDragCompleted();
                    break;
                case NativeMethods.WM_MOVING:
                    this.OnWindowDragMoving();
                    break;
                case NativeMethods.WM_NCLBUTTONDOWN:
                    this.ActiveRSDockPanel();
                    break;
                case NativeMethods.WM_NCRBUTTONUP:
                    this.ActiveRSDockPanel();
                    this.ShowCaptionContextMenu();
                    handled = true;
                    break;
            }
            return IntPtr.Zero;
        }

        private void ActiveRSDockPanel()
        {
            if (this.DockGrid.Children.Count > 1)
            {
                return;
            }
            var dockPanelList = RSDockPanel.GetDockPanelListUnderCursorPos();
            var dockPanel = dockPanelList.FirstOrDefault(t => t.ParentWindowHandle == this.Handle);
            dockPanel?.ActiveRSDockPanel();
        }

        private void ShowCaptionContextMenu()
        {
            if (this.DockGrid.Children.Count > 1)
            {
                return;
            }
            var dockPanelList = RSDockPanel.GetDockPanelListUnderCursorPos();
            var dockPanel = dockPanelList.FirstOrDefault(t => t.ParentWindowHandle == this.Handle);
            dockPanel?.ShowCaptionContextMenu();
        }


        public void OnWindowDragStarted()
        {
            
        }



        public void OnWindowDragMoving()
        {
            //获取当前鼠标位置
            var cursorPos = NativeMethods.GetCursorPos();

            //获取到鼠标底下的DockPanel列表 不包含当前窗体下的Panel
            var dockPanelList = RSDockPanel.GetDockPanelList(cursorPos)
                .Where(t => t.ParentWindowHandle != this.Handle)
                .ToList();

            //获取鼠标底下DockGrid容器
           this.TargetDockGrid = RSDockGrid.GetRSDockGridList(cursorPos)
                .FirstOrDefault(t => t.ParentWindow == this.Owner);

            //获取到当前桌面上窗体的ZIndex顺序
            var systemTopLevelWindowsZOrder = GetSystemTopLevelWindowsZOrder();

            //数据取交集 
            var systemTopLevelWindowsZOrderFilter = systemTopLevelWindowsZOrder
                  .Join(dockPanelList.Select(t => t.ParentWindowHandle).ToList(),
                  a => a,
                  b => b,
                  (a, b) => a).ToList();

            //让Panel矩形位置按照窗口ZIndex顺序进行排序
            var dockPanelSortedList = dockPanelList.SortByReferenceList(systemTopLevelWindowsZOrderFilter, a => a.ParentWindowHandle, b => b);

            //获取到拖拽时窗体底下的目标Panel
            this.TargetDockPanel = dockPanelSortedList.FirstOrDefault();

            if (this.TargetDockGrid == null)
            {
                DockPositionWindow1?.Close();
                DockPositionWindow1 = null;
            }

            IntPtr handle;
            RECT rect;
            //处理主DockGrid 位置提示
            if (this.TargetDockGrid != null)
            {
                //创建位置参考类型1的提示
                if (DockPositionWindow1 == null)
                {
                    DockPositionWindow1 = new RSDockPositionWindow(this, this.TargetDockGrid);
                    DockPositionWindow1.DockPositionWindowType = DockPositionWindowType.Type1;
                    DockPositionWindow1.ShowActivated = false;
                    DockPositionWindow1.Show();
                }
            }


            if (this.TargetDockPanel == null || this.TargetDockPanel != TargetDockPanelHistory)
            {
                TargetDockPanelHistory = this.TargetDockPanel;
                DockPositionWindow2?.Close();
                DockPositionWindow3?.Close();
                DockPositionWindow2 = null;
                DockPositionWindow3 = null;
            }


            //处理DockPanel 位置提示
            if (this.TargetDockPanel != null)
            {
                Window targetDockPanelWindow = this.TargetDockPanel.ParentWindow;
                IntPtr targetDockPanelWindowHandle = this.TargetDockPanel.ParentWindowHandle;
                //将目标Panel所在窗体设置到最前面
                //只要这个窗体的OwnedWindows数量为0 则需要将窗体设置到当前拖拽窗体的下面
                if (targetDockPanelWindow.OwnedWindows.Count == 0)
                {
                    NativeMethods.SetWindowPos(targetDockPanelWindowHandle,
                        this.HwndSource.Handle,
                        0,
                        0,
                        0,
                        0,
                        NativeMethods.SWP_NOMOVE
                        | NativeMethods.SWP_NOSIZE
                        | NativeMethods.SWP_NOACTIVATE
                        | NativeMethods.SWP_NOOWNERZORDER);
                }

                if (DockPositionWindow3 == null)
                {
                    DockPositionWindow3 = new RSDockPositionWindow(this, this.TargetDockPanel);
                    DockPositionWindow3.DockPositionWindowType = DockPositionWindowType.Type3;
                    DockPositionWindow3.ShowActivated = false;
                    DockPositionWindow3.Show();
                }
            }


            var panelPositionInfo3 = DockPositionWindow3?.GetPanelPositionInfo(cursorPos);
            var panelPositionInfo2 = DockPositionWindow2?.GetPanelPositionInfo(cursorPos);
            var panelPositionInfo1 = DockPositionWindow1?.GetPanelPositionInfo(cursorPos);

            //优先面板的的先触发
            if (panelPositionInfo3 != null && DockPositionWindow3 != null)
            {
                DockPositionWindow3.MouseMoveEventTrigger(panelPositionInfo3);
            }
            else if (panelPositionInfo2 != null && DockPositionWindow2 != null)
            {
                DockPositionWindow2 .MouseMoveEventTrigger(panelPositionInfo2);
            }
            else if (panelPositionInfo1 != null && DockPositionWindow1 != null)
            {
                DockPositionWindow1.MouseMoveEventTrigger(panelPositionInfo1);
            }
        }


        /// <summary>
        /// 获取系统中所有顶级窗口的Z顺序（从顶层到底层）
        /// </summary>
        public static List<IntPtr> GetSystemTopLevelWindowsZOrder()
        {
            var zOrder = new List<IntPtr>();
            IntPtr currentWindow = NativeMethods.GetTopWindow(IntPtr.Zero);
            while (currentWindow != IntPtr.Zero)
            {
                zOrder.Add(currentWindow);
                currentWindow = NativeMethods.GetWindow(currentWindow, NativeMethods.GW_HWNDNEXT);
            }
            return zOrder;
        }

        public void OnWindowDragCompleted()
        {
            CloseDockPositionWindow();
        }

        private void CloseDockPositionWindow()
        {
            DockPositionWindow1?.Close();
            DockPositionWindow2?.Close();
            DockPositionWindow3?.Close();
            DockPositionWindow1 = null;
            DockPositionWindow2 = null;
            DockPositionWindow3 = null;
            TargetDockPanelHistory = null;
        }

        [Description("标题栏高度设置")]
        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }

        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(RSDragPreview),
                new PropertyMetadata(32D, OnCaptionHeightPropertyChanged));

        private static void OnCaptionHeightPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var dockWindow = d as RSDragPreview;
            dockWindow.UpdateWindowChrome();
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.UpdateWindowChrome();
        }

        public void UpdateWindowChrome()
        {
            WindowChrome.SetWindowChrome(this, null);
            var chrome = new WindowChrome
            {
                CaptionHeight = this.CaptionHeight,
                UseAeroCaptionButtons = false,
            };
            WindowChrome.SetWindowChrome(this, chrome);
        }
    }
}
