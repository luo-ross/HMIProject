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
        private RSDialog PART_Dialog;
        private DispatcherTimer InfoBarTimer;
        static RSWindow()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSWindow), new FrameworkPropertyMetadata(typeof(RSWindow)));
        }


        public bool IsShowInfo
        {
            get { return (bool)GetValue(IsShowInfoProperty); }
            set { SetValue(IsShowInfoProperty, value); }
        }

        public static readonly DependencyProperty IsShowInfoProperty =
            DependencyProperty.Register("IsShowInfo", typeof(bool), typeof(RSWindow), new PropertyMetadata(true));



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

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Dialog = this.GetTemplateChild(nameof(this.PART_Dialog)) as RSDialog;
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
                return this.PART_Dialog?.Loading;
            }
        }


        public IModal Modal
        {
            get
            {
                return this.PART_Dialog?.Modal;
            }
        }


        public IMessage MessageBox
        {
            get
            {
                return this.PART_Dialog?.MessageBox;
            }
        }




        public IDialog Dialog
        {
            get
            {
                return this.PART_Dialog;
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
