using CommunityToolkit.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.Entity;
using RS.HMIServer.IDAL;
using RS.Models;
using StackExchange.Redis;

namespace RS.HMIServer.DAL
{

    /// <summary>
    /// 用户数据逻辑层
    /// </summary>
    [ServiceInjectConfig(typeof(IUserDAL), ServiceLifetime.Transient)]
    internal class UserDAL : Repository, IUserDAL
    {
        /// <summary>
        /// 鉴权Redis数据库
        /// </summary>
        private readonly IDatabase AuthRedis;
        public UserDAL(RSAppDbContext rsAppDb, RedisDbContext redisDbContext)
        {
            this.RSAppDb = rsAppDb;
            this.AuthRedis = redisDbContext.GetAuthRedis();
        }

        /// <summary>
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult<UserModel>> DeleteUserAsync(UserModel userModel)
        {
            //删除用户是一个比较复杂的逻辑过程 这里简单实现
            if (userModel.Id == null)
            {
                return OperateResult.CreateFailResult<UserModel>("用户主键不能为空");
            }

            userModel.IsDelete = true;
            userModel.DeleteTime = DateTime.Now;

            //这里待处理
            //userModel.DeleteId = null;
            //userModel.DeleteBy = null;

            var effectRows = await this.RSAppDb.User
                  .Where(t => t.Id == userModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.IsDelete, userModel.IsDelete)
                  .SetProperty(b => b.DeleteTime, userModel.DeleteTime)
                  .SetProperty(b => b.DeleteId, userModel.DeleteId)
                  );

            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult<UserModel>("更新失败");
            }

            return OperateResult.CreateSuccessResult(userModel);
        }

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult<PageDataModel<UserModel>>> GetUsersAsync(Pagination pagination)
        {
            OperateResult<PageDataModel<UserModel>> getPaginationListReuslt =
              await this.GetPaginationListWithCreateUpateDeleteByAsync<UserEntity, UserModel>(pagination);
            if (!getPaginationListReuslt.IsSuccess)
            {
                return getPaginationListReuslt;
            }

            return getPaginationListReuslt;
        }

        /// <summary>
        /// 更新用户邮箱
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateEmailAsync(UserModel userModel)
        {

            //更新邮箱是一个比较复杂的逻辑过程 这里简单实现
            if (userModel.Id == null)
            {
                return OperateResult.CreateFailResult("用户主键不能为空");
            }

            var effectRows = await this.RSAppDb.User
                  .Where(t => t.Id == userModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.Email, userModel.Email));
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 更新用户是否禁用
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateIsDisableAsync(UserModel userModel)
        {
            if (userModel.Id == null)
            {
                return OperateResult.CreateFailResult("用户主键不能为空");
            }

            var effectRows = await this.RSAppDb.User
                  .Where(t => t.Id == userModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.IsDisabled, userModel.IsDisabled));
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 更新用户昵称
        /// </summary>
        /// <param name="userModel"></param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateNickNameAsync(UserModel userModel)
        {
            //更新邮箱是一个比较复杂的逻辑过程 这里简单实现
            if (userModel.Id == null)
            {
                return OperateResult.CreateFailResult("用户主键不能为空");
            }

            //字符串安全处理 防止用户写脚本
            userModel.NickName= userModel.NickName?.FixHtml();

            if (string.IsNullOrEmpty(userModel.NickName)
                ||string.IsNullOrWhiteSpace(userModel.NickName))
            {
                return OperateResult.CreateFailResult("用户昵称不能为空");
            }

            var effectRows = await this.RSAppDb.User
                  .Where(t => t.Id == userModel.Id)
                  .ExecuteUpdateAsync(setters =>
                  setters.SetProperty(b => b.NickName, userModel.NickName));
            if (effectRows == 0)
            {
                return OperateResult.CreateFailResult("更新失败");
            }

            return OperateResult.CreateSuccessResult();
        }
    }
}
