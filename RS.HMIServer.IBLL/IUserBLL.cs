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
        /// 删除用户
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> DeleteUserAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetUsersAsync(AESEncryptModel aesEncryptModel  , string sessionId);

        /// <summary>
        /// 更新邮箱
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult> UpdateEmailAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 更新用户是否禁用
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult> UpdateIsDisableAsync(AESEncryptModel aesEncryptModel, string sessionId);


        /// <summary>
        /// 更新昵称
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult> UpdateNickNameAsync(AESEncryptModel aesEncryptModel, string sessionId);
    }
}
