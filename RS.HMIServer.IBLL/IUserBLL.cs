using RS.HMIServer.Models;
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
    public interface IUserBLL
    {
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetUsersAsync(string sessionId);

           }
}
