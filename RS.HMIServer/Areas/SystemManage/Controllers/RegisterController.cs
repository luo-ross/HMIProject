using Microsoft.AspNetCore.Mvc;

namespace RS.HMIServer.Areas.SystemManage.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    [Area("SystemManage")]
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
