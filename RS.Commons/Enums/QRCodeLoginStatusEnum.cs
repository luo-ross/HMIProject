using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Enums
{
    /// <summary>
    /// 二维码登录状态枚举
    /// </summary>
    public enum QRCodeLoginStatusEnum
    {
        BeginGetQRCode=0,
        WaitScanQRCode=1,
        ScanQRCodeSuccess = 2,
        QRCodeAuthLogin = 3,
        CancelQRCodeLogin = 4,
        QRCodeLoginTimeOut = 5,
    }
}
