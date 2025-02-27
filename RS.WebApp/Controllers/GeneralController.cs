using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.Models;
using RS.WebApp.IBLL;
using System.Collections;


namespace RS.WebApp.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    public class GeneralController : BaseController
    {
        private readonly IGeneralService GeneralService;
        private readonly ICryptographyService CryptographyService;
        private readonly ILogService LogService;
        public GeneralController(IGeneralService generalService, ICryptographyService cryptographyService,ILogService logService) 
        {
            this.GeneralService = generalService;
            this.GeneralService = generalService;
            this.LogService = logService;
            this.CryptographyService = cryptographyService;
        }

        /// <summary>
        /// 心跳检测接口
        /// </summary>
        /// <returns>服务器状态信息</returns>
        [HttpGet]
        public OperateResult HeartBeatCheck()
        {
            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 获取会话实体
        /// </summary>
        /// <param name="sessionRequestModel">会话请求参数</param>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult<SessionResultModel>> GetSessionModel(SessionRequestModel sessionRequestModel)
        {
            //验证签名
            ArrayList arrayList = new ArrayList
            {
                sessionRequestModel.RsaPublicKey,
                sessionRequestModel.TimeStamp,
                sessionRequestModel.Nonce
            };

            //获取会话的Hash数据
            var getHashResult = CryptographyService.GetRSAHash(arrayList);
            if (!getHashResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(getHashResult);
            }

            //获取签名
            var signature = Convert.FromBase64String(sessionRequestModel.MsgSignature);
            var verifyDataResult = CryptographyService.RSAVerifyData(getHashResult.Data, signature, sessionRequestModel.RsaPublicKey);

            if (!verifyDataResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>("你无权访问该接口!");
            }

            //获取会话
            var getSessionModelResult = await GeneralService.GetSessionModelAsync(sessionRequestModel);
            if (!getSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(getSessionModelResult);
            }
            return OperateResult.CreateSuccessResult(getSessionModelResult.Data);
        }
    }
}
