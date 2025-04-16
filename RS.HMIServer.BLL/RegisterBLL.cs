using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Commons.Helper;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Models;
using System.Security.Claims;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 账号注册服务
    /// </summary>
    [ServiceInjectConfig(typeof(IRegisterBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class RegisterBLL : IRegisterBLL
    {
        /// <summary>
        /// 注册数据仓储接口
        /// </summary>
        private readonly IRegisterDAL RegisterDAL;

        /// <summary>
        /// 邮箱服务
        /// </summary>
        private readonly IEmailBLL EmailService;

        /// <summary>
        /// 通用服务
        /// </summary>
        private readonly IGeneralBLL GeneralBLL;

        /// <summary>
        /// 短信服务
        /// </summary>
        private readonly ISMSBLL SMSBLL;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;



        /// <summary>
        /// 注册服务构造函数
        /// </summary>
        /// <param name="registerDAL">注册数据仓储</param>
        /// <param name="emailBLL">邮箱服务</param>
        /// <param name="generalBLL">通用服务</param>
        /// <param name="sMSService">短信服务</param>
        public RegisterBLL(IRegisterDAL registerDAL, IEmailBLL emailBLL, IGeneralBLL generalBLL, ISMSBLL sMSService, ICryptographyBLL cryptographyBLL)
        {
            this.RegisterDAL = registerDAL;
            this.EmailService = emailBLL;
            this.GeneralBLL = generalBLL;
            this.SMSBLL = sMSService;
            this.CryptographyBLL = cryptographyBLL;
        }


        /// <summary>
        /// 往客户端发送邮箱验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetEmailVerifyAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<EmailRegisterPostModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var emailRegisterPostModel = getAESDecryptResult.Data;

            //判断邮箱地址是否合法
            if (!emailRegisterPostModel.Email.IsEmail())
            {
                return OperateResult.CreateFailResult<AESEncryptModel>($"邮箱地址格式错误:{emailRegisterPostModel.Email}");
            }

            //判断邮箱是否注册
            var operateResult = await this.RegisterDAL.IsEmailRegisteredAsync(emailRegisterPostModel.Email);
            if (operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>($"邮箱{emailRegisterPostModel.Email}已注册，请直接使用该账号登录。{Environment.NewLine}如果忘记密码，请使用密码找回功能！");
            }

            //邮箱地址哈希值作为会话主键
            string emailHashCode = this.CryptographyBLL.GetMD5HashCode(emailRegisterPostModel.Email);

            EmailRegisterSessionModel registerSessionModel = new EmailRegisterSessionModel();

            //查询是否已经存在会话
            var getRegisterSessionResult = await this.RegisterDAL.GetEmailSessionAsync(emailHashCode);

            //如果已经创建了注册会话
            if (getRegisterSessionResult.IsSuccess)
            {
                registerSessionModel = getRegisterSessionResult.Data;

                //判断会话Id是否相同
                if (!sessionId.Equals(registerSessionModel.SessionId))
                {
                    return OperateResult.CreateFailResult<AESEncryptModel>($"邮箱{emailRegisterPostModel.Email}正在注册，请勿重复发起！{Environment.NewLine}或者等待2分钟后重新注册！");
                }
                else
                {
                    //把Redis注册会话主键移除 然后重新创建新的会话
                    var removeRegisterSessionResult = await RegisterDAL.RemoveSessionAsync(emailHashCode);
                    if (!removeRegisterSessionResult.IsSuccess)
                    {
                        return OperateResult.CreateFailResult<AESEncryptModel>(removeRegisterSessionResult);
                    }
                }
            }

            //生成验证码
            var verify = new Random(Guid.NewGuid().GetHashCode()).Next(100000, 999999);

            //生成有效期
            DateTime expireTime = DateTime.Now.AddSeconds(120);

            //重新创建注册会话
            registerSessionModel.Email = emailRegisterPostModel.Email;
            registerSessionModel.Password = emailRegisterPostModel.Password;
            registerSessionModel.EmailVerificataion = verify.ToString();
            registerSessionModel.EmailVerifyExpireTime = expireTime.ToTimeStamp();
            registerSessionModel.SessionId = sessionId;

            //将注册会话保存到Redis数据库 只有一个可以成功保存 
            var createEmailSessionResult = await this.RegisterDAL.CreateEmailSessionAsync(emailHashCode, registerSessionModel, expireTime);
            if (!createEmailSessionResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(createEmailSessionResult);
            }

            string emailRegisterSessionId = createEmailSessionResult.Data;

            //这里是通过邮箱服务发送验证码 （也可以剥离出来使用消息队列）
            EmailRegisterVerifyModel emailRegisterVerifyModel = new EmailRegisterVerifyModel
            {
                Email = emailRegisterPostModel.Email,
                Verify = $"{verify}",
            };

            //通过邮箱服务发送验证码
            operateResult = await this.EmailService.SendVerifyAsync(emailRegisterVerifyModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }


            //如果邮件发送成功 则生成新的Token给到用户
            //并且这个新的凭证有效期是2分钟
            //只有这个Token的用户才能访问接口EmailVerifyValid
            var claimList = new List<Claim>
            {
                new Claim(ClaimTypes.Sid, sessionId),
                new Claim(ClaimTypes.Role,"EmailVerifyValid")
            };

            //通过JWT 生成Token  待处理
            var generateJWTTokenResult = this.GeneralBLL.GenerateJWTToken(claimList, audiences);
            if (!generateJWTTokenResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(generateJWTTokenResult);
            }

            string token = generateJWTTokenResult.Data;


            //这里需要重新生成一个会话Id
            var verifyResultModel = new RegisterVerifyModel()
            {
                ExpireTime = expireTime.ToTimeStamp(),
                RegisterSessionId = emailRegisterSessionId,
                Token = token,
            };

            //AES对称加密
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(verifyResultModel, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }

        /// <summary>
        /// 邮箱验证码验证
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> EmailVerifyValidAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            //获取解密数据
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RegisterVerifyValidModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<EmailRegisterSessionModel>(getAESDecryptResult);
            }
            var registerVerifyValidModel = getAESDecryptResult.Data;


            //获取注册会话
            var getRegisterSessionResult = await this.RegisterDAL.GetEmailSessionAsync(registerVerifyValidModel.RegisterSessionId);
            if (!getRegisterSessionResult.IsSuccess)
            {
                return getRegisterSessionResult;
            }
            var registerSessionModel = getRegisterSessionResult.Data;

            //判断会话否相同
            if (!sessionId.Equals(registerSessionModel.SessionId))
            {
                return OperateResult.CreateFailResult<PhoneRegisterSessionModel>($"请勿恶意发起注册！");
            }

            //验证验证码是否一致
            if (!registerSessionModel.EmailVerificataion.Equals(registerVerifyValidModel.Verify))
            {
                //默认返回失败
                return OperateResult.CreateFailResult<EmailRegisterSessionModel>("验证码错误！");
            }

            //如果验证成功 就进入注册账号的逻辑
            var registerAccountResult = await this.RegisterDAL.EmailRegisterAccountAsync(registerSessionModel, registerVerifyValidModel.RegisterSessionId);
            if (!registerAccountResult.IsSuccess)
            {
                return registerAccountResult;
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 获取注册短信验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetSMSVerifyAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            ////进行数据解密
            //var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<SMSRegisterPostModel>(aesEncryptModel, sessionId);
            //if (!getAESDecryptResult.IsSuccess)
            //{
            //    return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            //}
            //var smsRegisterPostModel = getAESDecryptResult.Data;

            ////创建验证码
            //var verify = new Random(Guid.NewGuid().GetHashCode()).Next(100000, 999999);
            ////创建有效期
            //DateTime expireTime = DateTime.Now.AddSeconds(120);

            ////发送注册短信验证码
            //var sendVerifyResult = await this.SMSBLL.SendRegisterVerifyAsync(smsRegisterPostModel.CountryCode, smsRegisterPostModel.Phone, verify);
            //if (!sendVerifyResult.IsSuccess)
            //{
            //    return OperateResult.CreateFailResult<AESEncryptModel>(sendVerifyResult);
            //}

            ////更新注册会话
            //var updateRegisterSessionResult = await this.RegisterDAL.UpdateSessionAsync(smsRegisterPostModel.Token, verify, expireTime);
            //if (!updateRegisterSessionResult.IsSuccess)
            //{
            //    return OperateResult.CreateFailResult<AESEncryptModel>(updateRegisterSessionResult);
            //}

            ////返回客户端验证码相关数据
            //var verifyResultModel = new RegisterVerifyModel()
            //{
            //    ExpireTime = expireTime.ToTimeStamp(),
            //    Token = smsRegisterPostModel.Token
            //};

            ////返回加密数据
            //var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(verifyResultModel, sessionId);
            //if (!getAESEncryptResult.IsSuccess)
            //{
            //    return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            //}
            //return getAESEncryptResult;

            return OperateResult.CreateSuccessResult<AESEncryptModel>(null);
        }

        /// <summary>
        /// 短信验证码验证
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> SMSVerifyValidAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            ////获取解密数据
            //var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<RegisterVerifyValidModel>(aesEncryptModel, sessionId);
            //if (!getAESDecryptResult.IsSuccess)
            //{
            //    return OperateResult.CreateFailResult<EmailRegisterSessionModel>(getAESDecryptResult);
            //}
            //var registerVerifyValidModel = getAESDecryptResult.Data;

            ////校验验证码获取会话数据
            //var verifyValidResult = await VerifyValidAsync(registerVerifyValidModel, sessionId, VerifyValidType.SMSValidType);
            //if (!verifyValidResult.IsSuccess)
            //{
            //    return verifyValidResult;
            //}
            //var registerSessionModel = verifyValidResult.Data;

            ////如果验证成功 就进入注册账号的逻辑
            //var registerAccountResult = await this.RegisterDAL.RegisterAccountAsync(registerSessionModel, registerVerifyValidModel.Token);
            //if (!registerAccountResult.IsSuccess)
            //{
            //    return registerAccountResult;
            //}

            return OperateResult.CreateSuccessResult();
        }

    }
}
