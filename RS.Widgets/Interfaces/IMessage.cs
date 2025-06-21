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

namespace RS.Widgets.Interfaces
{
    public interface IMessage
    {

        /// <summary>
        /// 处理按钮点击事件
        /// </summary>
        void HandleBtnClickEvent();


        /// <summary>
        /// 消息框显示
        /// </summary>
        void MessageBoxDisplay(Window window);

        /// <summary>
        /// 消息框显示
        /// </summary>
        void MessageBoxClose();


        Task<MessageBoxResult> ShowMessageAsync(Window window, string messageBoxText = null,
          string caption = null,
          MessageBoxButton button = MessageBoxButton.OK,
          MessageBoxImage icon = MessageBoxImage.None,
          MessageBoxResult defaultResult = MessageBoxResult.None,
          MessageBoxOptions options = MessageBoxOptions.None
          );

         Task<MessageBoxResult> ShowMessageAsync(string messageBoxText);

         Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, 
             string caption);

         Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
             string caption, 
             MessageBoxButton button);

         Task<MessageBoxResult> ShowMessageAsync(string messageBoxText, 
             string caption, 
             MessageBoxButton button, 
             MessageBoxImage icon);

         Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
             string caption, 
             MessageBoxButton button,
             MessageBoxImage icon,
             MessageBoxResult defaultResult);

         Task<MessageBoxResult> ShowMessageAsync(string messageBoxText,
             string caption,
             MessageBoxButton button, 
             MessageBoxImage icon, 
             MessageBoxResult defaultResult,
             MessageBoxOptions options);

         Task<MessageBoxResult> ShowMessageAsync(Window window,
             string messageBoxText);

         Task<MessageBoxResult> ShowMessageAsync(Window window,
             string messageBoxText, 
             string caption);

         Task<MessageBoxResult> ShowMessageAsync(Window window,
             string messageBoxText, 
             string caption,
             MessageBoxButton button);
         Task<MessageBoxResult> ShowMessageAsync(Window window, 
             string messageBoxText,
             string caption, 
             MessageBoxButton button,
             MessageBoxImage icon);

         Task<MessageBoxResult> ShowMessageAsync(Window window, 
             string messageBoxText,
             string caption,
             MessageBoxButton button,
             MessageBoxImage icon, 
             MessageBoxResult defaultResult);
       
    }
}
