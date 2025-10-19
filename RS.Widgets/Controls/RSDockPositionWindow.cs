using CommunityToolkit.Mvvm.ComponentModel;
using RS.Commons.Helper;
using RS.Widgets.Models;
using RS.Win32API;
using RS.Win32API.Structs;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;

namespace RS.Widgets.Controls
{
    public enum DockPositionWindowType
    {
        Type1,
        Type2,
        Type3
    }

    public enum PositionEnum
    {
        UnKnown,
        WindowLeft,
        WindowTop,
        WindowRight,
        WindowBottom,
        TabLeft,
        TabTop,
        TabRight,
        TabBottom,
        TabCenter
    }


    public class BtnPositionInfo
    {
        public CheckBox BtnPosition { get; set; }
        public RECT ScreenRect { get; set; }
        public PositionEnum Position { get; set; }

        public override string ToString()
        {
            return $"{Position.ToString()} {ScreenRect.ToString()}";
        }
    }


    public class RSDockPositionWindow : Window
    {
        private CheckBox PART_BtnLeftPosition;
        private CheckBox PART_BtnTopPosition;
        private CheckBox PART_BtnRightPosition;
        private CheckBox PART_BtnBottomPosition;


        private CheckBox PART_BtnTabLeftPosition;
        private CheckBox PART_BtnTabTopPosition;
        private CheckBox PART_BtnTabRightPosition;
        private CheckBox PART_BtnTabBottomPosition;
        private CheckBox PART_BtnTabCenterPosition;

        private RSDragPreview RSDragPreview { get; set; }

        public HwndSource HwndSource { get; private set; }
        public List<BtnPositionInfo> BtnPositionInfoList { get; set; }


        //public Rect DockPanelRect { get; set; }
        //public RSDockGrid DockGrid { get; set; }
        //public FrameworkElement UserView { get; set; }

        /// <summary>
        /// 记录Panel在屏幕上的位置
        /// </summary>
        public Rect ScreenRect { get; private set; }

        public IntPtr Handle { get; private set; }

        static RSDockPositionWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDockPositionWindow), new FrameworkPropertyMetadata(typeof(RSDockPositionWindow)));
        }

        private RSDockPositionWindow(RSDragPreview dragPreview)
        {
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            this.ShowInTaskbar = false;
            this.Topmost = true;
            this.MouseMove += RSDockPositionWindow_MouseMove;
            this.Loaded += RSDockPositionWindow_Loaded;
            this.RSDragPreview = dragPreview;
        }

        public RSDockPositionWindow(RSDragPreview dragPreview, RSDockGrid dockGrid) : this(dragPreview)
        {
            this.RSDockGrid = dockGrid;
            this.SetWindowLocation(dockGrid.ScreenRect);
        }

        public RSDockPositionWindow(RSDragPreview dragPreview, RSDockPanel dockPanel) : this(dragPreview)
        {
            this.RSDockPanel = dockPanel;
            this.SetWindowLocation(dockPanel.ScreenRect);
        }

        private void SetWindowLocation(Rect screenRect)
        {
            double minWidth = 250;
            double minHeight = 250;
            switch (this.DockPositionWindowType)
            {
                case DockPositionWindowType.Type1:
                    minWidth = 100;
                    minWidth = 100;
                    break;
                case DockPositionWindowType.Type2:
                    minWidth = 250;
                    minWidth = 250;
                    break;
                case DockPositionWindowType.Type3:
                    minWidth = 150;
                    minWidth = 150;
                    break;
            }


            if (screenRect.Width < minWidth)
            {
                this.Width = minWidth;
                this.Left = screenRect.Left - (minWidth - screenRect.Width) / 2;
            }
            else
            {
                this.Width = screenRect.Width;
                this.Left = screenRect.Left;
            }

            if (screenRect.Height < minHeight)
            {
                this.Height = minHeight;
                this.Top = screenRect.Top - (minHeight - screenRect.Height) / 2;
            }
            else
            {
                this.Height = screenRect.Height;
                this.Top = screenRect.Top;
            }
        }



        private void RSDockPositionWindow_Loaded(object sender, RoutedEventArgs e)
        {
            this.BtnPositionInfoList = new List<BtnPositionInfo>();
            this.AddBtnPositionInfo(this.PART_BtnLeftPosition, PositionEnum.WindowLeft);
            this.AddBtnPositionInfo(this.PART_BtnTopPosition, PositionEnum.WindowTop);
            this.AddBtnPositionInfo(this.PART_BtnRightPosition, PositionEnum.WindowRight);
            this.AddBtnPositionInfo(this.PART_BtnBottomPosition, PositionEnum.WindowBottom);


            this.AddBtnPositionInfo(this.PART_BtnTabLeftPosition, PositionEnum.TabLeft);
            this.AddBtnPositionInfo(this.PART_BtnTabTopPosition, PositionEnum.TabTop);
            this.AddBtnPositionInfo(this.PART_BtnTabRightPosition, PositionEnum.TabRight);
            this.AddBtnPositionInfo(this.PART_BtnTabBottomPosition, PositionEnum.TabBottom);
            this.AddBtnPositionInfo(this.PART_BtnTabCenterPosition, PositionEnum.TabCenter);

            this.RefreshScreenRect();
        }


        public void RefreshScreenRect()
        {
            if (PresentationSource.FromVisual(this) == null)
            {
                return;
            }

            var screenPoint = this.PointToScreen(new Point(0, 0));

            this.ScreenRect = new Rect(screenPoint.X,
                                       screenPoint.Y,
                                       this.ActualWidth,
                                       this.ActualHeight);
        }


        private void AddBtnPositionInfo(CheckBox buttonInfo, PositionEnum position)
        {
            if (buttonInfo == null)
            {
                return;
            }
            var screenPoint = buttonInfo.PointToScreen(new Point(0, 0));
            var actualWidth = buttonInfo.ActualWidth;
            var actualHeight = buttonInfo.ActualHeight;
            var btnPositionInfo = new BtnPositionInfo()
            {
                BtnPosition = buttonInfo,
                Position = position,
                ScreenRect = RECT.FromXYWH((int)screenPoint.X, (int)screenPoint.Y, (int)actualWidth, (int)actualHeight),
            };
            this.BtnPositionInfoList.Add(btnPositionInfo);
        }

        private void RSDockPositionWindow_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
        {
            
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
            return IntPtr.Zero;
        }


        [Description("参考位置类型")]
        public DockPositionWindowType DockPositionWindowType
        {
            get { return (DockPositionWindowType)GetValue(DockPositionWindowTypeProperty); }
            set { SetValue(DockPositionWindowTypeProperty, value); }
        }



        public static readonly DependencyProperty DockPositionWindowTypeProperty =
            DependencyProperty.Register("DockPositionWindowType", typeof(DockPositionWindowType), typeof(RSDockPositionWindow), new PropertyMetadata(DockPositionWindowType.Type1));



        public BtnPositionInfo BtnPositionInfoSelect
        {
            get { return (BtnPositionInfo)GetValue(BtnPositionInfoSelectProperty); }
            set { SetValue(BtnPositionInfoSelectProperty, value); }
        }

        public static readonly DependencyProperty BtnPositionInfoSelectProperty =
            DependencyProperty.Register("BtnPositionInfoSelect", typeof(BtnPositionInfo), typeof(RSDockPositionWindow), new PropertyMetadata(null));

        public double PositionMaskWidth
        {
            get { return (double)GetValue(PositionMaskWidthProperty); }
            set { SetValue(PositionMaskWidthProperty, value); }
        }

        public static readonly DependencyProperty PositionMaskWidthProperty =
            DependencyProperty.Register("PositionMaskWidth", typeof(double), typeof(RSDockPositionWindow), new PropertyMetadata(0D));



        public double PositionMaskHeight
        {
            get { return (double)GetValue(PositionMaskHeightProperty); }
            set { SetValue(PositionMaskHeightProperty, value); }
        }

        public RSDockGrid RSDockGrid { get; private set; }
        public RSDockPanel RSDockPanel { get; private set; }

        public static readonly DependencyProperty PositionMaskHeightProperty =
            DependencyProperty.Register("PositionMaskHeight", typeof(double), typeof(RSDockPositionWindow), new PropertyMetadata(0D));



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnLeftPosition = this.GetTemplateChild(nameof(this.PART_BtnLeftPosition)) as CheckBox;
            this.PART_BtnTopPosition = this.GetTemplateChild(nameof(this.PART_BtnTopPosition)) as CheckBox;
            this.PART_BtnRightPosition = this.GetTemplateChild(nameof(this.PART_BtnRightPosition)) as CheckBox;
            this.PART_BtnBottomPosition = this.GetTemplateChild(nameof(this.PART_BtnBottomPosition)) as CheckBox;

            switch (this.DockPositionWindowType)
            {
                case DockPositionWindowType.Type1:
                    break;
                case DockPositionWindowType.Type2:
                    this.PART_BtnTabLeftPosition = this.GetTemplateChild(nameof(this.PART_BtnTabLeftPosition)) as CheckBox;
                    this.PART_BtnTabTopPosition = this.GetTemplateChild(nameof(this.PART_BtnTabTopPosition)) as CheckBox;
                    this.PART_BtnTabRightPosition = this.GetTemplateChild(nameof(this.PART_BtnTabRightPosition)) as CheckBox;
                    this.PART_BtnTabBottomPosition = this.GetTemplateChild(nameof(this.PART_BtnTabBottomPosition)) as CheckBox;
                    this.PART_BtnTabCenterPosition = this.GetTemplateChild(nameof(this.PART_BtnTabCenterPosition)) as CheckBox;
                    break;
                case DockPositionWindowType.Type3:
                    this.PART_BtnTabCenterPosition = this.GetTemplateChild(nameof(this.PART_BtnTabCenterPosition)) as CheckBox;
                    break;
            }
        }


        public BtnPositionInfo GetPanelPositionInfo(POINT screenPoint)
        {
            this.PositionMaskWidth = 0;
            this.PositionMaskHeight = 0;
            this.BtnPositionInfoSelect = GetBtnPositionInfoList(this.BtnPositionInfoList, screenPoint).FirstOrDefault(); ;

            if (this.BtnPositionInfoSelect != null)
            {
                //Rect screenRect;
                //if (this.RSDockPanel != null)
                //{
                //    screenRect =
                //}
                //var screenRect = this.RSDockPanel =
                //this.PositionMaskWidth = screenRect.Width;
                //this.PositionMaskHeight = screenRect.Height;
                //var currentWindowActualWidth = screenRect.Width;
                //var currentWindowActualHeight = screenRect.Height;
                //var currentWindowActualWidthHalf = currentWindowActualWidth / 2D - 5;
                //var currentWindowActualHeightHalf = currentWindowActualHeight / 2D - 5;

                //if (this.PositionMaskWidth > currentWindowActualWidthHalf)
                //{
                //    this.PositionMaskWidth = currentWindowActualWidthHalf;
                //}
                //if (this.PositionMaskHeight > currentWindowActualHeightHalf)
                //{
                //    this.PositionMaskHeight = currentWindowActualHeightHalf;
                //}
                var dockPosition = this.BtnPositionInfoSelect.Position;
                switch (dockPosition)
                {
                    case PositionEnum.UnKnown:
                        break;
                    case PositionEnum.WindowLeft:
                        Console.WriteLine($"WindowLeft");
                        //这里只计算宽度 高度自适应
                        break;
                    case PositionEnum.WindowTop:
                        Console.WriteLine($"WindowTop");
                        //这里只计算高度 宽度自适应
                        break;
                    case PositionEnum.WindowRight:
                        Console.WriteLine($"WindowRight");
                        //这里只计算宽度 高度自适应
                        break;
                    case PositionEnum.WindowBottom:
                        Console.WriteLine($"WindowBottom");
                        //这里只计算高度 宽度自适应
                        break;
                    case PositionEnum.TabLeft:
                        Console.WriteLine($"TabLeft");
                        break;
                    case PositionEnum.TabTop:
                        Console.WriteLine($"TabTop");
                        break;
                    case PositionEnum.TabRight:
                        Console.WriteLine($"TabRight");
                        break;
                    case PositionEnum.TabBottom:
                        Console.WriteLine($"TabBottom");
                        break;
                    case PositionEnum.TabCenter:
                        Console.WriteLine($"TabCenter");
                        break;
                }
            }


            return this.BtnPositionInfoSelect;
        }



        public void MouseMoveEventTrigger(BtnPositionInfo positionInfoSelect)
        {
            this.BtnPositionInfoSelect = positionInfoSelect;
            if (this.BtnPositionInfoSelect != null)
            {
                //Rect screenRect;
                //if (this.RSDockPanel != null)
                //{
                //    screenRect =
                //}
                //var screenRect = this.RSDockPanel =
                //this.PositionMaskWidth = screenRect.Width;
                //this.PositionMaskHeight = screenRect.Height;
                //var currentWindowActualWidth = screenRect.Width;
                //var currentWindowActualHeight = screenRect.Height;
                //var currentWindowActualWidthHalf = currentWindowActualWidth / 2D - 5;
                //var currentWindowActualHeightHalf = currentWindowActualHeight / 2D - 5;

                //if (this.PositionMaskWidth > currentWindowActualWidthHalf)
                //{
                //    this.PositionMaskWidth = currentWindowActualWidthHalf;
                //}
                //if (this.PositionMaskHeight > currentWindowActualHeightHalf)
                //{
                //    this.PositionMaskHeight = currentWindowActualHeightHalf;
                //}
                var dockPosition = this.BtnPositionInfoSelect.Position;
                switch (dockPosition)
                {
                    case PositionEnum.UnKnown:
                        break;
                    case PositionEnum.WindowLeft:
                        Console.WriteLine($"WindowLeft");
                        //这里只计算宽度 高度自适应
                        break;
                    case PositionEnum.WindowTop:
                        Console.WriteLine($"WindowTop");
                        //这里只计算高度 宽度自适应
                        break;
                    case PositionEnum.WindowRight:
                        Console.WriteLine($"WindowRight");
                        //这里只计算宽度 高度自适应
                        break;
                    case PositionEnum.WindowBottom:
                        Console.WriteLine($"WindowBottom");
                        //这里只计算高度 宽度自适应
                        break;
                    case PositionEnum.TabLeft:
                        Console.WriteLine($"TabLeft");
                        break;
                    case PositionEnum.TabTop:
                        Console.WriteLine($"TabTop");
                        break;
                    case PositionEnum.TabRight:
                        Console.WriteLine($"TabRight");
                        break;
                    case PositionEnum.TabBottom:
                        Console.WriteLine($"TabBottom");
                        break;
                    case PositionEnum.TabCenter:
                        Console.WriteLine($"TabCenter");
                        break;
                }
            }
        }

        public List<BtnPositionInfo> GetBtnPositionInfoList(List<BtnPositionInfo> btnPositionInfoList, POINT cursorPos)
        {
            return btnPositionInfoList.Where(t => t.ScreenRect.Left <= cursorPos.X
             && t.ScreenRect.Top <= cursorPos.Y
             && t.ScreenRect.Right >= cursorPos.X
             && t.ScreenRect.Bottom >= cursorPos.Y).ToList();
        }

    }
}
