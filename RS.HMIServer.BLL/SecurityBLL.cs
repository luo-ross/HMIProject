using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Helper;
using RS.HMIServer.Entity;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Models;
using RTools_NTS.Util;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 安全服务
    /// </summary>
    [ServiceInjectConfig(typeof(ISecurityBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class SecurityBLL : ISecurityBLL
    {

        /// <summary>
        /// 邮箱服务
        /// </summary>
        private readonly IEmailBLL EmailBLL;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;

        /// <summary>
        /// 注册数据仓储接口
        /// </summary>
        private readonly ISecurityDAL SecurityDAL;

        /// <summary>
        /// 通用服务
        /// </summary>
        private readonly IGeneralBLL GeneralBLL;

        /// <summary>
        /// 配置接口
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// 配置接口
        /// </summary>
        private readonly IOpenCVBLL OpenCVBLL;

        public SecurityBLL(ISecurityDAL securityDAL,
            IEmailBLL emailBLL,
            ICryptographyBLL cryptographyBLL,
            IGeneralBLL generalBLL,
            IConfiguration configuration,
            IOpenCVBLL openCVBLL
            )
        {
            this.EmailBLL = emailBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.SecurityDAL = securityDAL;
            this.GeneralBLL = generalBLL;
            this.Configuration = configuration;
            this.OpenCVBLL = openCVBLL;
        }


        public async Task<OperateResult> PasswordResetEmailSendAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<EmailSecurityModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var emailSecurityModel = getAESDecryptResult.Data;

            if (emailSecurityModel == null)
            {
                throw new ArgumentNullException(nameof(emailSecurityModel));
            }
            string webHost = this.Configuration["WebHost"];
            if (string.IsNullOrEmpty(webHost) || string.IsNullOrWhiteSpace(webHost))
            {
                throw new ArgumentNullException(nameof(webHost));
            }

            if (string.IsNullOrEmpty(emailSecurityModel.Email) || string.IsNullOrWhiteSpace(emailSecurityModel.Email))
            {
                return OperateResult.CreateFailResult("邮件地址不能为空");
            }

            //验证邮箱格式是否正确
            if (!emailSecurityModel.Email.IsEmail())
            {
                return OperateResult.CreateFailResult("邮箱格式不正确！");
            }

            //验证用户是否存在
            var userEntity = this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == emailSecurityModel.Email);
            if (userEntity == null)
            {
                return OperateResult.CreateFailResult("用户不存在！");
            }

            //创建修改密码会话
            string passwordResetToken = Guid.NewGuid().ToString();  //创建会话主键
            var operateResult = await this.SecurityDAL.CreateEmailPasswordResetSessionAsync(passwordResetToken, new EmailSecurityModel()
            {
                Email = emailSecurityModel.Email,
            });

            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            emailSecurityModel.ResetLink = $"{webHost}/EmailPasswordReset?Email={Uri.EscapeDataString(emailSecurityModel.Email)}&Token={passwordResetToken}";

            operateResult = await this.EmailBLL.SendPassResetAsync(emailSecurityModel);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            //发送给用户后，等待用户点击链接进行密码修改

            return operateResult;

        }

        public async Task<OperateResult> EmailPasswordResetConfirmAsync(AESEncryptModel aesEncryptModel, string sessionId, string audiences)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<EmailPasswordConfirmModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var emailPasswordConfirmModel = getAESDecryptResult.Data;

            //查看会话是否存在
            var validResult = await this.SecurityDAL.EmailPasswordResetSessionValidAsync(emailPasswordConfirmModel.Email, emailPasswordConfirmModel.Token);
            if (!validResult.IsSuccess)
            {
                return validResult;
            }
            var emailSecurityModel = validResult.Data;

            //判断用户是否相同
            if (!emailSecurityModel.Email.Equals(emailPasswordConfirmModel.Email))
            {
                return OperateResult.CreateFailResult("密码重置会话不存在");
            }
            //获取邮箱
            string email = emailPasswordConfirmModel.Email;
            //然后获取用户信息
            var userEntity = this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == email);
            if (userEntity == null)
            {
                return OperateResult.CreateFailResult("用户不存在！");
            }

            return await this.SecurityDAL.EmailPasswordResetConfirmAsync(emailPasswordConfirmModel);
        }

        /// <summary>
        /// 获取验证码信息
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <param name="audiences">客户端类型</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetImgVerifyModelAsync(string sessionId, string audiences)
        {
            //在这个创建验证码数据前简单验证一下用户是否有权限啥的
            OperateResult operateResult =await this.SecurityDAL.IsCanCreateImgVerifySessionAsync(sessionId);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            //首先获取验证码图片数据
            var getVerifyImgInitModel = await this.OpenCVBLL.GetVerifyImgInitModelAsync();
            if (!getVerifyImgInitModel.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getVerifyImgInitModel);
            }
            var verifyImgInitModel = getVerifyImgInitModel.Data;

            //获取到图片信息后 将这些验证数据放在Redis缓存里
            OperateResult<string> createVerifySessionModelResult = await this.SecurityDAL.CreateVerifySessionModelAsync(verifyImgInitModel, sessionId);
            if (!createVerifySessionModelResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(createVerifySessionModelResult);
            }

            string verifyId = createVerifySessionModelResult.Data;

            //获取唯一Id
            verifyImgInitModel.VerifyId = verifyId;

            //这里还需要把验证码信息存起来，用于后面的用户进行验证使用
            ImgVerifyModel imgVerifyModel = new ImgVerifyModel()
            {
                ImgBtnPositionX = verifyImgInitModel.ImgBtnPositionX,
                ImgBtnPositionY = verifyImgInitModel.ImgBtnPositionY,
                IconHeight = verifyImgInitModel.IconHeight,
                IconWidth = verifyImgInitModel.IconWidth,
                ImgBuffer = verifyImgInitModel.ImgBuffer,
                ImgHeight = verifyImgInitModel.ImgHeight,
                ImgWidth = verifyImgInitModel.ImgWidth,
                VerifyId = verifyImgInitModel.VerifyId,
            };

            //AES对称加密
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(imgVerifyModel, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }
    }
}
