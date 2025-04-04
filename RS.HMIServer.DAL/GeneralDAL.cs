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
        private readonly ICryptographyBLL CryptographyBLL;
        /// <summary>
        /// 客户端Id Redis数据库
        /// </summary>
        private readonly IDatabase ClientIdRedis;
        /// <summary>
        /// 客户端IP Redis数据库
        /// </summary>
        private readonly IDatabase ClientIPRedis;
        public GeneralDAL(RSAppDbContext rsAppDb, RedisDbContext redisDbContext, ICryptographyBLL cryptographyBLL)
        {
            this.RSAppDb = rsAppDb;
            this.SessionRedis = redisDbContext.GetSessionRedis();
            this.CryptographyBLL = cryptographyBLL;
            this.ClientIdRedis = redisDbContext.GetClientIdRedis();
            this.ClientIPRedis = redisDbContext.GetClientIPRedis();
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
            sessionModel.AesKey = CryptographyBLL.UnprotectData(sessionModel.AesKey);
            sessionModel.AppId = CryptographyBLL.UnprotectData(sessionModel.AppId);
            return OperateResult.CreateSuccessResult(sessionModel);
        }




        /// <summary>
        /// 保存客户端连接
        /// </summary>
        /// <param name="clientId">客户端id</param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        public async Task<OperateResult<string>> SaveClientIdAsync(LoginClientModel loginClientModel)
        {
            //MD5 hash值32位
            if (string.IsNullOrEmpty(loginClientModel.ClientIPHash)
                || string.IsNullOrWhiteSpace(loginClientModel.ClientIPHash)
                || loginClientModel.ClientIPHash.Length != 32)
            {
                return OperateResult.CreateFailResult<string>("clientIP不能为空或者格式不正确");
            }

            //创建一个Key值
            string clientIdNew = Guid.NewGuid().ToString();


            //这里不用插叙直接插入
            await this.ClientIPRedis.StringSetAsync(loginClientModel.ClientIPHash, clientIdNew, TimeSpan.FromMinutes(15), When.NotExists);

            //在进行一次查询
            var clientIdExist = await this.ClientIPRedis.StringGetAsync(loginClientModel.ClientIPHash);
            if (!clientIdExist.HasValue)
            {
                //再尝试插入一次
                await this.ClientIPRedis.StringSetAsync(loginClientModel.ClientIPHash, clientIdNew, TimeSpan.FromMinutes(15), When.NotExists);
                //再尝试获取一次
                clientIdExist = await this.ClientIPRedis.StringGetAsync(loginClientModel.ClientIPHash);
                //如果还获取不成功
                if (!clientIdExist.HasValue)
                {
                    //如果这里还失败 只说明数据库有问题了
                    return OperateResult.CreateFailResult<string>(new OperateResult()
                    {
                        ErrorCode = 1000001,
                        Message = "Redis无法正常读写数据",
                        IsSuccess = false
                    });
                }
            }

            //刷新时间
            var result = await this.ClientIPRedis.StringSetAsync(loginClientModel.ClientIPHash, clientIdExist, TimeSpan.FromMinutes(15), When.Always);
            //这里正常它就不可能失败，如果失败说明数据库有问题
            if (!result)
            {
                return OperateResult.CreateFailResult<string>(new OperateResult()
                {
                    ErrorCode = 1000001,
                    Message = "Redis无法正常读写数据",
                    IsSuccess = false
                });
            }

            //拿到真实的ClientId
            clientIdNew = clientIdExist.ToString();

            //重新刷新ClientId 方便
            result = await this.ClientIdRedis.StringSetAsync(clientIdNew, loginClientModel.ToJson(), TimeSpan.FromMinutes(15), When.Always);
            if (!result)
            {
                return OperateResult.CreateFailResult<string>(new OperateResult()
                {
                    ErrorCode = 1000001,
                    Message = "Redis无法正常读写数据",
                    IsSuccess = false
                });
            }
            //这个时候这个ClientId百分百是存储在服务端了
            return OperateResult.CreateSuccessResult(clientIdNew);
        }


        /// <summary>
        /// 获取登录客户端信息
        /// </summary>
        /// <param name="clientId">客户端Id</param>
        /// <returns></returns>
        public async Task<OperateResult<LoginClientModel>> GetLoginClientModelAsync(string clientId)
        {
            var loginClientModelResult = this.ClientIdRedis.StringGet(clientId);
            if (!loginClientModelResult.HasValue)
            {
                return OperateResult.CreateFailResult<LoginClientModel>("未查询到客户端信息");
            }
            var loginClientModelJson = loginClientModelResult.ToString();
            var loginClientModel = loginClientModelJson.ToObject<LoginClientModel>();
            if (loginClientModel == null)
            {
                return OperateResult.CreateFailResult<LoginClientModel>("未查询到客户端信息");
            }
            return OperateResult.CreateSuccessResult(loginClientModel);
        }


        public async Task<OperateResult> IsClientIPExistAsync(LoginClientModel loginClientModel, string clientId)
        {
            //刷新时间
            var result = await this.ClientIPRedis.StringSetAsync(loginClientModel.ClientIPHash, clientId, TimeSpan.FromMinutes(15), When.Always);
            //这里正常它就不可能失败，如果失败说明数据库有问题
            if (!result)
            {
                return OperateResult.CreateFailResult<string>(new OperateResult()
                {
                    ErrorCode = 1000001,
                    Message = "Redis无法正常读写数据",
                    IsSuccess = false
                });
            }

            //刷新时间
            result = await this.ClientIdRedis.StringSetAsync(clientId, loginClientModel.ToJson(), TimeSpan.FromMinutes(15), When.Always);
            //这里正常它就不可能失败，如果失败说明数据库有问题
            if (!result)
            {
                return OperateResult.CreateFailResult<string>(new OperateResult()
                {
                    ErrorCode = 1000001,
                    Message = "Redis无法正常读写数据",
                    IsSuccess = false
                });
            }
            return OperateResult.CreateSuccessResult();
        }


    }
}
