﻿using RS.HMIServer.Models;
using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IBLL
{
    public interface ISMSBLL
    {
        /// <summary>
        /// 发送注册短信验证码
        /// </summary>
        /// <param name="countryCode">国家代码</param>
        /// <param name="phone">电话号码</param>
        /// <param name="verify">验证码</param>
        /// <returns></returns>
        Task<OperateResult> SendRegisterVerifyAsync(string countryCode, string phone, int verify);
    }
}
