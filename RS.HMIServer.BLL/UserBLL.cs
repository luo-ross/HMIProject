using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.Entity;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using TencentCloud.Ciam.V20220331.Models;

namespace RS.HMIServer.BLL
{
    [ServiceInjectConfig(typeof(IUserBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class UserBLL : IUserBLL
    {
        /// <summary>
        /// 用户数据仓储接口
        /// </summary>
        private readonly IUserDAL UserDAL;

        /// <summary>
        /// 邮箱服务接口
        /// </summary>
        private readonly IEmailBLL EmailBLL;

        /// <summary>
        /// 通用服务接口
        /// </summary>
        private readonly IGeneralBLL GeneralBLL;

        /// <summary>
        /// 加解密服务接口
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;
        private readonly ISMSBLL SMSBLL;
        public UserBLL(IUserDAL userDAL, IEmailBLL emailBLL, IGeneralBLL generalBLL, ICryptographyBLL cryptographyBLL, ISMSBLL sMSService)
        {
            this.UserDAL = userDAL;
            this.EmailBLL = emailBLL;
            this.GeneralBLL = generalBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.SMSBLL = sMSService;
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
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(getUsersResult.Data, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }

      
    }
}
