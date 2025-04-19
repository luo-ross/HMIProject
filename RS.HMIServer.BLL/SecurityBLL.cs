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

        private readonly IConfiguration Configuration;


        public SecurityBLL(ISecurityDAL securityDAL, IEmailBLL emailBLL, ICryptographyBLL cryptographyBLL, IGeneralBLL generalBLL, IConfiguration configuration)
        {
            this.EmailBLL = emailBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.SecurityDAL = securityDAL;
            this.GeneralBLL = generalBLL;
            this.Configuration = configuration;
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
            string  email= emailPasswordConfirmModel.Email;
            //然后获取用户信息
            var userEntity = this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == email);
            if (userEntity == null)
            {
                return OperateResult.CreateFailResult("用户不存在！");
            }

            return await this.SecurityDAL.EmailPasswordResetConfirmAsync(emailPasswordConfirmModel);
        }



    }
}
