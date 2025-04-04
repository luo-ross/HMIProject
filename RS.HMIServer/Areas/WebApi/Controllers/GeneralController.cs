using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.Models;
using RS.HMIServer.IBLL;
using System.Collections;
using RS.HMIServer.Models;


namespace RS.HMIServer.Areas.WebApi.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    public class GeneralController : BaseController
    {
        private readonly IGeneralBLL GeneralBLL;
        private readonly ICryptographyBLL CryptographyBLL;
        private readonly ILogBLL LogBLL;
        public GeneralController(IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL, ILogBLL logBLL)
        {
            GeneralBLL = generalBLL;
            LogBLL = logBLL;
            CryptographyBLL = cryptographyBLL;
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
            var getHashResult = CryptographyBLL.GetRSAHash(arrayList);
            if (!getHashResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(getHashResult);
            }

            //获取签名

            if (string.IsNullOrEmpty(sessionRequestModel.MsgSignature)
                || string.IsNullOrWhiteSpace(sessionRequestModel.MsgSignature))
            {
                return OperateResult.CreateFailResult<SessionResultModel>("数据签名不能为空!");
            }
            byte[] signature = null;
            try
            {
                signature = Convert.FromBase64String(sessionRequestModel.MsgSignature);
            }
            catch (FormatException)
            {
                return OperateResult.CreateFailResult<SessionResultModel>("数据签名格式不正确!");
            }
            var verifyDataResult = CryptographyBLL.RSAVerifyData(getHashResult.Data, signature, sessionRequestModel.RsaPublicKey);

            if (!verifyDataResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>("你无权访问该接口!");
            }

            //获取请求的网络信息 
            string remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress.ToString();
            string localIpAddress = this.HttpContext.Connection.LocalIpAddress.ToString();
            string xForwardedFor = this.HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
            string userAgent = this.HttpContext.Request.Headers["User-Agent"].ToString();
            OperateResult<string> operateResult = await this.GeneralBLL.GetClientIdAsync(new LoginClientModel()
            {
                LocalIpAddress = localIpAddress,
                RemoteIpAddress = remoteIpAddress,
                UserAgent = userAgent,
                XForwardedFor = xForwardedFor,
            });

            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>("创建会话失败");
            }

            var sessionId = operateResult.Data;


            //获取会话
            var getSessionModelResult = await GeneralBLL.GetSessionModelAsync(sessionRequestModel, sessionId);
            if (!getSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(getSessionModelResult);
            }





            return OperateResult.CreateSuccessResult(getSessionModelResult.Data);
        }
    }
}
