using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.Commons;
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
            SecurityBLL = securityBLL;
            LogBLL = logBLL;
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
    }
}