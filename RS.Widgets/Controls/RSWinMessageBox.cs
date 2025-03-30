using RS.Widgets.Interface;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace RS.Widgets.Controls
{
    public class RSWinMessageBox : RSWindowBase, IMessageBox
    {
        public Button PART_BtnYes { get; set; }
        public Button PART_BtnOK { get; set; }
        public Button PART_BtnNo { get; set; }
        public Button PART_BtnCancel { get; set; }
        public TaskCompletionSource<MessageBoxResult> MessageBoxResultTCS { get; set; }
        static RSWinMessageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSWinMessageBox), new FrameworkPropertyMetadata(typeof(RSWinMessageBox)));
        }

        public RSWinMessageBox(Window owner = null)
        {
            this.Width = 600;
            this.Height = 350;
            this.BorderCornerRadius = new CornerRadius(5);
            this.Owner = owner;
            if (this.Owner == null)
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterScreen;
            }
            else
            {
                this.WindowStartupLocation = WindowStartupLocation.CenterOwner;
            }
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
                this.Owner = window;
                this.ShowDialog();
            });
        }

        public void MessageBoxClose()
        {
            this.Dispatcher.Invoke(() =>
            {
                this.Close();
            });
        }
    }
}
