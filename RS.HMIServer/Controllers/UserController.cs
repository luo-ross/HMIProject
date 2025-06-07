
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using RS.HMIServer.IBLL;

namespace RS.HMIServer.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    [Authorize]
    public class UserController : BaseController
    {
        /// <summary>
        /// 用户服务接口
        /// </summary>
        private readonly IUserBLL UserBLL;

        private readonly ILogBLL LogBLL;
        public UserController(IUserBLL userBLL, ILogBLL logBLL)
        {
            UserBLL = userBLL;
            LogBLL = logBLL;
        }

        /// <summary>
        /// 获取用户接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult<AESEncryptModel>> GetUser(AESEncryptModel aesEncryptModel)
        {
            return await UserBLL.GetUsersAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 更新用户是否禁用
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateIsDisable(AESEncryptModel aesEncryptModel)
        {
            return await this.UserBLL.UpdateIsDisableAsync(aesEncryptModel, SessionId);
        }

        /// <summary>
        /// 更新邮箱
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateEmail(AESEncryptModel aesEncryptModel)
        {
            return await this.UserBLL.UpdateEmailAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 更新昵称
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateNickName(AESEncryptModel aesEncryptModel)
        {
            return await this.UserBLL.UpdateNickNameAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult<AESEncryptModel>> DeleteUser(AESEncryptModel aesEncryptModel)
        {
            return await this.UserBLL.DeleteUserAsync(aesEncryptModel, SessionId);
        }

    }
}
