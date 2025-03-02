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
        private Button PART_BtnYes;
        private Button PART_BtnOK;
        private Button PART_BtnNo;
        private Button PART_BtnCancel;

        static RSWinMessageBox()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSWinMessageBox), new FrameworkPropertyMetadata(typeof(RSWinMessageBox)));
        }

       

        public RSWinMessageBox(Window owner = null)
        {
            this.Width = 500;
            this.Height = 350;
            this.BorderCornerRadius = new CornerRadius(10);
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




        [Description("消息提示图标")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxImage MessageBoxImage
        {
            get { return (MessageBoxImage)GetValue(MessageBoxImageProperty); }
            set { SetValue(MessageBoxImageProperty, value); }
        }

        public static readonly DependencyProperty MessageBoxImageProperty =
            DependencyProperty.Register("MessageBoxImage", typeof(MessageBoxImage), typeof(RSWinMessageBox), new PropertyMetadata(MessageBoxImage.None));


        [Description("消息框按钮")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxButton MessageBoxButton
        {
            get { return (MessageBoxButton)GetValue(MessageBoxButtonProperty); }
            set { SetValue(MessageBoxButtonProperty, value); }
        }

        public static readonly DependencyProperty MessageBoxButtonProperty =
            DependencyProperty.Register("MessageBoxButton", typeof(MessageBoxButton), typeof(RSWinMessageBox), new PropertyMetadata(MessageBoxButton.OK));


        [Description("消息框内容")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public string MessageBoxText
        {
            get { return (string)GetValue(MessageBoxTextProperty); }
            set { SetValue(MessageBoxTextProperty, value); }
        }

        public static readonly DependencyProperty MessageBoxTextProperty =
            DependencyProperty.Register("MessageBoxText", typeof(string), typeof(RSWinMessageBox), new PropertyMetadata(null));


        [Description("消息框标题")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(RSWinMessageBox), new PropertyMetadata("消息提示"));



        [Description("消息框默认返回消息")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxResult DefaultResult
        {
            get { return (MessageBoxResult)GetValue(DefaultResultProperty); }
            set { SetValue(DefaultResultProperty, value); }
        }

        public static readonly DependencyProperty DefaultResultProperty =
            DependencyProperty.Register("DefaultResult", typeof(MessageBoxResult), typeof(RSWinMessageBox), new PropertyMetadata(MessageBoxResult.None));


        [Description("指定消息框的特殊显示选项")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxOptions Options
        {
            get { return (MessageBoxOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register("Options", typeof(MessageBoxOptions), typeof(RSWinMessageBox), new PropertyMetadata(MessageBoxOptions.None));


        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnYes = this.GetTemplateChild(nameof(this.PART_BtnYes)) as Button;
            this.PART_BtnOK = this.GetTemplateChild(nameof(this.PART_BtnOK)) as Button;
            this.PART_BtnNo = this.GetTemplateChild(nameof(this.PART_BtnNo)) as Button;
            this.PART_BtnCancel = this.GetTemplateChild(nameof(this.PART_BtnCancel)) as Button;

            if (this.PART_BtnYes != null)
            {
                this.PART_BtnYes.Click -= PART_BtnYes_Click;
                this.PART_BtnYes.Click += PART_BtnYes_Click;
            }
            if (this.PART_BtnOK != null)
            {
                this.PART_BtnOK.Click -= PART_BtnOK_Click;
                this.PART_BtnOK.Click += PART_BtnOK_Click;
            }
            if (this.PART_BtnNo != null)
            {
                this.PART_BtnNo.Click -= PART_BtnNo_Click;
                this.PART_BtnNo.Click += PART_BtnNo_Click;
            }
            if (this.PART_BtnCancel != null)
            {
                this.PART_BtnCancel.Click -= PART_BtnCancel_Click;
                this.PART_BtnCancel.Click += PART_BtnCancel_Click;
            }
        }

        public TaskCompletionSource<MessageBoxResult> MessageBoxResultTCS { get; set; }
        private void PART_BtnCancel_Click(object sender, RoutedEventArgs e)
        {
            this.SetMessageBoxResult(MessageBoxResult.Cancel);
        }

        private void PART_BtnNo_Click(object sender, RoutedEventArgs e)
        {
            this.SetMessageBoxResult(MessageBoxResult.No);
        }

        private void PART_BtnOK_Click(object sender, RoutedEventArgs e)
        {
            this.SetMessageBoxResult(MessageBoxResult.OK);
        }

        private void PART_BtnYes_Click(object sender, RoutedEventArgs e)
        {
            this.SetMessageBoxResult(MessageBoxResult.Yes);
        }


        public void SetMessageBoxResult(MessageBoxResult messageBoxResult)
        {
            this.MessageBoxResultTCS?.SetResult(messageBoxResult);
            this.MessageBoxClose();
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
