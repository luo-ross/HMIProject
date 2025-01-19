using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace RS.Widgets.Controls
{
    public class RSUserControl : ContentControl
    {
        private RSLoading PART_Loading;
        public RSMessageBox MessageBox;
        static RSUserControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSUserControl), new FrameworkPropertyMetadata(typeof(RSUserControl)));
        }

        public async Task<OperateResult> InvokeLoadingActionAsync(Func<Task<OperateResult>> func, LoadingConfig loadingConfig = null)
        {
            return await this.PART_Loading.InvokeLoadingActionAsync(func, loadingConfig);
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


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_Loading = this.GetTemplateChild(nameof(this.PART_Loading)) as RSLoading ;
            this.MessageBox = this.GetTemplateChild(nameof(this.MessageBox)) as RSMessageBox;
        }


    }
}
