using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.Logging;
using RS.Models;
using RS.Widgets.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RS.Widgets.Models
{
    public partial class InfoBarModel:NotifyBase
    {
        /// <summary>
        /// 消息
        /// </summary>
        [ObservableProperty]
        private string message;
   

        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }

        /// <summary>
        /// 消息类型
        /// </summary>
        [ObservableProperty]

        private InfoType infoType;

    }
}
