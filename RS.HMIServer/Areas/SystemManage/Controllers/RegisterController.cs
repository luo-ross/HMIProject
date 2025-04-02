using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Hosting;
using RS.Commons;
using RS.HMIServer.IBLL;
using System.Collections.Generic;
using System.Linq;

namespace RS.HMIServer.Areas.SystemManage.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Area("SystemManage")]
    [Route("/[area]/[controller]/[action]")]
    public class RegisterController : BaseController
    {
        private readonly IHttpContextAccessor HttpContextAccessor;
        private readonly ILogService LogService;

        public RegisterController(IHttpContextAccessor httpContextAccessor, ILogService logService)
        {
            this.HttpContextAccessor = httpContextAccessor;
            this.LogService = logService;
        }
        public IActionResult Init()
        {
            //这里先模拟 后面待完善
            var clientId = Guid.NewGuid().ToString();
            var request = this.HttpContextAccessor.HttpContext.Request;
            //添加客户端Id
            request.RouteValues.Add("clientId", clientId);
            foreach (var item in request.Query)
            {
                request.RouteValues.Add(item.Key, item.Value);
            }
            return RedirectToAction("Index", "Register", request.RouteValues);
        }



        public IActionResult Index(string clientId)
        {
            return View();
        }
    }
}
