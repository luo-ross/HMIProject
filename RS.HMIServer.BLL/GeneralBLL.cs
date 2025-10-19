using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.Models;
using System.Collections;
using System.Security.Claims;
using System.Text;
using RS.HMIServer.Models;
using System.Security.Cryptography;

namespace RS.HMIServer.BLL
{
    [ServiceInjectConfig(typeof(IGeneralBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class GeneralBLL : IGeneralBLL
    {
        private readonly IConfiguration Configuration;
        private readonly ICryptographyBLL CryptographyBLL;
        private readonly IMemoryCache MemoryCache;
        private readonly IGeneralDAL GeneralDAL;
        public GeneralBLL(ICryptographyBLL cryptographyBLL, IMemoryCache memoryCache, IConfiguration configuration, IGeneralDAL generalDAL)
        {
            CryptographyBLL = cryptographyBLL;
            MemoryCache = memoryCache;
            Configuration = configuration;
            GeneralDAL = generalDAL;
        }

        public OperateResult<string> GenerateJWTToken(List<Claim> claimList, string audiencesType, DateTime? expires = null)
        {
            //默认7天
            if (expires == null)
            {
                expires = DateTime.UtcNow.AddDays(7);
            }
            var tokenHandler = new JsonWebTokenHandler();
            var subject = new ClaimsIdentity();
            foreach (var claim in claimList)
            {
                subject.AddClaim(claim);
            }
            string tokenSecurityKey = Configuration["JWTConfig:SecurityKey"];
            string issuer = Configuration["JWTConfig:Issuer"];


            //这里实际上是就是添加了一堆Claim
            SecurityTokenDescriptor tokenDescriptor = new SecurityTokenDescriptor()
            {
                Issuer = issuer,
                Audience = audiencesType,
                Subject = subject,
                Expires = expires,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey)), SecurityAlgorithms.HmacSha256Signature)
            };
            string token = tokenHandler.CreateToken(tokenDescriptor);


            return OperateResult.CreateSuccessResult<string>(token);



        }




        public async Task<OperateResult<string>> GetClientIdAsync(LoginClientModel loginClientModel)
        {
            string clientIPHash = GetClientIPHashCode(loginClientModel);
            loginClientModel.ClientIPHash = clientIPHash;
            OperateResult<string> saveClientIdResult = await this.GeneralDAL.SaveClientIdAsync(loginClientModel);
            return saveClientIdResult;
        }

        public async Task<OperateResult> ValidCliendIdAsync(LoginClientModel loginClientModel, string clientId)
        {
            //计算IP的哈希值
            string clientIPHash = GetClientIPHashCode(loginClientModel);
            loginClientModel.ClientIPHash = clientIPHash;
            //通过用户提供个ClientId查询是否是服务端发放的
            OperateResult<LoginClientModel> getLoginClientModelResult = await this.GeneralDAL.GetLoginClientModelAsync(clientId);
            if (!getLoginClientModelResult.IsSuccess)
            {
                return getLoginClientModelResult;
            }

            //如果是服务端发放的 那么久获取这个存储起来的访问数据
            var loginClientModelExist = getLoginClientModelResult.Data;

            //验证这个ClientId是否是窃取的
            var isSameLoginClientModelResult = await this.IsSameLoginClientModelAsync(loginClientModel, loginClientModelExist);
            //如果验证不通过 说明是盗取的
            if (!isSameLoginClientModelResult.IsSuccess)
            {
                return isSameLoginClientModelResult;
            }
            //到这里说明验证通过了 这个ClientId是有效的

            return await this.GeneralDAL.IsClientIPExistAsync(loginClientModel, clientId);
        }

        private string GetClientIPHashCode(LoginClientModel loginClientModel)
        {
            return this.CryptographyBLL.GetMD5HashCode($"{loginClientModel.RemoteIpAddress}{loginClientModel.LocalIpAddress}{loginClientModel.XForwardedFor}");
        }

        public async Task<OperateResult> IsSameLoginClientModelAsync(LoginClientModel loginClientModel, LoginClientModel loginClientModelExist)
        {
            //比较IP哈希是否相同
            if (!loginClientModel.ClientIPHash.Equals(loginClientModelExist.ClientIPHash))
            {
                return OperateResult.CreateFailResult();
            }

            //LocalIpAddress
            if (!loginClientModel.LocalIpAddress.Equals(loginClientModelExist.LocalIpAddress))
            {
                return OperateResult.CreateFailResult();
            }

            //RemoteIpAddress
            if (!loginClientModel.RemoteIpAddress.Equals(loginClientModelExist.RemoteIpAddress))
            {
                return OperateResult.CreateFailResult();
            }

            //XForwardedFor
            if (loginClientModel.XForwardedFor != null && !loginClientModel.XForwardedFor.Equals(loginClientModelExist.XForwardedFor))
            {
                return OperateResult.CreateFailResult();
            }

            return OperateResult.CreateSuccessResult();
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
            var aesDecryptResult = CryptographyBLL.AESDecryptGeneric<T>(aesEncryptModel, sessionModel);
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
            var aesEncryptResult = this.CryptographyBLL.AESEncryptGeneric(encryptModelShould, sessionModel);
            if (!aesEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(aesEncryptResult);
            }
            return aesEncryptResult;
        }

        public async Task<OperateResult<SessionResultModel>> GetSessionResultModelAsync(SessionRequestModel sessionRequestModel, string sessionId)
        {
            //获取服务端签名公钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSASignPublicKey, out string globalRSASignPublicKey);
            if (string.IsNullOrEmpty(globalRSASignPublicKey))
            {
                return OperateResult.CreateFailResult<SessionResultModel>("获取服务端签名公钥失败！");
            }

            //获取服务端加密公钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAEncryptPublicKey, out string globalRSAEncryptPublicKey);
            if (string.IsNullOrEmpty(globalRSAEncryptPublicKey))
            {
                return OperateResult.CreateFailResult<SessionResultModel>("获取服务端加解公钥失败！");
            }

            //生成AES对称秘钥
            string aesKey = CryptographyBLL.GenerateAESKey();

            //通过客户端传递过来的加密公钥加密数据
            var rsaEncryptResult = CryptographyBLL.RSAEncrypt(aesKey, sessionRequestModel.RSAEncryptPublicKey);
            if (!rsaEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(rsaEncryptResult);
            }
            string aesKeyEncrypt = rsaEncryptResult.Data;

            //创建AppId

            string appId = Guid.NewGuid().ToString();


            //RSA非对称加密
            rsaEncryptResult = CryptographyBLL.RSAEncrypt(appId, sessionRequestModel.RSAEncryptPublicKey);
            if (!rsaEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(rsaEncryptResult);
            }
            string appIdEncrypt = rsaEncryptResult.Data;


            var claimList = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, sessionId)
            };


            //通过JWT 生成Token  待处理
            var generateJWTTokenResult = this.GenerateJWTToken(claimList, sessionRequestModel.AudiencesType);
            if (!generateJWTTokenResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(generateJWTTokenResult);
            }
            string token = generateJWTTokenResult.Data;


            //创建返回值
            SessionResultModel sessionResultModel = new SessionResultModel()
            {
                //返回给客户端验证服务端签名的
                RSASignPublicKey = globalRSASignPublicKey,
                //返回给客户端用来加密数据给服务端的
                RSAEncryptPublicKey = globalRSAEncryptPublicKey,
                Nonce = CryptographyBLL.CreateRandCode(10),
                TimeStamp = DateTime.UtcNow.ToTimeStampString(),
                SessionModel = new SessionModel()
                {
                    AesKey = aesKeyEncrypt,
                    AppId = appIdEncrypt,
                    Token = token,
                },
            };


            //把创建好的会话数据写入到Redis进行存储
            string aesKeyProtect = CryptographyBLL.ProtectData(aesKey);
            string appIdProtect = CryptographyBLL.ProtectData(appId);
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
                sessionResultModel.RSASignPublicKey,
                sessionResultModel.RSAEncryptPublicKey,
                sessionResultModel.TimeStamp,
                sessionResultModel.Nonce
            };

            //获取会话的Hash数据
            var getHashResult = CryptographyBLL.GetRSAHash(arrayList);
            if (!getHashResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(getHashResult);
            }

            //获取服务私钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSASignPrivateKey, out byte[] globalRSASignPrivateKey);
            if (globalRSASignPrivateKey == null || globalRSASignPrivateKey.Length == 0)
            {
                return OperateResult.CreateFailResult<SessionResultModel>("获取服务私钥失败！");
            }

            //进行RSA数据签名
            var rsaSignDataResult = CryptographyBLL.RSASignData(getHashResult.Data, globalRSASignPrivateKey);
            if (!rsaSignDataResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<SessionResultModel>(rsaSignDataResult);
            }
            sessionResultModel.MsgSignature = rsaSignDataResult.Data;
            return OperateResult.CreateSuccessResult(sessionResultModel);
        }



    }
}
