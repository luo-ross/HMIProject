using RS.Commons;
using RS.Widgets.Interface;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public class RSUserControl : ContentControl, IRSLoading
    {
        public RSLoading PART_Loading { get; set; }
        private RSMessageBox PART_MessageBox;
        private RSWindow ParentWin;
        public RSModal PART_Modal;

        public IMessageBox MessageBox
        {
            get
            {
                return this.PART_MessageBox;
            }
        }

        public IMessageBox WinMessageBox
        {
            get
            {
                return new RSWinMessageBox()
                {
                    Owner = this.ParentWin,
                    Width = 350,
                    Height = 250
                };
            }
        }

        static RSUserControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSUserControl), new FrameworkPropertyMetadata(typeof(RSUserControl)));
        }
        public RSUserControl()
        {
            this.ParentWin = this.TryFindParent<RSWindow>();
        }

        public async Task<OperateResult> InvokeLoadingActionAsync(Func<CancellationToken, Task<OperateResult>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return await this.PART_Loading.InvokeLoadingActionAsync(func, loadingConfig, cancellationToken);
        }

        public async Task<OperateResult<T>> InvokeLoadingActionAsync<T>(Func<CancellationToken, Task<OperateResult<T>>> func, LoadingConfig loadingConfig = null, CancellationToken cancellationToken = default)
        {
            return await this.PART_Loading.InvokeLoadingActionAsync<T>(func, loadingConfig, cancellationToken);
        }

        public double CaptionHeight
        {
            get { return (double)GetValue(CaptionHeightProperty); }
            set { SetValue(CaptionHeightProperty, value); }
        }
        public static readonly DependencyProperty CaptionHeightProperty =
            DependencyProperty.Register("CaptionHeight", typeof(double), typeof(RSUserControl), new PropertyMetadata(30D));



        [Description("是否允许窗体拖拽")]
        public bool IsAllowWindowDragCommand
        {
            get { return (bool)GetValue(IsAllowWindowDragCommandProperty); }
            set { SetValue(IsAllowWindowDragCommandProperty, value); }
        }

        public static readonly DependencyProperty IsAllowWindowDragCommandProperty =
            DependencyProperty.Register("IsAllowWindowDragCommand", typeof(bool), typeof(RSUserControl), new PropertyMetadata(false));


        [Description("是否双击进行窗体最大化和最小化")]
        public bool IsAllowWindowMaxRestoreCommand
        {
            get { return (bool)GetValue(IsAllowWindowMaxRestoreCommandProperty); }
            set { SetValue(IsAllowWindowMaxRestoreCommandProperty, value); }
        }

        public static readonly DependencyProperty IsAllowWindowMaxRestoreCommandProperty =
            DependencyProperty.Register("IsAllowWindowMaxRestoreCommand", typeof(bool), typeof(RSUserControl), new PropertyMetadata(false));


        #region 模态窗口控制
        public void ShowModal(object modalContent)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.PART_Modal.Content = modalContent;
                this.PART_Modal.Visibility = Visibility.Visible;
            });
        }

        public void HideModal()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.PART_Modal.Content = null;
                this.PART_Modal.Visibility = Visibility.Collapsed;
            });
        }
        #endregion



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Loading = this.GetTemplateChild(nameof(this.PART_Loading)) as RSLoading;
            this.PART_MessageBox = this.GetTemplateChild(nameof(this.PART_MessageBox)) as RSMessageBox;
            this.PART_Modal = GetTemplateChild(nameof(PART_Modal)) as RSModal;
        }


    }
}
