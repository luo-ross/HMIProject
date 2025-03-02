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
        TaskCompletionSource<MessageBoxResult> MessageBoxResultTCS { get; set; }

        [Description("消息提示图标")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxImage MessageBoxImage { get; set; }


        [Description("消息框按钮")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxButton MessageBoxButton { get; set; }


        [Description("消息框内容")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        string MessageBoxText { get; set; }

        [Description("消息框标题")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        string Caption { get; set; }


        [Description("消息框默认返回消息")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxResult DefaultResult { get; set; }

        [Description("指定消息框的特殊显示选项")]
        [Category("消息框样式设置")]
        [Browsable(true)]
        MessageBoxOptions Options { get; set; }


        /// <summary>
        /// 消息框显示
        /// </summary>
        void MessageBoxDisplay(Window window);

        /// <summary>
        /// 消息框显示
        /// </summary>
        void MessageBoxClose();

        void SetMessageBoxResult(MessageBoxResult messageBoxResult);

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
