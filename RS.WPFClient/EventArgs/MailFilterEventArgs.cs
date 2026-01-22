using RS.Widgets.Models;
using RS.WPFClient.Models;
using System;
using System.Windows;

namespace RS.Widgets
{
    /// <summary>
    /// 邮件选中事件参数
    /// </summary>
    public class MailFilterEventArgs : RoutedEventArgs
    {
      
        public MailFilterModel MailFilterModel { get; }

      
        /// <summary>
        /// 构造函数
        /// </summary>
        public MailFilterEventArgs(RoutedEvent routedEvent,  MailFilterModel mailFilterModel)
            : base(routedEvent)
        {
            MailFilterModel = mailFilterModel;
        }
    }
}

