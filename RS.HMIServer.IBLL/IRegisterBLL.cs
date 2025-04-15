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
    /// 注册服务接口
    /// </summary>
    public interface IRegisterBLL
    {

        /// <summary>
        /// 获取邮箱验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetEmailVerifyAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 邮箱验证码验证
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        Task<OperateResult> EmailVerifyValidAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 获取短信验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetSMSVerifyAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 邮箱验证码验证
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        Task<OperateResult> SMSVerifyValidAsync(AESEncryptModel aesEncryptModel, string sessionId);
    }
}
