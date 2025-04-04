using RS.HMIServer.Models;
using RS.Commons;
using RS.Commons.Enums;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IBLL
{
    public interface IGeneralBLL
    {
        /// <summary>
        /// 生成JWTToken
        /// </summary>
        /// <param name="audienceType">客户端类型</param>
        /// <param name="claimList">声明列表</param>
        /// <returns></returns>
        OperateResult<string> GenerateJWTToken(string audienceType, List<Claim> claimList);
       
        /// <summary>
        /// 获取客户端和服务端的加密会话
        /// </summary>
        /// <param name="sessionRequestModel"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<OperateResult<SessionResultModel>> GetSessionModelAsync(SessionRequestModel sessionRequestModel, string sessionId);

        /// <summary>
        /// 获取AES解密数据
        /// </summary>
        /// <typeparam name="T">解密类型</typeparam>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<T>> GetAESDecryptAsync<T>(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 获取AES加密数据
        /// </summary>
        /// <typeparam name="T">加密类型</typeparam>
        /// <param name="encryptModelShould">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetAESEncryptAsync<T>(T encryptModelShould, string sessionId);


        Task<OperateResult<string>> GetClientIdAsync(LoginClientModel loginClientModel);
        Task<OperateResult> ValidCliendIdAsync(LoginClientModel loginClientModel, string clientId);

    }
}
