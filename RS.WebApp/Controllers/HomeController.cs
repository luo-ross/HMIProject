using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.Commons.Interceptors;
using RS.WebApp.Models;
using System.Diagnostics;

namespace RS.WebApp.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class HomeController : Controller
    {
        private readonly ILogService LogService;

        public HomeController(ILogService logService)
        {
            LogService = logService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}