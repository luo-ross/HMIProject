using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.WebApp.IBLL;
using RS.Commons;
using RS.Models;
using RS.Commons.LogService;

namespace RS.WebApp.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    [Authorize]
    public class RegisterController : BaseController
    {
        /// <summary>
        /// 注册服务接口
        /// </summary>
        private readonly IRegisterService RegisterService;
        private readonly ILogService LogService;
        public RegisterController(IRegisterService registerService, ILogService logService)
        {
            this.RegisterService = registerService;
            this.LogService = logService;
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
            var handleResult = await RegisterService.GetEmailVerificationAsync(aesEncryptModel, this.SessionId);
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
            var handleResult = await RegisterService.EmailVerificationValidAsync(aesEncryptModel, this.SessionId);
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
            var handleResult = await RegisterService.GetSMSVerificationAsync(aesEncryptModel, this.SessionId);
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
            var handleResult = await RegisterService.SMSVerificationValidAsync(aesEncryptModel, this.SessionId);
            return handleResult;
        }
        #endregion
    }
}
