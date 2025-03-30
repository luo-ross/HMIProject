using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.WebApp.IBLL;

namespace RS.WebApp.Areas.WebApi.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    public class MarketingController : Controller
    {
        private readonly IGeneralService GeneralService;
        private readonly ICryptographyService CryptographyService;
        private readonly ILogService LogService;
        public MarketingController(IGeneralService generalService, ICryptographyService cryptographyService, ILogService logService)
        {
            GeneralService = generalService;
            LogService = logService;
            CryptographyService = cryptographyService;
        }

        [HttpGet]
        public OperateResult GetAdvertisementLink()
        {
            List<string> urlList = new List<string>();
            urlList.Add("https://img.tukuppt.com/bg_grid/00/22/50/oduSd3Hmgz.jpg!/fh/350");
            urlList.Add("https://img.tukuppt.com/bg_grid/01/60/68/gaxGfDG7Bk.jpg!/fh/350");
            urlList.Add("https://img.tukuppt.com/bg_grid/00/13/16/bBO4ci9EsW.jpg!/fh/350");
            return OperateResult.CreateSuccessResult(urlList);
        }

    }
}
