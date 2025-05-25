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
        /// 删除用户
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> DeleteUserAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<UserModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var userModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult<UserModel> operateResult = await this.UserDAL.DeleteUserAsync(userModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            //获取加密数据
            var getAESEncryptResult = await this.GeneralBLL.GetAESEncryptAsync(operateResult.Data, sessionId);
            if (!getAESEncryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESEncryptResult);
            }

            return getAESEncryptResult;
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<AESEncryptModel>> GetUsersAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<Pagination>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var pagination = getAESDecryptResult.Data;


            //这是获取用户数据
            var getUsersResult = await UserDAL.GetUsersAsync(pagination);
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

        /// <summary>
        /// 更新邮箱
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateEmailAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<UserModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var userModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult operateResult = await this.UserDAL.UpdateEmailAsync(userModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 更新用户是否禁用
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateIsDisableAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<UserModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var userModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult operateResult = await this.UserDAL.UpdateIsDisableAsync(userModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 更新昵称
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateNickNameAsync(AESEncryptModel aesEncryptModel, string sessionId)
        {
            //进行数据解密
            var getAESDecryptResult = await this.GeneralBLL.GetAESDecryptAsync<UserModel>(aesEncryptModel, sessionId);
            if (!getAESDecryptResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(getAESDecryptResult);
            }
            var userModel = getAESDecryptResult.Data;


            //将数据更新写入到数据库
            OperateResult operateResult = await this.UserDAL.UpdateNickNameAsync(userModel);
            if (!operateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult<AESEncryptModel>(operateResult);
            }

            return OperateResult.CreateSuccessResult();
        }
    }
}
