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

        private readonly ILogBLL LogBLL;

        public RegisterController(ILogBLL logBLL)
        {
            this.LogBLL = logBLL;
        }

        public IActionResult Index()
        {
            return View();
        }
    }
}
