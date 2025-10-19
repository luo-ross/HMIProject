using CommunityToolkit.Mvvm.ComponentModel;
using RS.Commons.Helper;
using RS.Widgets.Models;
using RS.Win32API.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class ApplicationViewModel : NotifyBase
    {
        public ApplicationViewModel()
        {
           
        }

        private bool? isServerConnectSuccess;
        /// <summary>
        /// 服务是否连接成功
        /// </summary>
        public bool? IsServerConnectSuccess
        {
            get { return isServerConnectSuccess; }
            set
            {
                this.SetProperty(ref isServerConnectSuccess, value);
            }
        }


        private bool? isNetworkAvailable;
        /// <summary>
        /// 网络是否可用
        /// </summary>
        public bool? IsNetworkAvailable
        {
            get { return isNetworkAvailable; }
            set
            {
                this.SetProperty(ref isNetworkAvailable, value);
            }
        }

        private bool isGetSessionModelSuccess;
        /// <summary>
        /// 是否成功获取会话
        /// </summary>
        public bool IsGetSessionModelSuccess
        {
            get { return isGetSessionModelSuccess; }
            set
            {
                this.SetProperty(ref isGetSessionModelSuccess, value);
            }
        }


    





    }
}
