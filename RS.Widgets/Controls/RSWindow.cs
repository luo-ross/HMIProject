using RS.Commons;
using RS.Widgets.Enums;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Threading;


namespace RS.Widgets.Controls
{
    public class RSWindow : RSWindowBase, IInfoBar, IWindow, IDialogBase, ILoading, IMessage, IModal, IWinModal
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


        [Description("自定义中部标题栏内容")]
        [Browsable(false)]
        public object MidCaptionContent
        {
            get { return (object)GetValue(MidCaptionContentProperty); }
            set { SetValue(MidCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty MidCaptionContentProperty =
            DependencyProperty.Register("MidCaptionContent", typeof(object), typeof(RSWindow), new PropertyMetadata(null));


        [Description("自定义左侧标题栏内容")]
        [Browsable(false)]
        public object LeftCaptionContent
        {
            get { return (object)GetValue(LeftCaptionContentProperty); }
            set { SetValue(LeftCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty LeftCaptionContentProperty =
            DependencyProperty.Register("LeftCaptionContent", typeof(object), typeof(RSWindow), new PropertyMetadata(null));



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


        public bool IsShowIcon
        {
            get { return (bool)GetValue(IsShowIconProperty); }
            set { SetValue(IsShowIconProperty, value); }
        }

        public static readonly DependencyProperty IsShowIconProperty =
            DependencyProperty.Register("IsShowIcon", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));




        public bool IsShowMidCaptionContent
        {
            get { return (bool)GetValue(IsShowMidCaptionContentProperty); }
            set { SetValue(IsShowMidCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty IsShowMidCaptionContentProperty =
            DependencyProperty.Register("IsShowMidCaptionContent", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));


        public bool IsShowLeftCaptionContent
        {
            get { return (bool)GetValue(IsShowLeftCaptionContentProperty); }
            set { SetValue(IsShowLeftCaptionContentProperty, value); }
        }

        public static readonly DependencyProperty IsShowLeftCaptionContentProperty =
            DependencyProperty.Register("IsShowLeftCaptionContent", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));




        public bool IsShowInfo
        {
            get { return (bool)GetValue(IsShowInfoProperty); }
            set { SetValue(IsShowInfoProperty, value); }
        }

        public static readonly DependencyProperty IsShowInfoProperty =
            DependencyProperty.Register("IsShowInfo", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));


        [Description("是否显示窗体关闭放大缩小按钮")]
        [Browsable(true)]
        public bool IsShowWinBtnCommands
        {
            get { return (bool)GetValue(IsShowWinBtnCommandsProperty); }
            set { SetValue(IsShowWinBtnCommandsProperty, value); }
        }

        public static readonly DependencyProperty IsShowWinBtnCommandsProperty =
            DependencyProperty.Register("IsShowWinBtnCommands", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));




        public bool IsShowCaption
        {
            get { return (bool)GetValue(IsShowCaptionProperty); }
            set { SetValue(IsShowCaptionProperty, value); }
        }

        public static readonly DependencyProperty IsShowCaptionProperty =
            DependencyProperty.Register("IsShowCaption", typeof(bool), typeof(RSWindowBase), new PropertyMetadata(true));




        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Minimize = this.GetTemplateChild(nameof(this.PART_Minimize)) as Button;
            this.PART_BtnMaxRestore = this.GetTemplateChild(nameof(this.PART_BtnMaxRestore)) as Button;
            this.PART_BtnClose = this.GetTemplateChild(nameof(this.PART_BtnClose)) as Button;
            this.PART_WinContentHost = this.GetTemplateChild(nameof(this.PART_WinContentHost)) as RSDialog;
        }


        #region Interface implementation

        public Task<OperateResult> InvokeAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return this.Loading.InvokeAsync(func, loadingConfig, cancellationToken);
        }

        public Task<OperateResult<T>> InvokeAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return this.Loading.InvokeAsync(func, loadingConfig, cancellationToken);
        }

        void IModal.ShowModal(object content)
        {
            this.Modal.ShowModal(content);
        }

        void IModal.CloseModal()
        {
            this.Modal.CloseModal();
        }

        void IWinModal.ShowModal(object content)
        {
            this.WinModal.ShowModal(content);
        }

        void IWinModal.CloseModal()
        {
            this.WinModal.CloseModal();
        }

        public void ShowDialog(object content)
        {
            this.WinModal.ShowDialog(content);
        }


        public void HandleBtnClickEvent()
        {
            this.MessageBox.HandleBtnClickEvent();
        }

        public void MessageBoxDisplay(Window window)
        {
            this.MessageBox.MessageBoxDisplay(window);
        }

        public void MessageBoxClose()
        {
            this.MessageBox.MessageBoxClose();
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText = null, string caption = null, MessageBoxButton button = MessageBoxButton.OK, MessageBoxImage icon = MessageBoxImage.None, MessageBoxResult defaultResult = MessageBoxResult.None, MessageBoxOptions options = MessageBoxOptions.None)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button, icon, defaultResult, options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon, defaultResult);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return await this.MessageBox.ShowMessageAsync(messageBoxText, caption, button, icon, defaultResult, options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption, MessageBoxButton button)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button, icon);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await this.MessageBox.ShowMessageAsync(window, messageBoxText, caption, button, icon, defaultResult);
        }

        public ILoading Loading
        {
            get
            {
                return this.PART_WinContentHost?.Loading;
            }
        }


        public IModal Modal
        {
            get
            {
                return this.PART_WinContentHost?.Modal;
            }
        }


        public IMessage MessageBox
        {
            get
            {
                return this.PART_WinContentHost?.MessageBox;
            }
        }




        public IDialog Dialog
        {
            get
            {
                return this.PART_WinContentHost;
            }
        }

        public IWinModal WinModal
        {
            get
            {
                return new RSWinModal(this);
            }
        }

        public IWinMessage WinMessageBox
        {
            get
            {
                return new RSWinMessage(this);
            }
        }

        #endregion








    }
}
