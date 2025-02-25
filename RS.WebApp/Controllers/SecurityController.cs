using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using RazorLight;
using RS.Commons;
using RS.Commons.LogService;
using RS.WebApp.IBLL;
using RS.WebApp.Models;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace RS.WebApp.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class SecurityController : BaseController
    {
        private readonly ISecurityService SecurityService;
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly ILogService LogService;

        public SecurityController(ISecurityService securityService, IHttpContextAccessor httpContextAccessor, ILogService logService)
        {
            this.SecurityService = securityService;
            this.HttpContextAccessor = httpContextAccessor;
            this.LogService = logService;
        }

        [Route("password/new")]
        public IActionResult Index()
        {
            return View();
        }

        [Route("password/edit")]
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

        [Route("password/reset")]
        [HttpPost]
        public async Task<OperateResult> PasswordResetAsync([FromBody] PasswordResetModel passwordResetModel)
        {
            return await this.SecurityService.PasswordResetAsync(this.HostWithScheme, passwordResetModel);
        }

        [Route("password/confirm")]
        [HttpPost]
        public async Task<OperateResult> PasswordConfirmAsync([FromBody]PasswordConfirmModel passwordConfirmModel)
        {

            return OperateResult.CreateSuccessResult();
        }
    }
}