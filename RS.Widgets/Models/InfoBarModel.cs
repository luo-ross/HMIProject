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
    public  class InfoBarModel:NotifyBase
    {


        private string message;
        /// <summary>
        /// 消息
        /// </summary>
        public string Message
        {
            get { return message; }
            set
            {
                this.SetProperty(ref message, value);

            }
        }


        /// <summary>
        /// 创建时间
        /// </summary>
        public DateTime CreateTime { get; set; }


        private InfoType infoType;
        /// <summary>
        /// 消息类型
        /// </summary>
        public InfoType InfoType
        {
            get { return infoType; }
            set
            {
                this.SetProperty(ref infoType, value);

            }
        }

    }
}
