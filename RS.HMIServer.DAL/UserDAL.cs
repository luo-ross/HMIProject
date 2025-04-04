
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.Entity;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.IDAL;
using RS.Commons;
using RS.Commons.Attributs;
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
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult<List<UserModel>>> GetUsersAsync()
        {
            var dataList = await RSAppDb.User.Select(t => new UserModel()
            {
                NickName = t.NickName,
                UserPic = t.UserPic,
            }).ToListAsync();
            return OperateResult.CreateSuccessResult(dataList);
        }
    }
}
