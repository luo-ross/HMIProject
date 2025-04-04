using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RazorLight;
using RS.Commons;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RS.HMIServer.Areas.SystemManage.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Area("SystemManage")]
    [Route("/[area]/[controller]/[action]")]
    public class SecurityController : BaseController
    {
        private readonly ISecurityBLL SecurityBLL;
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly ILogBLL LogBLL;

        public SecurityController(ISecurityBLL securityBLL, IHttpContextAccessor httpContextAccessor, ILogBLL logBLL)
        {
            SecurityBLL = securityBLL;
            HttpContextAccessor = httpContextAccessor;
            LogBLL = logBLL;
        }
      

        public IActionResult ForgetPassword()
        {
            return View();
        }

        public async Task<IActionResult> Form([FromQuery] string email, [FromQuery] string token)
        {
            ////验证密码重置会话是否存在
            //var operateResult = await this.SecurityBLL.PasswordResetSessionValidAsync(email, token);
            //if (!operateResult.IsSuccess)
            //{
            //    //不存在跳转到密码重置主页
            //    return RedirectToAction("Index");
            //}
            return View();
        }

        [HttpPost]
        public async Task<OperateResult> PasswordResetAsync([FromBody] PasswordResetModel passwordResetModel)
        {
            return await SecurityBLL.PasswordResetAsync(HostWithScheme, passwordResetModel);
        }

        [HttpPost]
        public async Task<OperateResult> PasswordConfirmAsync([FromBody] PasswordConfirmModel passwordConfirmModel)
        {

            return OperateResult.CreateSuccessResult();
        }
    }
}