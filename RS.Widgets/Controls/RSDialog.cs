using CommunityToolkit.HighPerformance.Helpers;
using CommunityToolkit.Mvvm.Messaging;
using NPOI.SS.Formula.Functions;
using RS.Commons;
using RS.Widgets.Controls;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
using RS.Win32API;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace RS.Widgets.Controls
{
    public class RSDialog : ContentControl, IDialog, IMessage, ILoading, IModal, IWinModal, IDialogBase
    {
        private RSLoading PART_Loading;
        private RSMessage PART_Message;
        private RSModal PART_Modal;
        public RSNavigate Navigate { get; private set; }
        public RSWindow ParentWin { get; private set; }

        static RSDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDialog), new FrameworkPropertyMetadata(typeof(RSDialog)));
        }

        public RSDialog()
        {
            this.Unloaded += RSDialog_Unloaded;
            this.DataContextChanged += RSDialog_DataContextChanged;
        }

        private void RSDialog_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            DialogHelper.UnregisterDialog(e.OldValue);
            DialogHelper.RegisterDialog(e.NewValue, this);
        }

        private void RSDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            DialogHelper.UnregisterDialog(this.DataContext);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.ParentWin = this.TryFindParent<RSWindow>();
            this.Navigate = this.TryFindParent<RSNavigate>();
            this.PART_Loading = this.GetTemplateChild(nameof(this.PART_Loading)) as RSLoading;
            this.PART_Message = this.GetTemplateChild(nameof(this.PART_Message)) as RSMessage;
            this.PART_Modal = GetTemplateChild(nameof(PART_Modal)) as RSModal;
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

        IWindow IDialog.ParentWin
        {
            get
            {
                return this.ParentWin;
            }
        }

        INavigate IDialog.Navigate
        {
            get
            {
                return this.Navigate;
            }
        }


        public ILoading Loading
        {
            get
            {
                return this.PART_Loading;
            }
        }

        public ILoading RootLoading
        {
            get
            {
                return this.ParentWin?.Loading;
            }
        }


        public IModal Modal
        {
            get
            {
                return this.PART_Modal;
            }
        }

        public IModal RootModal
        {
            get
            {
                return this.ParentWin?.Modal;
            }
        }

        public IMessage MessageBox
        {
            get
            {
                return this.PART_Message;
            }
        }

        public IMessage RootMessageBox
        {
            get
            {
                return this.ParentWin.MessageBox;
            }
        }


        public IWinModal WinModal
        {
            get
            {
                return new RSWinModal(this.ParentWin as Window);
            }
        }

        public IWinMessage WinMessageBox
        {
            get
            {
                return new RSWinMessage(this.ParentWin as Window);
            }
        }



        #endregion

    }
}
