using Microsoft.Extensions.DependencyInjection;
using RS.WebApp.Entity;
using RS.WebApp.IBLL;
using RS.WebApp.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using TencentCloud.Ciam.V20220331.Models;

namespace RS.WebApp.BLL
{
    [ServiceInjectConfig(typeof(IUserService), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class UserService : IUserService
    {
        /// <summary>
        /// 用户数据仓储接口
        /// </summary>
        private readonly IUserDAL UserDAL;

        /// <summary>
        /// 邮箱服务接口
        /// </summary>
        private readonly IEmailService EmailService;

        /// <summary>
        /// 通用服务接口
        /// </summary>
        private readonly IGeneralService GeneralService;

        /// <summary>
        /// 加解密服务接口
        /// </summary>
        private readonly ICryptographyService CryptographyService;
        private readonly ISMSService SMSService;
        public UserService(IUserDAL userDAL, IEmailService emailService, IGeneralService generalService, ICryptographyService cryptographyService, ISMSService sMSService)
        {
            this.UserDAL = userDAL;
            this.EmailService = emailService;
            this.GeneralService = generalService;
            this.CryptographyService = cryptographyService;
            this.SMSService = sMSService;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetUsersAsync(string sessionId)
        {
            //这是获取用户数据
            var getUsersResult = await UserDAL.GetUsersAsync();
            if (!getUsersResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getUsersResult);
            }

            //获取加密数据
            var getAESEncryptResult = await this.GeneralService.GetAESEncryptAsync(getUsersResult.Data, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }

        /// <summary>
        /// 验证用户登录
        /// </summary>
        /// <param name="aesEncryptModel">AES对称加密</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> ValidLoginAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralService.GetAESDecryptAsync<LoginValidModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var loginValidModel = getAESDecryptResult.Data;

            //查询用户信息
            var userEntityResult = await this.UserDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == loginValidModel.UserName);
            if (!userEntityResult.IsSuccess)
            {
                return OperateResult.CreateFailResult("用户名或者密码错误！");
            }
            var userEntity = userEntityResult.Data;

            //获取登录信息
            var logOnEntityResult = await this.UserDAL.FirstOrDefaultAsync<LogOnEntity>(t => t.UserId == userEntity.Id);
            if (!userEntityResult.IsSuccess)
            {
                return OperateResult.CreateFailResult("用户名或者密码错误！");
            }
            var logOnEntity = logOnEntityResult.Data;

            //是否禁用
            if (!logOnEntity.IsEnable)
            {
                return OperateResult.CreateFailResult("当前用户已禁用！");
            }

            //生成新密码
            var newPassword = this.CryptographyService.GetSHA256HashCode($"{loginValidModel.Password}-{logOnEntity.Salt}");
            
            //比较密码是否相同
            if (!newPassword.Equals(logOnEntity.Password))
            {
                return OperateResult.CreateFailResult("用户名或者密码错误！");
            }

            return OperateResult.CreateSuccessResult();
        }
    }
}
