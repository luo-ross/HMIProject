using RS.Commons;
using RS.Widgets.Enums;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace RS.Widgets.Controls
{
    public class RSWindow : RSWindowBase, IInfoBar, IWindow
    {
        private Button PART_Minimize;
        private Button PART_BtnMaxRestore;
        private Button PART_BtnClose;
        private RSDialog PART_WinContentHost;


        private DispatcherTimer InfoBarTimer;

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
         
        }

      


        [Description("消息")]
        public InfoBarModel InfoBarModel
        {
            get { return (InfoBarModel)GetValue(InfoBarModelProperty); }
            set { SetValue(InfoBarModelProperty, value); }
        }
        public static readonly DependencyProperty InfoBarModelProperty =
            DependencyProperty.Register("InfoBarModel", typeof(InfoBarModel), typeof(RSWindow), new PropertyMetadata(null, OnInfoBarModelPropertyChanged));

        private static void OnInfoBarModelPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var rsWindow = d as RSWindow;
            if (rsWindow.InfoBarModel == null)
            {
                return;
            }
            rsWindow.InfoBarTimer?.Stop();
            // 初始化定时器
            rsWindow.InfoBarTimer = new DispatcherTimer();
            // 设置定时器的间隔为 3 秒
            rsWindow.InfoBarTimer.Interval = TimeSpan.FromMilliseconds(rsWindow.InfoBarCloseDelay);
            // 为定时器的 Tick 事件添加处理程序
            rsWindow.InfoBarTimer.Tick += rsWindow.InfoBarTimer_Tick;
            // 启动定时器
            rsWindow.InfoBarTimer.Start();
        }


        [Description("消息提示自动关闭时间 单位毫秒")]
        public int InfoBarCloseDelay
        {
            get { return (int)GetValue(InfoBarCloseDelayProperty); }
            set { SetValue(InfoBarCloseDelayProperty, value); }
        }

        public static readonly DependencyProperty InfoBarCloseDelayProperty =
            DependencyProperty.Register("InfoBarCloseDelay", typeof(int), typeof(RSWindow), new PropertyMetadata(3000));



        private void InfoBarTimer_Tick(object? sender, EventArgs e)
        {
            this.InfoBarTimer.Stop();
            this.InfoBarTimer = null;
            this.Dispatcher.Invoke(() =>
            {
                this.InfoBarModel = null;
            });
        }


        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="infoType"></param>
        public void ShowInfoAsync(string message, InfoType infoType = InfoType.None)
        {
            var infoBarModel = new InfoBarModel()
            {
                CreateTime = DateTime.Now,
                Message = message,
                InfoType = infoType
            };
            this.Dispatcher.InvokeAsync(() =>
            {
                this.InfoBarModel = infoBarModel;
            });
        }

        //public async Task<OperateResult> InvokeLoadingActionAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        //{
        //    return await this.PART_WinContentHost.InvokeLoadingActionAsync(func, loadingConfig, cancellationToken);
        //}

        //public async Task<OperateResult<T>> InvokeLoadingActionAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        //{
        //    return await this.PART_WinContentHost.InvokeLoadingActionAsync<T>(func, loadingConfig, cancellationToken);
        //}

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
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(RSWindow), new PropertyMetadata(32D));



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


        [Description("是否显示标题")]
        public bool IsShowTitle
        {
            get { return (bool)GetValue(IsShowTitleProperty); }
            set { SetValue(IsShowTitleProperty, value); }
        }

        public static readonly DependencyProperty IsShowTitleProperty =
            DependencyProperty.Register("IsShowTitle", typeof(bool), typeof(RSWindow), new PropertyMetadata(true));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Minimize = this.GetTemplateChild(nameof(this.PART_Minimize)) as Button;
            this.PART_BtnMaxRestore = this.GetTemplateChild(nameof(this.PART_BtnMaxRestore)) as Button;
            this.PART_BtnClose = this.GetTemplateChild(nameof(this.PART_BtnClose)) as Button;
            this.PART_WinContentHost = this.GetTemplateChild(nameof(this.PART_WinContentHost)) as RSDialog;
        }


        /// <summary>
        /// 显示模态
        /// </summary>
        public void ShowModal(object modalContent)
        {
            this.PART_WinContentHost.ShowModal(modalContent);
            //this.Content = modalContent;
            //this.Show();
        }

        /// <summary>
        /// 关闭模态
        /// </summary>
        public void HideModal()
        {
            this.PART_WinContentHost.HideModal();
        }

        public IDialog GetDialog()
        {
            return this.PART_WinContentHost;
        }

        public IMessage GetMessageBox()
        {
            return this.PART_WinContentHost.GetMessageBox();
        }


        public IMessage GetWinMessageBox()
        {
            return new RSWinMessage()
            {
                Owner = this,
                Width = 350,
                Height = 250
            };
        }

        public IModal GetModal()
        {
           return this.PART_WinContentHost.GetModal();
        }

        public ILoading GetLoading()
        {
            return this.PART_WinContentHost.GetLoading();
        }

        
    }
}
