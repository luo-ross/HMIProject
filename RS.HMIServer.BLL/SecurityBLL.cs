using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Helper;
using RS.HMIServer.Entity;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Models;

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

        public SecurityBLL(ISecurityDAL securityDAL, IEmailBLL emailBLL, ICryptographyBLL cryptographyBLL, IGeneralBLL generalBLL)
        {
            this.EmailBLL = emailBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.SecurityDAL = securityDAL;
            this.GeneralBLL = generalBLL;
        }


        public async Task<OperateResult> PasswordResetAsync(AESEncryptModel aesEncryptModel, string hostWithScheme, string sessionId, string audiences)
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
            if (string.IsNullOrEmpty(hostWithScheme) || string.IsNullOrWhiteSpace(hostWithScheme))
            {
                throw new ArgumentNullException(nameof(hostWithScheme));
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
            var operateResult = await this.SecurityDAL.CreatePasswordResetSessionAsync(passwordResetToken, new EmailSecurityModel()
            {
                Email = emailSecurityModel.Email,
            });
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }
           
            emailSecurityModel.ResetLink = $"{hostWithScheme}/password/edit?email={Uri.EscapeDataString(emailSecurityModel.Email)}&token={passwordResetToken}";

            operateResult = await this.EmailBLL.SendPassResetAsync(emailSecurityModel);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            //发送给用户后，等待用户点击链接进行密码修改

            return operateResult;

        }

        
       


        /// <summary>
        /// 密码重置会话验证
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> PasswordResetSessionValidAsync(string email, string token)
        {
            return await this.SecurityDAL.PasswordResetSessionValidAsync(email, token);
        }
    }
}
