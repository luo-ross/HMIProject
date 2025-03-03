using RS.Widgets.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace RS.Widgets.Interface
{
    public interface IMessageBox
    {
        public Button PART_BtnYes { get; set; }
        public Button PART_BtnOK { get; set; }
        public Button PART_BtnNo { get; set; }
        public Button PART_BtnCancel { get; set; }
        TaskCompletionSource<MessageBoxResult> MessageBoxResultTCS { get; set; }

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


        [Description("消息提示图标")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxImage MessageBoxImage
        {
            get => this is DependencyObject obj ? MessageBoxAttached.GetMessageBoxImage(obj) : default;
            set
            {
                if (this is DependencyObject obj)
                    MessageBoxAttached.SetMessageBoxImage(obj, value);
            }
        }




        [Description("消息框按钮")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxButton MessageBoxButton
        {
            get => this is DependencyObject obj ? MessageBoxAttached.GetMessageBoxButton(obj) : default;
            set
            {
                if (this is DependencyObject obj)
                    MessageBoxAttached.SetMessageBoxButton(obj, value);
            }
        }
       

        [Description("消息框内容")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        string MessageBoxText
        {
            get => this is DependencyObject obj ? MessageBoxAttached.GetMessageBoxText(obj) : default;
            set
            {
                if (this is DependencyObject obj)
                    MessageBoxAttached.SetMessageBoxText(obj, value);
            }
        }
      

        [Description("消息框标题")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        string Caption
        {
            get => this is DependencyObject obj ? MessageBoxAttached.GetCaption(obj) : default;
            set
            {
                if (this is DependencyObject obj)
                    MessageBoxAttached.SetCaption(obj, value);
            }
        }
      


        [Description("消息框默认返回消息")]
        [Category("消息框样式设置")]
        [Browsable(true)]

        MessageBoxResult DefaultResult
        {
            get => this is DependencyObject obj ? MessageBoxAttached.GetDefaultResult(obj) : default;
            set
            {
                if (this is DependencyObject obj)
                    MessageBoxAttached.SetDefaultResult(obj, value);
            }
        }
       

        [Description("指定消息框的特殊显示选项")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxOptions Options
        {
            get => this is DependencyObject obj ? MessageBoxAttached.GetOptions(obj) : default;
            set
            {
                if (this is DependencyObject obj)
                    MessageBoxAttached.SetOptions(obj, value);
            }
        }




        /// <summary>
        /// 消息框显示
        /// </summary>
        void MessageBoxDisplay(Window window);

        /// <summary>
        /// 消息框显示
        /// </summary>
        void MessageBoxClose();

        void SetMessageBoxResult(MessageBoxResult messageBoxResult)
        {
            this.MessageBoxResultTCS?.SetResult(messageBoxResult);
            this.MessageBoxClose();
        }
 

        Dispatcher Dispatcher { get;}
        async Task<MessageBoxResult> ShowMessageBox(Window window, string messageBoxText = null,
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

        async Task<MessageBoxResult> ShowAsync(string messageBoxText)
        {
            return await ShowMessageBox(null, messageBoxText: messageBoxText);
        }

        async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption)
        {
            return await ShowMessageBox(null,
                messageBoxText: messageBoxText,
                caption: caption);
        }

        async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button)
        {
            return await ShowMessageBox(null,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: MessageBoxImage.None,
                defaultResult: MessageBoxResult.None,
                options: MessageBoxOptions.None);
        }

        async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return await ShowMessageBox(null,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon);
        }

        async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await ShowMessageBox(null,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon,
                defaultResult: defaultResult);
        }

        async Task<MessageBoxResult> ShowAsync(string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return await ShowMessageBox(null,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon,
                defaultResult: defaultResult,
                options: options);
        }

        async Task<MessageBoxResult> ShowAsync(Window window,string messageBoxText)
        {
            return await ShowMessageBox(window,messageBoxText: messageBoxText);
        }

        async Task<MessageBoxResult> ShowAsync(Window window, string messageBoxText, string caption)
        {
            return await ShowMessageBox(window,
                messageBoxText: messageBoxText,
                caption: caption);
        }

        async Task<MessageBoxResult> ShowAsync(Window window, string messageBoxText, string caption, MessageBoxButton button)
        {
            return await ShowMessageBox(window,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: MessageBoxImage.None,
                defaultResult: MessageBoxResult.None,
                options: MessageBoxOptions.None);
        }
        async Task<MessageBoxResult> ShowAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon)
        {
            return await ShowMessageBox(window,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon);
        }

        async Task<MessageBoxResult> ShowAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult)
        {
            return await ShowMessageBox(window,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon,
                defaultResult: defaultResult);
        }

        async Task<MessageBoxResult> ShowAsync(Window window, string messageBoxText, string caption, MessageBoxButton button, MessageBoxImage icon, MessageBoxResult defaultResult, MessageBoxOptions options)
        {
            return await ShowMessageBox(window,
                messageBoxText: messageBoxText,
                caption: caption,
                button: button,
                icon: icon,
                defaultResult: defaultResult,
                options: options);
        }
    }
}
