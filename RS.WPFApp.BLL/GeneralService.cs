using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Models;
using RS.RESTfulApi;
using RS.WPFApp.IBLL;
using System.Collections;


namespace RS.WPFApp.BLL
{
    [ServiceInjectConfig(typeof(IGeneralService), ServiceLifetime.Transient, IsInterceptor = true)]

    internal class GeneralService : IGeneralService
    {
        private readonly IMemoryCache MemoryCache;
        private readonly ICryptographyService CryptographyService;
        public GeneralService(ICryptographyService cryptographyService, IMemoryCache memoryCache)
        {
            CryptographyService = cryptographyService;
            MemoryCache = memoryCache;
        }

       

      



        /// <summary>
        /// 创建会话
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> GetSessionModelAsync()
        {

            //获取客户端公钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAPublicKey, out string rSAPublicKey);
            if (string.IsNullOrEmpty(rSAPublicKey))
            {
                return OperateResult.CreateFailResult("获取客户端公钥失败！");
            }


            //创建会话请求
            SessionRequestModel sessionRequestModel = new SessionRequestModel()
            {
                RsaPublicKey = rSAPublicKey,
                Nonce = CryptographyService.CreateRandCode(10),
                TimeStamp = DateTime.UtcNow.ToTimeStampString(),
                AudienceType = AudienceType.WindowsAudience,
            };

            //数据按照顺序组成数组
            ArrayList arrayList = new ArrayList
            {
                sessionRequestModel.RsaPublicKey,
                sessionRequestModel.TimeStamp,
                sessionRequestModel.Nonce
            };

            //获取会话的Hash数据
            var getRSAHashResult = CryptographyService.GetRSAHash(arrayList);
            if (!getRSAHashResult.IsSuccess)
            {
                return getRSAHashResult;
            }

            //获取客户端私钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAPrivateKey, out byte[] rsaPrivateKey);
            if (rsaPrivateKey == null || rsaPrivateKey.Length == 0)
            {
                return OperateResult.CreateFailResult("获取客户端私钥失败！");
            }

            //进行RSA数据签名
            var rsaSignDataResult = CryptographyService.RSASignData(getRSAHashResult.Data, rsaPrivateKey);
            if (!rsaSignDataResult.IsSuccess)
            {
                return rsaSignDataResult;
            }
            sessionRequestModel.MsgSignature = rsaSignDataResult.Data;

            //往服务端发送数据 并获取回传数据
            var aesEncryptModelResult = await RSAppAPI.GetSessionModel.HttpPostAsync<SessionRequestModel, SessionResultModel>(nameof(RSAppAPI), sessionRequestModel);
            if (!aesEncryptModelResult.IsSuccess)
            {
                return aesEncryptModelResult;
            }
            var sessionResultModel = aesEncryptModelResult.Data;

            //数据按照顺序组成数组
            arrayList = new ArrayList
            {
                sessionResultModel.SessionModel.AesKey,
                sessionResultModel.SessionModel.Token,
                sessionResultModel.SessionModel.AppId,
                sessionResultModel.RsaPublicKey,
                sessionResultModel.TimeStamp,
                sessionResultModel.Nonce
            };

            //获取会话的Hash数据
            getRSAHashResult = CryptographyService.GetRSAHash(arrayList);
            if (!getRSAHashResult.IsSuccess)
            {
                return getRSAHashResult;
            }

            //获取签名
            var signature = Convert.FromBase64String(sessionResultModel.MsgSignature);
            var verifyDataResult = CryptographyService.RSAVerifyData(getRSAHashResult.Data, signature, sessionResultModel.RsaPublicKey);

            if (!verifyDataResult.IsSuccess)
            {
                return verifyDataResult;
            }

            //解密AesKey
            var rsaDecryptResult = CryptographyService.RSADecrypt(sessionResultModel.SessionModel.AesKey, rsaPrivateKey);
            if (!rsaDecryptResult.IsSuccess)
            {
                return rsaDecryptResult;
            }
            sessionResultModel.SessionModel.AesKey = rsaDecryptResult.Data;

            //解密AppId
            rsaDecryptResult = CryptographyService.RSADecrypt(sessionResultModel.SessionModel.AppId, rsaPrivateKey);
            if (!rsaDecryptResult.IsSuccess)
            {
                return rsaDecryptResult;
            }
            sessionResultModel.SessionModel.AppId = rsaDecryptResult.Data;

            //把会话数据存储在缓存里
            MemoryCache.Set(MemoryCacheKey.SessionModelKey, sessionResultModel.SessionModel);

            //将服务端公钥存储在缓存里
            MemoryCache.Set(MemoryCacheKey.GlobalRSAPublicKey, sessionResultModel.RsaPublicKey);

            return OperateResult.CreateSuccessResult();
        }

     
    }
}
