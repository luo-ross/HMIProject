using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.HMIServer.Areas;
using RS.HMIServer.Models;
using System.Diagnostics;

namespace RS.HMIServer.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class LoginController : BaseController
    {
        private readonly ILogService LogService;

        public LoginController(ILogService logService)
        {
            LogService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult QRCode()
        {
            return View();
        }
    }
}