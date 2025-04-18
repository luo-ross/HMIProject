using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RazorLight;
using RS.Commons;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using RS.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RS.HMIServer.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    [Authorize]
    public class SecurityController : BaseController
    {
        private readonly ISecurityBLL SecurityBLL;
        private readonly ILogBLL LogBLL;

        public SecurityController(ISecurityBLL securityBLL,  ILogBLL logBLL)
        {
            SecurityBLL = securityBLL;
            LogBLL = logBLL;
        }
     

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> PasswordResetAsync(AESEncryptModel aesEncryptModel)
        {
            return await SecurityBLL.PasswordResetAsync(aesEncryptModel,HostWithScheme,  SessionId, Audiences);
        }

        [HttpPost]
        [Authorize]
        public async Task<OperateResult> PasswordConfirmAsync(PasswordConfirmModel passwordConfirmModel)
        {
            return OperateResult.CreateSuccessResult();
        }
    }
}