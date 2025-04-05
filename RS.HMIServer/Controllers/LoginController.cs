using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.HMIServer.Areas;
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

        //public async Task<IActionResult> Default()
        //{
        //    //获取请求的网络信息 
        //    string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
        //    string localIpAddress = this.HttpContext.Connection.LocalIpAddress.ToString();
        //    string xForwardedFor = this.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        //    string userAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
        //    OperateResult<string> operateResult = await this.GeneralBLL.GetClientIdAsync(new LoginClientModel()
        //    {
        //        LocalIpAddress=localIpAddress,
        //        RemoteIpAddress=remoteIpAddress,
        //        UserAgent=userAgent,
        //        XForwardedFor=xForwardedFor,
        //    });
        //    if (!operateResult.IsSuccess)
        //    {
        //        return Redirect("/ServerError");
        //    }
        //    var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        //    return RedirectToAction("Index", new { ClientId = operateResult.Data, TimeStamp = timeStamp });
        //}

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