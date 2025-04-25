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
            this. SecurityBLL = securityBLL;
            this.LogBLL = logBLL;
        }


        [HttpPost]
        [Authorize]
        public async Task<OperateResult> PasswordResetEmailSend(AESEncryptModel aesEncryptModel)
        {
            return await SecurityBLL.PasswordResetEmailSendAsync(aesEncryptModel,  SessionId, Audiences);
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
        public async Task<OperateResult<AESEncryptModel>> GetVerifyImgModel()
        {
            return await this.SecurityBLL.GetVerifyImgModelAsync(SessionId, Audiences);
            //return File(combinedBytes, "application/octet-stream");
        }
    }
}