using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RS.WebApp.IBLL;
using RS.WebApp.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.Models;
using System.Collections;
using System.Security.Claims;
using System.Text;

namespace RS.WebApp.BLL
{
    [ServiceInjectConfig(typeof(IGeneralService), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class GeneralService : IGeneralService
    {
        private readonly IConfiguration Configuration;
        private readonly ICryptographyService CryptographyService;
        private readonly IMemoryCache MemoryCache;
        private readonly IGeneralDAL GeneralDAL;
        public GeneralService(ICryptographyService cryptographyService, IMemoryCache memoryCache, IConfiguration configuration, IGeneralDAL generalDAL)
        {
            CryptographyService = cryptographyService;
            MemoryCache = memoryCache;
            Configuration = configuration;
            GeneralDAL = generalDAL;
        }

        public OperateResult<string> GenerateJWTToken(string audienceType, List<Claim> claimList)
        {
            var tokenHandler = new JsonWebTokenHandler();
            var subject = new ClaimsIdentity();
            subject.AddClaim(new Claim(ClaimTypes.Role, "Test"));
            foreach (var claim in claimList)
            {
                subject.AddClaim(claim);
            }
            string tokenSecurityKey = Configuration["ConnectionStrings:JWTConfig:SecurityKey"];
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = "http://wpf.mycompany.com/authapi",
                Audience = audienceType,
                Subject = subject,
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey)), SecurityAlgorithms.HmacSha256Signature)
            };
            string token = tokenHandler.CreateToken(tokenDescriptor);

            return OperateResult.CreateSuccessResult<string>(token);
        }


        public async Task<OperateResult<T>> GetAESDecryptAsync<T>(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //通过SessionId获取SessionModel
            var getSessionModelResult = await this.GeneralDAL.GetSessionModelAsync(sessionId);
            if (!getSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<T>(getSessionModelResult);
            }
            var sessionModel = getSessionModelResult.Data;

            //对数据解密
            var aesDecryptResult = CryptographyService.AESDecrypt<T>(aesEncryptModel, sessionModel);
            if (!aesDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<T>(aesDecryptResult);
            }

            return aesDecryptResult;
        }


        public async Task<OperateResult<AESEncryptModel>> GetAESEncryptAsync<T>(T encryptModelShould, string sessionId)
        {
            //通过SessionId获取SessionModel
            var getSessionModelResult = await this.GeneralDAL.GetSessionModelAsync(sessionId);
            if (!getSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getSessionModelResult);
            }
            var sessionModel = getSessionModelResult.Data;


            //对返回的数据进行加密
            var aesEncryptResult = CryptographyService.AESEncrypt(encryptModelShould, sessionModel);
            if (!aesEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(aesEncryptResult);
            }
            return aesEncryptResult;
        }

        public async Task<OperateResult<SessionResultModel>> GetSessionModelAsync(SessionRequestModel sessionRequestModel)
        {
            //获取服务端公钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAPublicKey, out string serverRSAPublicKey);
            if (string.IsNullOrEmpty(serverRSAPublicKey))
            {
                return OperateResult.CreateFailResult<SessionResultModel>("获取服务端公钥失败！");
            }

            //生成AES对称秘钥
            string aesKey = CryptographyService.GenerateAESKey();

            //通过客户端传递过来的公钥加密AES秘钥
            var rsaEncryptResult = CryptographyService.RSAEncrypt(aesKey, sessionRequestModel.RsaPublicKey);
            if (!rsaEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(rsaEncryptResult);
            }
            string aesKeyEncrypt = rsaEncryptResult.Data;

            //创建会话ID 也可以说是AppId
            string appId = Guid.NewGuid().ToString();

            //RSA非对称加密
            rsaEncryptResult = CryptographyService.RSAEncrypt(appId, sessionRequestModel.RsaPublicKey);
            if (!rsaEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(rsaEncryptResult);
            }
            string appIdEncrypt = rsaEncryptResult.Data;

            //创建返回值
            SessionResultModel sessionResultModel = new SessionResultModel()
            {
                RsaPublicKey = serverRSAPublicKey,
                Nonce = CryptographyService.CreateRandCode(10),
                TimeStamp = DateTime.UtcNow.ToTimeStampString(),
                SessionModel = new SessionModel()
                {
                    AesKey = aesKeyEncrypt,
                    AppId = appIdEncrypt,
                }
            };

            string sessionId = Guid.NewGuid().ToString();
            var claimList = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, sessionId)
            };

            //通过JWT 生成Token  待处理
            var generateJWTTokenResult = this.GenerateJWTToken(sessionRequestModel.AudienceType, claimList);
            if (!generateJWTTokenResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(generateJWTTokenResult);
            }
            string token = generateJWTTokenResult.Data;
            sessionResultModel.SessionModel.Token = token;


            //把创建好的会话数据写入到Redis进行存储
            string aesKeyProtect = CryptographyService.ProtectData(aesKey);
            string appIdProtect = CryptographyService.ProtectData(appId);
            var saveSessionModelResult = await GeneralDAL.SaveSessionModelAsync(new SessionModel()
            {
                AppId = appIdProtect,
                AesKey = aesKeyProtect,
                Token = token,
            }, sessionId);

            if (!saveSessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(saveSessionModelResult);
            }

            //将数据按顺序放入数组
            ArrayList arrayList = new ArrayList
            {
                sessionResultModel.SessionModel.AesKey,
                sessionResultModel.SessionModel.Token,
                sessionResultModel.SessionModel.AppId,
                sessionResultModel.RsaPublicKey,
                sessionResultModel.TimeStamp,
                sessionResultModel.Nonce
            };

            //获取会话的Hash数据
            var getHashResult = CryptographyService.GetRSAHash(arrayList);
            if (!getHashResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(getHashResult);
            }

            //获取服务私钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAPrivateKey, out byte[] serverRSAPrivateKey);
            if (serverRSAPrivateKey == null || serverRSAPrivateKey.Length == 0)
            {
                return OperateResult.CreateFailResult<SessionResultModel>("获取服务私钥失败！");
            }

            //进行RSA数据签名
            var rsaSignDataResult = CryptographyService.RSASignData(getHashResult.Data, serverRSAPrivateKey);
            if (!rsaSignDataResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(rsaSignDataResult);
            }
            sessionResultModel.MsgSignature = rsaSignDataResult.Data;



            return OperateResult.CreateSuccessResult(sessionResultModel);
        }
    }
}
