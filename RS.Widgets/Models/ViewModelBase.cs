using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using RS.Commons;
using RS.Widgets.Controls;
using RS.Widgets.Interfaces;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Models
{
    public class ViewModelBase : NotifyBase, IViewModeBase, ILoading, IMessage, IModal, IWinModal
    {
    
        public ViewModelBase()
        {

        }
    

        /// <summary>
        /// 对话层
        /// </summary>
        public IDialog Dialog
        {
            get
            {
                return DialogHelper.GetDialog(this);
            }
        }

        /// <summary>
        /// 导航
        /// </summary>
        public INavigate Navigate
        {
            get
            {
                return this.Dialog.Navigate;
            }
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
            this.Dialog.Modal.ShowModal(content);
        }

        void IWinModal.CloseModal()
        {
            this.Dialog.Modal.CloseModal();
        }


        public void ShowDialog(object content)
        {
            this.Dialog.WinModal.ShowDialog(content);
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

        public IWindow ParentWin
        {
            get
            {
                return this.Dialog?.ParentWin;
            }
        }

        public ILoading Loading
        {
            get
            {
                return this.Dialog?.Loading;
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
                return this.Dialog?.Modal;
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
                return this.Dialog?.MessageBox;
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
                return this.Dialog.WinModal;
            }
        }

        public IInfoBar InfoBar
        {
            get
            {
                return this.ParentWin;
            }
        }

        #endregion
    }

}