using RS.Widgets.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class ApplicationViewModel : NotifyBase
    {

        private bool? isServerConnectSuccess;
        /// <summary>
        /// 服务是否连接成功
        /// </summary>
        public bool? IsServerConnectSuccess
        {
            get
            {
                return isServerConnectSuccess;
            }
            set
            {
                OnPropertyChanged(ref isServerConnectSuccess, value);
            }
        }
    }
}
