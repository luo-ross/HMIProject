using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Models;
using StackExchange.Redis;

namespace RS.HMIServer.DAL
{
    /// <summary>
    /// 通用数据逻辑层
    /// </summary>
    [ServiceInjectConfig(typeof(IGeneralDAL), ServiceLifetime.Transient)]
    internal class GeneralDAL : Repository, IGeneralDAL
    {
        private readonly IDatabase SessionRedis;
        private readonly ICryptographyService CryptographyService;
        public GeneralDAL(RSAppDbContext rsAppDb, RedisDbContext redisDbContext, ICryptographyService cryptographyService)
        {
            this.RSAppDb = rsAppDb;
            this.SessionRedis = redisDbContext.GetSessionRedis();
            this.CryptographyService = cryptographyService;
        }

        /// <summary>
        /// 保存会话
        /// </summary>
        /// <param name="sessionModel"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<OperateResult> SaveSessionModelAsync(SessionModel sessionModel, string sessionId)
        {
            //把会话数据存储到Redis
            var stringSetResult = await SessionRedis.StringSetAsync(sessionId, sessionModel.ToJson(), TimeSpan.FromMinutes(15));
            if (!stringSetResult)
            {
                return OperateResult.CreateSuccessResult("存储会话数据失败");
            }
            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 获取会话
        /// </summary>
        /// <param name="sessionModelKey"></param>
        /// <returns></returns>
        public async Task<OperateResult<SessionModel>> GetSessionModelAsync(string sessionModelKey)
        {
            if (string.IsNullOrEmpty(sessionModelKey) || string.IsNullOrWhiteSpace(sessionModelKey))
            {
                return OperateResult.CreateFailResult<SessionModel>("客户端和服务端未连接！");
            }

            //从Redis数据库获取会话数据
            string jsonSring = await SessionRedis.StringGetAsync(sessionModelKey);
            if (string.IsNullOrEmpty(jsonSring))
            {
                return OperateResult.CreateFailResult<SessionModel>("客户端和服务端未连接！");
            }

            //反序列话获取会话实体
            var sessionModel = jsonSring.ToObject<SessionModel>();

            //对数据进行解密
            sessionModel.AesKey = CryptographyService.UnprotectData(sessionModel.AesKey);
            sessionModel.AppId = CryptographyService.UnprotectData(sessionModel.AppId);
            return OperateResult.CreateSuccessResult(sessionModel);
        }

      

    }
}
