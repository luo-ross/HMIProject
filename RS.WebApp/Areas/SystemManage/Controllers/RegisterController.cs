using Microsoft.AspNetCore.Mvc;

namespace RS.WebApp.Areas.SystemManage.Controllers
{
    [ApiExplorerSettings(IgnoreApi = true)]
    public class RegisterController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
