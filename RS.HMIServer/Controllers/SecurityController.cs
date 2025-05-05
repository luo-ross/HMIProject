using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.HMIServer.BLL;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using RS.Models;

namespace RS.HMIServer.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    public class SecurityController : BaseController
    {
        private readonly ISecurityBLL SecurityBLL;
        private readonly ILogBLL LogBLL;

        public SecurityController(ISecurityBLL securityBLL, ILogBLL logBLL)
        {
            this.SecurityBLL = securityBLL;
            this.LogBLL = logBLL;
        }


        [HttpPost]
        [Authorize]
        public async Task<OperateResult> PasswordResetEmailSend(AESEncryptModel aesEncryptModel)
        {
            return await SecurityBLL.PasswordResetEmailSendAsync(aesEncryptModel, SessionId, Audiences);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> EmailPasswordResetConfirm(AESEncryptModel aesEncryptModel)
        {
            return await SecurityBLL.EmailPasswordResetConfirmAsync(aesEncryptModel, SessionId, Audiences);
        }

        /// <summary>
        /// 这里让用户必须通过Post才能获取到图像数据
        /// </summary>
        [HttpGet]
        [Authorize]
        public async Task<OperateResult<AESEncryptModel>> GetImgVerifyModel()
        {
            return await this.SecurityBLL.GetImgVerifyModelAsync(SessionId, Audiences);
        }


        /// <summary>
        /// 验证登录
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <returns></returns>
        [HttpPost]
        [Authorize]
        public async Task<OperateResult<AESEncryptModel>> ValidLogin(AESEncryptModel aesEncryptModel)
        {
            return await this.SecurityBLL.ValidLoginAsync(aesEncryptModel, SessionId, Audiences);
        }
    }
}