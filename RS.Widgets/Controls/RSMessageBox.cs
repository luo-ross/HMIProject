using RS.Widgets.Interface;
using ScottPlot;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSMessageBox : ContentControl, IMessageBox
    {
        public Button PART_BtnYes { get; set; }
        public Button PART_BtnOK { get; set; }
        public Button PART_BtnNo { get; set; }
        public Button PART_BtnCancel { get; set; }
        public TaskCompletionSource<MessageBoxResult> MessageBoxResultTCS { get; set; }

        static RSMessageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSMessageBox), new FrameworkPropertyMetadata(typeof(RSMessageBox)));
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnYes = this.GetTemplateChild(nameof(this.PART_BtnYes)) as Button;
            this.PART_BtnOK = this.GetTemplateChild(nameof(this.PART_BtnOK)) as Button;
            this.PART_BtnNo = this.GetTemplateChild(nameof(this.PART_BtnNo)) as Button;
            this.PART_BtnCancel = this.GetTemplateChild(nameof(this.PART_BtnCancel)) as Button;
            ((IMessageBox)this).HandleBtnClickEvent();
        }


        public void MessageBoxDisplay(Window window)
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Visibility = Visibility.Visible;
            });
        }

        public void MessageBoxClose()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Visibility = Visibility.Collapsed;
            });
        }
    }
}
