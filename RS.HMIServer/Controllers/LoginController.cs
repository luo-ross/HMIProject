using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.Controllers
{

    [ApiExplorerSettings(IgnoreApi = true)]
    public class LoginController : BaseController
    {
        private readonly ILogBLL LogBLL;
        private readonly ILoginBLL LoginBLL;
        private readonly IGeneralBLL GeneralBLL;
        public LoginController(ILoginBLL loginBLL, IGeneralBLL generalBLL, ILogBLL logBLL)
        {
            this.LogBLL = logBLL;
            this.LoginBLL = loginBLL;
            this.GeneralBLL = generalBLL;
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