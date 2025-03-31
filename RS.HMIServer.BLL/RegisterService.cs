using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Utilities;
using RS.HMIServer.Entity;
using RS.HMIServer.DAL;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.Models;
using System;
using System.Data.SqlTypes;
using System.Net.Mail;
using RS.Commons.Helper;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 账号注册服务
    /// </summary>
    [ServiceInjectConfig(typeof(IRegisterService), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class RegisterService : IRegisterService
    {
        /// <summary>
        /// 注册数据仓储接口
        /// </summary>
        private readonly IRegisterDAL RegisterDAL;

        /// <summary>
        /// 邮箱服务
        /// </summary>
        private readonly IEmailService EmailService;

        /// <summary>
        /// 通用服务
        /// </summary>
        private readonly IGeneralService GeneralService;

        /// <summary>
        /// 短信服务
        /// </summary>
        private readonly ISMSService SMSService;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyService CryptographyService;



        /// <summary>
        /// 注册服务构造函数
        /// </summary>
        /// <param name="registerDAL">注册数据仓储</param>
        /// <param name="emailService">邮箱服务</param>
        /// <param name="generalService">通用服务</param>
        /// <param name="sMSService">短信服务</param>
        public RegisterService(IRegisterDAL registerDAL, IEmailService emailService, IGeneralService generalService, ISMSService sMSService, ICryptographyService cryptographyService)
        {
            this.RegisterDAL = registerDAL;
            this.EmailService = emailService;
            this.GeneralService = generalService;
            this.SMSService = sMSService;
            this.CryptographyService = cryptographyService;
        }


        /// <summary>
        /// 往客户端发送邮箱验证码
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetEmailVerificationAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralService.GetAESDecryptAsync<EmailRegisterPostModel>(aesEncryptModel, sessionId);
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
            string token = this.CryptographyService.GetMD5HashCode(emailRegisterPostModel.Email);

            RegisterSessionModel registerSessionModel = new RegisterSessionModel();

            //查询是否已经存在会话
            var getRegisterSessionResult = await this.RegisterDAL.GetSessionAsync(token);
            //如果存在
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
                    var removeRegisterSessionResult = await RegisterDAL.RemoveSessionAsync(token);
                    if (!removeRegisterSessionResult.IsSuccess)
                    {
                        return OperateResult.CreateFailResult<AESEncryptModel>(removeRegisterSessionResult);
                    }
                }
            }

            //生成验证码
            var verification = new Random(Guid.NewGuid().GetHashCode()).Next(100000, 999999);

            //生成有效期
            DateTime expireTime = DateTime.Now.AddSeconds(120);

            //重新创建注册会话
            registerSessionModel.Email = emailRegisterPostModel.Email;
            registerSessionModel.Password = emailRegisterPostModel.Password;
            registerSessionModel.EmailVerificataion = verification.ToString();
            registerSessionModel.EmailVerificationExpireTime = expireTime.ToTimeStamp();
            registerSessionModel.SessionId = sessionId;

            //将注册会话保存到Redis数据库 只有一个可以成功保存 
            operateResult = await this.RegisterDAL.CreateSessionAsync(token, registerSessionModel, expireTime);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }


            //这里是通过邮箱服务发送验证码 （也可以剥离出来使用消息队列）
            EmailRegisterVerificationModel emailRegisterVerificationModel = new EmailRegisterVerificationModel
            {
                Email = emailRegisterPostModel.Email,
                Verification = $"{verification}",
            };
            operateResult = await this.EmailService.SendVerificationAsync(emailRegisterVerificationModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            //返回客户端验证码相关数据
            var verificationResultModel = new RegisterVerificationModel()
            {
                ExpireTime = expireTime.ToTimeStamp(),
                Token = token
            };

            //AES对称加密
            var getAESEncryptResult = await this.GeneralService.GetAESEncryptAsync(verificationResultModel, sessionId);
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
        public async Task<OperateResult> EmailVerificationValidAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //获取解密数据
            var getAESDecryptResult = await this.GeneralService.GetAESDecryptAsync<RegisterVerificationValidModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<RegisterSessionModel>(getAESDecryptResult);
            }
            var registerVerificationValidModel = getAESDecryptResult.Data;

            //校验验证码获取会话数据
            var verificationValidResult = await VerificationValidAsync(registerVerificationValidModel, sessionId, VerificationValidType.EmailValiType);
            if (!verificationValidResult.IsSuccess)
            {
                return verificationValidResult;
            }
            var registerSessionModel = verificationValidResult.Data;

            //如果验证成功 就进入注册账号的逻辑
            var registerAccountResult = await this.RegisterDAL.RegisterAccountAsync(registerSessionModel, registerVerificationValidModel.Token);
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
        public async Task<OperateResult<AESEncryptModel>> GetSMSVerificationAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralService.GetAESDecryptAsync<SMSRegisterPostModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var smsRegisterPostModel = getAESDecryptResult.Data;

            //创建验证码
            var verification = new Random(Guid.NewGuid().GetHashCode()).Next(100000, 999999);
            //创建有效期
            DateTime expireTime = DateTime.Now.AddSeconds(120);

            //发送注册短信验证码
            var sendVerificationResult = await this.SMSService.SendRegisterVerificationAsync(smsRegisterPostModel.CountryCode, smsRegisterPostModel.Phone, verification);
            if (!sendVerificationResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(sendVerificationResult);
            }

            //更新注册会话
            var updateRegisterSessionResult = await this.RegisterDAL.UpdateSessionAsync(smsRegisterPostModel.Token, verification, expireTime);
            if (!updateRegisterSessionResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(updateRegisterSessionResult);
            }

            //返回客户端验证码相关数据
            var verificationResultModel = new RegisterVerificationModel()
            {
                ExpireTime = expireTime.ToTimeStamp(),
                Token = smsRegisterPostModel.Token
            };

            //返回加密数据
            var getAESEncryptResult = await this.GeneralService.GetAESEncryptAsync(verificationResultModel, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }
            return getAESEncryptResult;
        }

        /// <summary>
        /// 短信验证码验证
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> SMSVerificationValidAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //获取解密数据
            var getAESDecryptResult = await this.GeneralService.GetAESDecryptAsync<RegisterVerificationValidModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<RegisterSessionModel>(getAESDecryptResult);
            }
            var registerVerificationValidModel = getAESDecryptResult.Data;

            //校验验证码获取会话数据
            var verificationValidResult = await VerificationValidAsync(registerVerificationValidModel, sessionId, VerificationValidType.SMSValidType);
            if (!verificationValidResult.IsSuccess)
            {
                return verificationValidResult;
            }
            var registerSessionModel = verificationValidResult.Data;

            //如果验证成功 就进入注册账号的逻辑
            var registerAccountResult = await this.RegisterDAL.RegisterAccountAsync(registerSessionModel, registerVerificationValidModel.Token);
            if (!registerAccountResult.IsSuccess)
            {
                return registerAccountResult;
            }

            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 短信验证码验证
        /// </summary>
        /// <param name="aesEncryptModel">AES加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        private async Task<OperateResult<RegisterSessionModel>> VerificationValidAsync(RegisterVerificationValidModel registerVerificationValidModel, string sessionId, VerificationValidType verificationValidType)
        {
            //获取注册会话
            var getRegisterSessionResult = await this.RegisterDAL.GetSessionAsync(registerVerificationValidModel.Token);
            if (!getRegisterSessionResult.IsSuccess)
            {
                return getRegisterSessionResult;
            }
            var registerSessionModel = getRegisterSessionResult.Data;

            //判断会话否相同
            if (!sessionId.Equals(registerSessionModel.SessionId))
            {
                return OperateResult.CreateFailResult<RegisterSessionModel>($"请勿恶意发起注册！");
            }

            //验证逻辑
            switch (verificationValidType)
            {
                case VerificationValidType.EmailValiType:
                    //验证验证码是否一致
                    if (!registerSessionModel.EmailVerificataion.Equals(registerVerificationValidModel.Verification))
                    {
                        //默认返回失败
                        return OperateResult.CreateFailResult<RegisterSessionModel>("验证码错误！");
                    }
                    break;
                case VerificationValidType.SMSValidType:
                    //验证验证码是否一致
                    if (!registerSessionModel.PhoneVerificataion.Equals(registerVerificationValidModel.Verification))
                    {
                        return OperateResult.CreateFailResult<RegisterSessionModel>("验证码错误！");
                    }
                    break;
            }

            return OperateResult.CreateSuccessResult(registerSessionModel);

        }

    }
}
