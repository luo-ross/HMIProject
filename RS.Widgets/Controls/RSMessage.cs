using RS.Widgets.Interface;
using ScottPlot;
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
    public class RSMessage : ContentControl, IMessage
    {
        public Button PART_BtnYes { get; set; }
        public Button PART_BtnOK { get; set; }
        public Button PART_BtnNo { get; set; }
        public Button PART_BtnCancel { get; set; }
        public TaskCompletionSource<MessageBoxResult> MessageBoxResultTCS { get; set; }

        static RSMessage()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(RSMessage), new FrameworkPropertyMetadata(typeof(RSMessage)));
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
            DependencyProperty.Register("MessageBoxImage", typeof(MessageBoxImage), typeof(RSMessage), new PropertyMetadata(default));



        [Description("消息框按钮")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxButton MessageBoxButton
        {
            get { return (MessageBoxButton)GetValue(MessageBoxButtonProperty); }
            set { SetValue(MessageBoxButtonProperty, value); }
        }

        public static readonly DependencyProperty MessageBoxButtonProperty =
            DependencyProperty.Register("MessageBoxButton", typeof(MessageBoxButton), typeof(RSMessage), new PropertyMetadata(default));



        [Description("消息框内容")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public string MessageBoxText
        {
            get { return (string)GetValue(MessageBoxTextProperty); }
            set { SetValue(MessageBoxTextProperty, value); }
        }

        public static readonly DependencyProperty MessageBoxTextProperty =
            DependencyProperty.Register("MessageBoxText", typeof(string), typeof(RSMessage), new PropertyMetadata(default));



        [Description("消息框标题")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public string Caption
        {
            get { return (string)GetValue(CaptionProperty); }
            set { SetValue(CaptionProperty, value); }
        }

        public static readonly DependencyProperty CaptionProperty =
            DependencyProperty.Register("Caption", typeof(string), typeof(RSMessage), new PropertyMetadata(default));



        [Description("消息框默认返回消息")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxResult DefaultResult
        {
            get { return (MessageBoxResult)GetValue(DefaultResultProperty); }
            set { SetValue(DefaultResultProperty, value); }
        }

        public static readonly DependencyProperty DefaultResultProperty =
            DependencyProperty.Register("DefaultResult", typeof(MessageBoxResult), typeof(RSMessage), new PropertyMetadata(default));


        [Description("指定消息框的特殊显示选项")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        public MessageBoxOptions Options
        {
            get { return (MessageBoxOptions)GetValue(OptionsProperty); }
            set { SetValue(OptionsProperty, value); }
        }

        public static readonly DependencyProperty OptionsProperty =
            DependencyProperty.Register("Options", typeof(MessageBoxOptions), typeof(RSMessage), new PropertyMetadata(default));



        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            this.PART_BtnYes = this.GetTemplateChild(nameof(this.PART_BtnYes)) as Button;
            this.PART_BtnOK = this.GetTemplateChild(nameof(this.PART_BtnOK)) as Button;
            this.PART_BtnNo = this.GetTemplateChild(nameof(this.PART_BtnNo)) as Button;
            this.PART_BtnCancel = this.GetTemplateChild(nameof(this.PART_BtnCancel)) as Button;
            ((IMessage)this).HandleBtnClickEvent();
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

        public void SetMessageBoxResult(MessageBoxResult messageBoxResult)
        {
            this.MessageBoxResultTCS?.SetResult(messageBoxResult);
            this.MessageBoxClose();
        }


        /// <summary>
        /// 处理按钮点击事件
        /// </summary>
        public void HandleBtnClickEvent()
        {
            if (this.PART_BtnYes != null)
            {
                this.PART_BtnYes.Click += (sender, e) =>
                {
                    this.SetMessageBoxResult(MessageBoxResult.Yes);
                };
            }

            if (this.PART_BtnOK != null)
            {
                this.PART_BtnOK.Click += (sender, e) =>
                {
                    this.SetMessageBoxResult(MessageBoxResult.OK);
                };
            }
            if (this.PART_BtnNo != null)
            {
                this.PART_BtnNo.Click += (sender, e) =>
                {
                    this.SetMessageBoxResult(MessageBoxResult.No);
                };
            }
            if (this.PART_BtnCancel != null)
            {
                this.PART_BtnCancel.Click += (sender, e) =>
                {
                    this.SetMessageBoxResult(MessageBoxResult.Cancel);
                };
            }
        }


        public async Task<MessageBoxResult> ShowMessageAsync(Window window,
          string messageBoxText = null,
          string caption = null,
          MessageBoxButton button = MessageBoxButton.OK,
          MessageBoxImage icon = MessageBoxImage.None,
          MessageBoxResult defaultResult = MessageBoxResult.None,
          MessageBoxOptions options = MessageBoxOptions.None
          )
        {
            this.MessageBoxResultTCS = new TaskCompletionSource<MessageBoxResult>();
            if (string.IsNullOrWhiteSpace(caption))
            {
                caption = "消息提示";
            }
            Dispatcher.Invoke(() =>
            {
                this.MessageBoxText = messageBoxText;
                this.Caption = caption;
                this.MessageBoxButton = button;
                this.MessageBoxImage = icon;
                this.DefaultResult = defaultResult;
                this.Options = options;
                this.MessageBoxDisplay(window);
            });
            return await this.MessageBoxResultTCS.Task;
        }



        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText)
        {
            return await ShowMessageAsync(null,
                            messageBoxText: messageBoxText,
                            caption : null,
                            button: MessageBoxButton.OK,
                            icon: MessageBoxImage.None,
                            defaultResult: MessageBoxResult.None,
                            options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, 
            string caption)
        {
            return await ShowMessageAsync(null,
                           messageBoxText: messageBoxText,
                           caption: caption,
                           button: MessageBoxButton.OK,
                           icon: MessageBoxImage.None,
                           defaultResult: MessageBoxResult.None,
                           options: MessageBoxOptions.None);

        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button)
        {
            return await ShowMessageAsync(null,
                          messageBoxText: messageBoxText,
                          caption: caption,
                          button: button,
                          icon: MessageBoxImage.None,
                          defaultResult: MessageBoxResult.None,
                          options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon)
        {
            return await ShowMessageAsync(null,
                         messageBoxText: messageBoxText,
                         caption: caption,
                         button: button,
                         icon: icon,
                         defaultResult: MessageBoxResult.None,
                         options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon, 
            MessageBoxResult defaultResult)
        {
            return await ShowMessageAsync(null,
                          messageBoxText: messageBoxText,
                          caption: caption,
                          button: button,
                          icon: icon,
                          defaultResult: defaultResult,
                          options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return await ShowMessageAsync(null,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon,
                defaultResult: defaultResult,
                options: options);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, 
            string messageBoxText)
        {
            return await ShowMessageAsync(window,
                           messageBoxText: messageBoxText,
                           caption: null,
                           button: MessageBoxButton.OK,
                           icon: MessageBoxImage.None,
                           defaultResult: MessageBoxResult.None,
                           options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, 
            string messageBoxText, 
            string caption)
        {
            return await ShowMessageAsync(window,
                            messageBoxText: messageBoxText,
                            caption: caption,
                            button: MessageBoxButton.OK,
                            icon: MessageBoxImage.None,
                            defaultResult: MessageBoxResult.None,
                            options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window,
            string messageBoxText,
            string caption,
            MessageBoxButton button)
        {
            return await ShowMessageAsync(window,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: MessageBoxImage.None,
                defaultResult: MessageBoxResult.None,
                options: MessageBoxOptions.None);
        }
        public async Task<MessageBoxResult> ShowMessageAsync(Window window,
            string messageBoxText, 
            string caption, 
            MessageBoxButton button, 
            MessageBoxImage icon)
        {
            return await ShowMessageAsync(window,
                             messageBoxText: messageBoxText,
                             caption: caption,
                             button: button,
                             icon: icon,
                             defaultResult: MessageBoxResult.None,
                             options: MessageBoxOptions.None);
        }

        public async Task<MessageBoxResult> ShowMessageAsync(Window window, 
            string messageBoxText,
            string caption,
            MessageBoxButton button,
            MessageBoxImage icon,
            MessageBoxResult defaultResult)
        {
            return await ShowMessageAsync(window,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon,
                defaultResult: defaultResult);
        }
    }
}
