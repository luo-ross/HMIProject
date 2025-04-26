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
    /// <summary>
    /// 安全服务接口
    /// </summary>
    public interface ISecurityBLL
    {

        /// <summary>
        /// 密码重置
        /// </summary>
        /// <returns></returns>
        Task<OperateResult> PasswordResetEmailSendAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences);

        /// <summary>
        /// 密码重置确认
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <param name="audiences">客户端类型</param>
        /// <returns></returns>
        Task<OperateResult> EmailPasswordResetConfirmAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences);

        /// <summary>
        /// 获取验证码信息
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="audiences">客户端类型</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetImgVerifyModelAsync(string sessionId, string audiences);
    }
}
