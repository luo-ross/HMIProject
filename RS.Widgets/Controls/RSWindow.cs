using RS.HMI.Models.Widgets;
using RS.Win32API;
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
using System.Windows.Interop;
using System.Windows.Media;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.WindowsAndMessaging;


namespace RS.Widgets.Controls
{
    public class RSWindow : Window
    {
        private RSBorder PART_Caption;
        private Button PART_Minimize;
        private Button PART_BtnMaxRestore;
        private Button PART_BtnClose;
        private Border PART_Border;
        private RSUserControl PART_WinContentHost;
        public RSMessageBox MessageBox
        {
            get
            {
                return this.PART_WinContentHost.MessageBox;
            }
        }
        static RSWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSWindow), new FrameworkPropertyMetadata(typeof(RSWindow)));
        }
        public RSWindow()
        {
            this.CommandBindings.Add(new CommandBinding(SystemCommands.CloseWindowCommand, CloseWindow, CanCloseWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MinimizeWindowCommand, MinimizeWindow, CanMinimizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.MaximizeWindowCommand, MaximizeRestoreWindow, CanMaximizeWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.RestoreWindowCommand, MaximizeRestoreWindow, CanRestoreWindow));
            this.CommandBindings.Add(new CommandBinding(SystemCommands.ShowSystemMenuCommand, ShowSystemMenu, CanShowSystemMenu));
            // 添加命令绑定
            this.CommandBindings.Add(new CommandBinding(RSCommands.CleanTextCommand, CleanTextText));

            this.SizeChanged += RSWindow_SizeChanged;
            this.StateChanged += RSWindow_StateChanged;
        }


        public async Task<OperateResult> InvokeLoadingActionAsync(Func<Task<OperateResult>> func, LoadingConfig loadingConfig = null)
        {
            return await this.PART_WinContentHost.InvokeLoadingActionAsync(func, loadingConfig);
        }

        private void CleanTextText(object sender, ExecutedRoutedEventArgs e)
        {
            RSCommands.CleanText(e.Parameter);
        }

        private void CanWindowMove(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }


        private void WindowMaxRestore(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.PART_BtnMaxRestore.Command != null && this.PART_BtnMaxRestore.Command.CanExecute(null))
            {
                if (!(this.ResizeMode == ResizeMode.NoResize))
                {
                    this.PART_BtnMaxRestore.Command.Execute(null);
                }
            }
        }

        private void CanShowSystemMenu(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ShowSystemMenu(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.ShowSystemMenu(this, this.PointToScreen(Mouse.GetPosition(this)));
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

        private void RSWindow_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            var width = this.PART_Border.ActualWidth;
            var height = this.PART_Border.ActualHeight;
            this.BorderClipRect = this.GetBorderClipRect(this.BorderCornerRadius, width, height);
        }

        private void CanRestoreWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WindowState == WindowState.Maximized;
        }

        private void CanMaximizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = this.WindowState == WindowState.Normal;
        }

        private void MaximizeRestoreWindow(object sender, ExecutedRoutedEventArgs e)
        {
            if (this.WindowState == WindowState.Maximized)
            {
                SystemCommands.RestoreWindow(this);
            }
            else
            {
                SystemCommands.MaximizeWindow(this);
            }
        }

        private void CanMinimizeWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void MinimizeWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        private void CanCloseWindow(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CloseWindow(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Border = this.GetTemplateChild(nameof(this.PART_Border)) as Border;
            this.PART_Caption = this.GetTemplateChild(nameof(this.PART_Caption)) as RSBorder;
            this.PART_Minimize = this.GetTemplateChild(nameof(this.PART_Minimize)) as Button;
            this.PART_BtnMaxRestore = this.GetTemplateChild(nameof(this.PART_BtnMaxRestore)) as Button;
            this.PART_BtnClose = this.GetTemplateChild(nameof(this.PART_BtnClose)) as Button;
            this.PART_WinContentHost = this.GetTemplateChild(nameof(this.PART_WinContentHost)) as RSUserControl;
        }



        [Description("圆角边框大小")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public CornerRadius BorderCornerRadius
        {
            get { return (CornerRadius)GetValue(BorderCornerRadiusProperty); }
            set { SetValue(BorderCornerRadiusProperty, value); }
        }

        public static readonly DependencyProperty BorderCornerRadiusProperty =
            DependencyProperty.Register("BorderCornerRadius", typeof(CornerRadius), typeof(RSWindow), new PropertyMetadata(new CornerRadius(0)));


        [Description("宽边裁剪")]
        [Browsable(false)]
        public Geometry BorderClipRect
        {
            get { return (Geometry)GetValue(BorderClipRectProperty); }
            set { SetValue(BorderClipRectProperty, value); }
        }

        public static readonly DependencyProperty BorderClipRectProperty =
            DependencyProperty.Register("BorderClipRect", typeof(Geometry), typeof(RSWindow), new PropertyMetadata(null));




        [Description("窗口最大化时是否全屏")]
        [Browsable(false)]
        public bool IsMaxsizedFullScreen
        {
            get { return (bool)GetValue(IsMaxsizedFullScreenProperty); }
            set { SetValue(IsMaxsizedFullScreenProperty, value); }
        }

        public static readonly DependencyProperty IsMaxsizedFullScreenProperty =
            DependencyProperty.Register("IsMaxsizedFullScreen", typeof(bool), typeof(RSWindow), new PropertyMetadata(false));


        [Description("自定义标题栏内容")]
        [Browsable(false)]
        public object CaptionContent
        {
            get { return (object)GetValue(CaptionContentProperty); }
            set { SetValue(CaptionContentProperty, value); }
        }

        public static readonly DependencyProperty CaptionContentProperty =
            DependencyProperty.Register("CaptionContent", typeof(object), typeof(RSWindow), new PropertyMetadata(null));



        [Description("标题栏高度设置")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }

        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(RSWindow), new PropertyMetadata(30D));



        [Description("是否沉浸式")]
        [Browsable(true)]
        [Category("自定义窗口样式")]
        public bool IsFitSystem
        {
            get { return (bool)GetValue(IsFitSystemProperty); }
            set { SetValue(IsFitSystemProperty, value); }
        }
        public static readonly DependencyProperty IsFitSystemProperty =
            DependencyProperty.Register("IsFitSystem", typeof(bool), typeof(RSWindow), new PropertyMetadata(false));

        protected override void OnPropertyChanged(DependencyPropertyChangedEventArgs e)
        {
            base.OnPropertyChanged(e);
            if (e.Property == IsMaxsizedFullScreenProperty)
            {
                if (this.IsMaxsizedFullScreen)
                {
                    SystemCommands.MaximizeWindow(this);
                }
                else
                {
                    SystemCommands.RestoreWindow(this);
                }

            }

        }

        /// <summary>
        /// 获取裁剪边框
        /// </summary>
        /// <param name="borderCornerRadius">边框圆角大小</param>
        /// <param name="width">高度</param>
        /// <param name="height">宽度</param>
        /// <returns></returns>
        public Geometry GetBorderClipRect(CornerRadius borderCornerRadius,double width,double height)
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
