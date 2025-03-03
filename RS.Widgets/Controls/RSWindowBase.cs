using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;


namespace RS.Widgets.Controls
{
    public class RSWindowBase : Window
    {
        internal Border PART_Border;

        public RSWindowBase()
        {
            #region 这里样式必须在构造函数配置
            this.WindowStyle = WindowStyle.None;
            this.AllowsTransparency = true;
            #endregion
            this.SizeChanged += RSWindowBase_SizeChanged;
            this.StateChanged += RSWindow_StateChanged;
            this.Activated += RSWindowBase_Activated;
        }

        private void RSWindowBase_Activated(object? sender, EventArgs e)
        {
            this.UpdateBorderClipRect();
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
            Ross.SetWindowPos(new HWND(hWnd), HWND.Null, 0, 0, nWidth, nHeight, SET_WINDOW_POS_FLAGS.SWP_NOZORDER | SET_WINDOW_POS_FLAGS.SWP_NOACTIVATE);
        }


        private void RSWindowBase_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            this.UpdateBorderClipRect();
        }




        private void UpdateBorderClipRect()
        {
            if (this.PART_Border == null)
            {
                return;
            }
            var width = this.PART_Border.ActualWidth;
            var height = this.PART_Border.ActualHeight;
            this.BorderClipRect = this.GetBorderClipRect(this.BorderCornerRadius, width, height);
        }


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Border = this.GetTemplateChild(nameof(this.PART_Border)) as Border;
            this.UpdateBorderClipRect();
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


        [Description("宽边裁剪")]
        [Browsable(false)]
        public Geometry BorderClipRect
        {
            get { return (Geometry)GetValue(BorderClipRectProperty); }
            set { SetValue(BorderClipRectProperty, value); }
        }

        public static readonly DependencyProperty BorderClipRectProperty =
            DependencyProperty.Register("BorderClipRect", typeof(Geometry), typeof(RSWindowBase), new PropertyMetadata(null));

        /// <summary>
        /// 获取裁剪边框
        /// </summary>
        /// <param name="borderCornerRadius">边框圆角大小</param>
        /// <param name="width">高度</param>
        /// <param name="height">宽度</param>
        /// <returns></returns>
        public Geometry GetBorderClipRect(CornerRadius borderCornerRadius, double width, double height)
        {

            // 创建 PathGeometry
            PathGeometry pathGeometry = new PathGeometry();

            // 创建 PathFigure
            PathFigure pathFigure = new PathFigure
            {
                StartPoint = new Point(borderCornerRadius.TopLeft, 0) // 起点
            };

            // 添加顶部边缘
            pathFigure.Segments.Add(new LineSegment(new Point(width - borderCornerRadius.TopRight, 0), true));

            // 添加右上角圆角（半径 20）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(width, borderCornerRadius.TopRight), // 终点
                new Size(borderCornerRadius.TopRight, borderCornerRadius.TopRight),   // 半径
                0,                  // 旋转角度
                false,              // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));

            // 添加右侧边缘
            pathFigure.Segments.Add(new LineSegment(new Point(width, height - borderCornerRadius.BottomRight), true));

            // 添加右下角圆角（半径 40）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(width - borderCornerRadius.BottomRight, height), // 终点
                new Size(borderCornerRadius.BottomRight, borderCornerRadius.BottomRight),    // 半径
                0,                   // 旋转角度
                false,               // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));

            // 添加底部边缘
            pathFigure.Segments.Add(new LineSegment(new Point(borderCornerRadius.BottomLeft, height), true));

            // 添加左下角圆角（半径 30）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(0, height - borderCornerRadius.BottomLeft), // 终点
                new Size(borderCornerRadius.BottomLeft, borderCornerRadius.BottomLeft),   // 半径
                0,                  // 旋转角度
                false,              // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));

            // 添加左侧边缘
            pathFigure.Segments.Add(new LineSegment(new Point(0, borderCornerRadius.TopLeft), true));

            // 添加左上角圆角（半径 10）
            pathFigure.Segments.Add(new ArcSegment(
                new Point(borderCornerRadius.TopLeft, 0), // 终点
                new Size(borderCornerRadius.TopLeft, borderCornerRadius.TopLeft), // 半径
                0,                // 旋转角度
                false,            // 是否大弧
                SweepDirection.Clockwise, // 方向
                true));
            // 将 PathFigure 添加到 PathGeometry
            pathGeometry.Figures.Add(pathFigure);

            return pathGeometry;
        }
    }
}
