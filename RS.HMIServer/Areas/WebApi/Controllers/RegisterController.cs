using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.HMIServer.IBLL;
using RS.Commons;
using RS.Models;

namespace RS.HMIServer.Areas.WebApi.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    [Authorize]
    public class RegisterController : BaseController
    {
        /// <summary>
        /// 注册服务接口
        /// </summary>
        private readonly IRegisterBLL RegisterBLL;
        private readonly ILogBLL LogBLL;
        public RegisterController(IRegisterBLL registerBLL, ILogBLL logBLL)
        {
            RegisterBLL = registerBLL;
            LogBLL = logBLL;
        }

        #region 注册邮箱验证业务处理
        /// <summary>
        /// 获取注册邮箱验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult<AESEncryptModel>> GetEmailVerification(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.GetEmailVerificationAsync(aesEncryptModel, SessionId);
            return handleResult;
        }

        /// <summary>
        /// 注册邮箱验证码是否正确
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> EmailVerificationValid(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.EmailVerificationValidAsync(aesEncryptModel, SessionId);
            return handleResult;
        }

        #endregion

        #region 注册短信验证逻辑处理
        /// <summary>
        /// 获取注册短信验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult<AESEncryptModel>> GetSMSVerification(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.GetSMSVerificationAsync(aesEncryptModel, SessionId);
            return handleResult;
        }

        /// <summary>
        /// 注册短信验证码是否正确
        /// </summary>
        /// <param name="aesEncryptModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> SMSVerificationValid(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.SMSVerificationValidAsync(aesEncryptModel, SessionId);
            return handleResult;
        }
        #endregion
    }
}
