using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RazorLight;
using RS.Commons;
using RS.WebApp.IBLL;
using RS.WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RS.WebApp.Areas.SystemManage.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Area("SystemManage")]
    public class SecurityController : BaseController
    {
        private readonly ISecurityService SecurityService;
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly ILogService LogService;

        public SecurityController(ISecurityService securityService, IHttpContextAccessor httpContextAccessor, ILogService logService)
        {
            SecurityService = securityService;
            HttpContextAccessor = httpContextAccessor;
            LogService = logService;
        }
     
        public IActionResult SignIn()
        {
            return View();
        }
        public IActionResult SignUp()
        {
            return View();
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> Form([FromQuery] string email, [FromQuery] string token)
        {
            ////验证密码重置会话是否存在
            //var operateResult = await this.SecurityService.PasswordResetSessionValidAsync(email, token);
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
            return await SecurityService.PasswordResetAsync(HostWithScheme, passwordResetModel);
        }

        [HttpPost]
        public async Task<OperateResult> PasswordConfirmAsync([FromBody] PasswordConfirmModel passwordConfirmModel)
        {

            return OperateResult.CreateSuccessResult();
        }
    }
}