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
        public async Task<OperateResult<AESEncryptModel>> GetEmailVerify(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.GetEmailVerifyAsync(aesEncryptModel,this. SessionId, this.Audiences);
            return handleResult;
        }

        /// <summary>
        /// 注册邮箱验证码是否正确
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize(Roles = "EmailVerifyValid")]
        public async Task<OperateResult> EmailVerifyValid(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.EmailVerifyValidAsync(aesEncryptModel, this.SessionId,this.Audiences);
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
        public async Task<OperateResult<AESEncryptModel>> GetSMSVerify(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.GetSMSVerifyAsync(aesEncryptModel, this.SessionId, this.Audiences);
            return handleResult;
        }

        /// <summary>
        /// 注册短信验证码是否正确
        /// </summary>
        /// <param name="aesEncryptModel"></param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult> SMSVerifyValid(AESEncryptModel aesEncryptModel)
        {
            var handleResult = await RegisterBLL.SMSVerifyValidAsync(aesEncryptModel, this.SessionId, this.Audiences);
            return handleResult;
        }
        #endregion
    }
}
