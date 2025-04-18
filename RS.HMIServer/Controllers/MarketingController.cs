using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.HMIServer.IBLL;

namespace RS.HMIServer.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    public class MarketingController : Controller
    {
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        private readonly ILogBLL LogBLL;
        public MarketingController(IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL, ILogBLL logBLL)
        {
            GeneralBLL = generalBLL;
            LogBLL = logBLL;
            CryptographyBLL = cryptographyBLL;
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
