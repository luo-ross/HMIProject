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


        private const string WINDOWPLACEMENTConfigKey = "C47C8080-072C-42E1-AB5C-5239C5C16808";

        private WINDOWPLACEMENT _WINDOWPLACEMENT;
        /// <summary>
        /// 获取窗体位置
        /// </summary>
        public WINDOWPLACEMENT WINDOWPLACEMENT
        {
            get
            {
                if (_WINDOWPLACEMENT.Equals(default(WINDOWPLACEMENT)))
                {
                    this.WINDOWPLACEMENT = ConfigHelpler.GetDefaultConfig(WINDOWPLACEMENTConfigKey, new WINDOWPLACEMENT());
                }
                return _WINDOWPLACEMENT;
            }
            set
            {
                this.SetProperty(ref _WINDOWPLACEMENT, value);
                ConfigHelpler.SaveAppConfigAsync(WINDOWPLACEMENTConfigKey, WINDOWPLACEMENT);
            }
        }






    }
}
