using CommunityToolkit.Mvvm.ComponentModel;
using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public partial class ApplicationViewModel : NotifyBase
    {
        /// <summary>
        /// 服务是否连接成功
        /// </summary>
        [ObservableProperty]
        private bool? isServerConnectSuccess;


        /// <summary>
        /// 网络是否可用
        /// </summary>
        [ObservableProperty]
        private bool? isNetworkAvailable;


        /// <summary>
        /// 是否成功获取会话
        /// </summary>
        [ObservableProperty]
        private bool isGetSessionModelSuccess;
       
    
    }
}
