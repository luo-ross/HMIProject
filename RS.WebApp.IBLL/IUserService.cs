using RS.WebApp.Models;
using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RS.WebApp.IBLL
{
    public interface IUserService
    {
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetUsersAsync(string sessionId);

        /// <summary>
        /// 验证用户登录
        /// </summary>
        /// <param name="aesEncryptModel">AES对称加密</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult> ValidLoginAsync(AESEncryptModel aesEncryptModel, string sessionId);
    }
}
