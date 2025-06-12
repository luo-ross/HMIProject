using RS.Widgets.Enums;
using RS.Win32API;
using System.ComponentModel;
using System.Reflection.Metadata;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Shapes;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;


namespace RS.Widgets.Controls
{
    public class RSWindowBase : Window
    {
        internal Border PART_Border;
        public HwndSource HwndSource;

        public RSWindowBase()
        {
            #region 这里样式必须在构造函数配置
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            #endregion
            this.SizeChanged += RSWindowBase_SizeChanged;
            this.StateChanged += RSWindow_StateChanged;
            this.Activated += RSWindowBase_Activated;
            this.Loaded += RSWindowBase_Loaded;
            this.Closing += RSWindow_Closing;
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);
            this.HwndSource = (HwndSource)PresentationSource.FromVisual(this);
        }


        [Description("窗口最大化时是否全屏")]
        [Browsable(false)]
        public bool IsMaxsizedFullScreen
        {
            get { return (bool)GetValue(IsMaxsizedFullScreenProperty); }
            set { SetValue(IsMaxsizedFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsMaxsizedFullScreenProperty =
            DependencyProperty.Register("IsMaxsizedFullScreen", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(false));



        [Description("圆角边框大小")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty =
            DependencyProperty.Register("BorderCornerRadius", typeof(CornerRadius), typeof(RSWindowBase), new PropertyMetadata(new CornerRadius(0)));


        //[Description("宽边裁剪")]
        //[Browsable(false)]
        //public Geometry BorderClipRect
        //{
        //    get { return (Geometry)GetValue(BorderClipRectProperty); }
        //    set { SetValue(BorderClipRectProperty, value); }
        //}

        //public static readonly DependencyProperty BorderClipRectProperty =
        //    DependencyProperty.Register("BorderClipRect", typeof(Geometry), typeof(RSWindowBase), new PropertyMetadata(null));


        [Description("是否显示窗体关闭放大缩小按钮")]
        [Browsable(true)]
        public bool IsHidenWinCommandBtn
        {
            get { return (bool)GetValue(IsHidenWinCommandBtnProperty); }
            set { SetValue(IsHidenWinCommandBtnProperty, value); }
        }

        public static readonly DependencyProperty IsHidenWinCommandBtnProperty =
            DependencyProperty.Register("IsHidenWinCommandBtn", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(false));

        #region Icon参数设置

    

        public CornerRadius IconCornerRadius
        {
            get { return (CornerRadius)GetValue(IconCornerRadiusProperty); }
            set { SetValue(IconCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty IconCornerRadiusProperty =
            DependencyProperty.Register("IconCornerRadius", typeof(CornerRadius), typeof(RSWindowBase), new PropertyMetadata(default));



        public double IconWidth
        {
            get { return (double)GetValue(IconWidthProperty); }
            set { SetValue(IconWidthProperty, value); }
        }

        public static readonly DependencyProperty IconWidthProperty =
            DependencyProperty.Register("IconWidth", typeof(double), typeof(RSWindowBase), new PropertyMetadata(20D));



        public double IconHeight
        {
            get { return (double)GetValue(IconHeightProperty); }
            set { SetValue(IconHeightProperty, value); }
        }

        public static readonly DependencyProperty IconHeightProperty =
            DependencyProperty.Register("IconHeight", typeof(double), typeof(RSWindowBase), new PropertyMetadata(20D));



        public Thickness IconMargin
        {
            get { return (Thickness)GetValue(IconMarginProperty); }
            set { SetValue(IconMarginProperty, value); }
        }

        public static readonly DependencyProperty IconMarginProperty =
            DependencyProperty.Register("IconMargin", typeof(Thickness), typeof(RSWindowBase), new PropertyMetadata(new Thickness(5,0,0,0)));

        #endregion


        private void RSWindow_Closing(object? sender, CancelEventArgs e)
        {
            ApplicationBase.RSWinInfoBar?.Close();
        }

        private void RSWindowBase_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.RefreshWindowSizeAndLocation();
            }
        }

        private void RSWindowBase_Activated(object? sender, EventArgs e)
        {
            //this.UpdateBorderClipRect();
        }

        private void RSWindow_StateChanged(object? sender, EventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                this.RefreshWindowSizeAndLocation();
            }
        }

        private void RefreshWindowSizeAndLocation()
        {
            // 使用 WindowInteropHelper 获取窗口句柄
            var hWnd = new WindowInteropHelper(this).Handle;
            int nWidth = IsMaxsizedFullScreen ? (int)SystemParameters.PrimaryScreenWidth : (int)SystemParameters.WorkArea.Width;  // 新的宽度
            int nHeight = IsMaxsizedFullScreen ? (int)SystemParameters.PrimaryScreenHeight : (int)SystemParameters.WorkArea.Height; // 新的高度
            NativeMethods.SetWindowPos(new HandleRef(null, hWnd), new HandleRef(null,IntPtr.Zero), 0, 0, nWidth, nHeight, NativeMethods.SWP_NOZORDER | NativeMethods.SWP_NOACTIVATE);
        }


        private void RSWindowBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //this.UpdateBorderClipRect();
        }




        //private void UpdateBorderClipRect()
        //{
        //    if (this.PART_Border == null)
        //    {
        //        return;
        //    }
        //    var width = this.PART_Border.ActualWidth;
        //    var height = this.PART_Border.ActualHeight;
        //    this.BorderClipRect = this.GetBorderClipRect(this.BorderCornerRadius, width, height);
        //}


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Border = this.GetTemplateChild(nameof(this.PART_Border)) as Border;
            var resizeHost = this.GetTemplateChild("PART_ResizeHost") as Grid;
            if (resizeHost != null)
            {
                foreach (Rectangle resizeRectangle in resizeHost.Children)
                {
                    resizeRectangle.PreviewMouseDown += ResizeRectangle_PreviewMouseDown;
                    resizeRectangle.MouseMove += ResizeRectangle_MouseMove;
                }
            }

            //this.UpdateBorderClipRect();
        }

        private void ResizeRectangle_MouseMove(object sender, MouseEventArgs e)
        {

        }
        private void ResizeWindow(ResizeDirection direction)
        {
            NativeMethods.SendMessage(this.HwndSource.Handle, NativeMethods.WM_SYSCOMMAND, NativeMethods.SC_SIZE + (int)direction, IntPtr.Zero);
        }



        ///// <summary>
        ///// 获取裁剪边框
        ///// </summary>
        ///// <param name="borderCornerRadius">边框圆角大小</param>
        ///// <param name="width">高度</param>
        ///// <param name="height">宽度</param>
        ///// <returns></returns>
        //public Geometry GetBorderClipRect(CornerRadius borderCornerRadius, double width, double height)
        //{

        //    // 创建 PathGeometry
        //    PathGeometry pathGeometry = new PathGeometry();

        //    // 创建 PathFigure
        //    PathFigure pathFigure = new PathFigure
        //    {
        //        StartPoint = new Point(borderCornerRadius.TopLeft, 0) // 起点
        //    };

        //    // 添加顶部边缘
        //    pathFigure.Segments.Add(new LineSegment(new Point(width - borderCornerRadius.TopRight, 0), true));

        //    // 添加右上角圆角（半径 20）
        //    pathFigure.Segments.Add(new ArcSegment(
        //        new Point(width, borderCornerRadius.TopRight), // 终点
        //        new Size(borderCornerRadius.TopRight, borderCornerRadius.TopRight),   // 半径
        //        0,                  // 旋转角度
        //        false,              // 是否大弧
        //        SweepDirection.Clockwise, // 方向
        //        true));

        //    // 添加右侧边缘
        //    pathFigure.Segments.Add(new LineSegment(new Point(width, height - borderCornerRadius.BottomRight), true));

        //    // 添加右下角圆角（半径 40）
        //    pathFigure.Segments.Add(new ArcSegment(
        //        new Point(width - borderCornerRadius.BottomRight, height), // 终点
        //        new Size(borderCornerRadius.BottomRight, borderCornerRadius.BottomRight),    // 半径
        //        0,                   // 旋转角度
        //        false,               // 是否大弧
        //        SweepDirection.Clockwise, // 方向
        //        true));

        //    // 添加底部边缘
        //    pathFigure.Segments.Add(new LineSegment(new Point(borderCornerRadius.BottomLeft, height), true));

        //    // 添加左下角圆角（半径 30）
        //    pathFigure.Segments.Add(new ArcSegment(
        //        new Point(0, height - borderCornerRadius.BottomLeft), // 终点
        //        new Size(borderCornerRadius.BottomLeft, borderCornerRadius.BottomLeft),   // 半径
        //        0,                  // 旋转角度
        //        false,              // 是否大弧
        //        SweepDirection.Clockwise, // 方向
        //        true));

        //    // 添加左侧边缘
        //    pathFigure.Segments.Add(new LineSegment(new Point(0, borderCornerRadius.TopLeft), true));

        //    // 添加左上角圆角（半径 10）
        //    pathFigure.Segments.Add(new ArcSegment(
        //        new Point(borderCornerRadius.TopLeft, 0), // 终点
        //        new Size(borderCornerRadius.TopLeft, borderCornerRadius.TopLeft), // 半径
        //        0,                // 旋转角度
        //        false,            // 是否大弧
        //        SweepDirection.Clockwise, // 方向
        //        true));
        //    // 将 PathFigure 添加到 PathGeometry
        //    pathGeometry.Figures.Add(pathFigure);

        //    return pathGeometry;
        //}

        private void ResizeRectangle_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            Rectangle rectangle = sender as Rectangle;
            if (rectangle != null)
            {
                switch (rectangle.Name)
                {
                    case nameof(ResizeDirection.PART_Top):
                        ResizeWindow(ResizeDirection.PART_Top);
                        break;
                    case nameof(ResizeDirection.PART_Bottom):
                        ResizeWindow(ResizeDirection.PART_Bottom);
                        break;
                    case nameof(ResizeDirection.PART_Left):
                        ResizeWindow(ResizeDirection.PART_Left);
                        break;
                    case nameof(ResizeDirection.PART_Right):
                        ResizeWindow(ResizeDirection.PART_Right);
                        break;
                    case nameof(ResizeDirection.PART_LeftTop):
                        ResizeWindow(ResizeDirection.PART_LeftTop);
                        break;
                    case nameof(ResizeDirection.PART_RightTop):
                        ResizeWindow(ResizeDirection.PART_RightTop);
                        break;
                    case nameof(ResizeDirection.PART_LeftBottom):
                        ResizeWindow(ResizeDirection.PART_LeftBottom);
                        break;
                    case nameof(ResizeDirection.PART_RightBottom):
                        ResizeWindow(ResizeDirection.PART_RightBottom);
                        break;
                    default:
                        break;
                }
            }
        }




    }
}
