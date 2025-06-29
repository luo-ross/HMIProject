using RS.Widgets.Enums;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Threading;

namespace RS.Widgets.Interfaces
{
    public interface IInfoBar
    {
        /// <summary>
        /// 添加消息
        /// </summary>
        /// <param name="message"></param>
        /// <param name="infoType"></param>
        void ShowInfoAsync(string message, InfoType infoType = InfoType.None);

        /// <summary>
        /// 添加警告消息
        /// </summary>
        /// <param name="message"></param>
        public void ShowWarningInfoAsync(string message)
        {
            this.ShowInfoAsync(message, InfoType.Warning);
        }

        /// <summary>
        /// 添加通知消息
        /// </summary>
        /// <param name="message"></param>
        void ShowInformationAsync(string message)
        {
            this.ShowInfoAsync(message, InfoType.Information);
        }

        /// <summary>
        /// 添加错误消息
        /// </summary>
        /// <param name="message"></param>
        void ShowErrorInfoAsync(string message)
        {
            this.ShowInfoAsync(message, InfoType.Error);
        }
    }
}
