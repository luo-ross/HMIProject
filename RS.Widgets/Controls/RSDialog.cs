using CommunityToolkit.Mvvm.Messaging;
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
    public class RSDialog : ContentControl, IDialog
    {
        private RSLoading PART_Loading;
        private RSMessage PART_Message;
        private RSModal PART_Modal;
        private IWindow ParentWin;

        //public IMessage MessageBox
        //{
        //    get
        //    {
        //        return this.PART_Message;
        //    }
        //}

        //public IMessage WinMessageBox
        //{
        //    get
        //    {
        //        return new RSWinMessage()
        //        {
        //            Owner = (RSWindow)this.ParentWin,
        //            Width = 350,
        //            Height = 250
        //        };
        //    }
        //}

        static RSDialog()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSDialog), new FrameworkPropertyMetadata(typeof(RSDialog)));
        }
        public RSDialog()
        {
            this.Loaded += RSDialog_Loaded;
            this.Unloaded += RSDialog_Unloaded;
        }

        private void RSDialog_Unloaded(object sender, RoutedEventArgs e)
        {
            //注销Dialog
            DialogManager.UnregisterDialog(DialogKey);
        }

        private void RSDialog_Loaded(object sender, RoutedEventArgs e)
        {
            this.ParentWin = this.TryFindParent<RSWindow>();
        }

        //public async Task<OperateResult> InvokeLoadingActionAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        //{
        //    return await this.PART_Loading.InvokeLoadingActionAsync(func, loadingConfig, cancellationToken);
        //}

        //public async Task<OperateResult<T>> InvokeLoadingActionAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        //{
        //    return await this.PART_Loading.InvokeLoadingActionAsync<T>(func, loadingConfig, cancellationToken);
        //}


        [Description("对话框编码")]
        public string DialogKey
        {
            get { return (string)GetValue(DialogKeyProperty); }
            set { SetValue(DialogKeyProperty, value); }
        }

        public static readonly DependencyProperty DialogKeyProperty =
            DependencyProperty.Register("DialogKey", typeof(string), typeof(RSDialog), new PropertyMetadata(null, OnDialogKeyPropertyChanged));

        private static void OnDialogKeyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is RSDialog rsDialog)
            {
                //注册Dialog
                DialogManager.RegisterDialog(rsDialog.DialogKey, rsDialog);
            }
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Loading = this.GetTemplateChild(nameof(this.PART_Loading)) as RSLoading;
            this.PART_Message = this.GetTemplateChild(nameof(this.PART_Message)) as RSMessage;
            this.PART_Modal = GetTemplateChild(nameof(PART_Modal)) as RSModal;
        }

        public IWindow GetParentWin()
        {
            return this.ParentWin;
        }

        public ILoading GetLoading()
        {
            return this.PART_Loading;
        }

        public IModal GetModal()
        {
           return this.PART_Modal;
        }
    
        public void ShowModal(object modalContent)
        {
            this.PART_Modal.ShowModal(modalContent);
        }

        public void HideModal()
        {
            this.PART_Modal.HideModal();
        }

        public IMessage GetMessageBox()
        {
            return this.PART_Message;
        }

        public IMessage GetWinMessageBox()
        {
            return new RSWinMessage()
            {
                Owner = (RSWindow)this.ParentWin,
                Width = 350,
                Height = 250
            };
        }

        public IModal GetWinModal()
        {
            return new RSWinModal()
            {
                Owner = (RSWindow)this.ParentWin,
            };
        }
    
    }
}
