using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.Entity;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using StackExchange.Redis;

namespace RS.HMIServer.DAL
{
    /// <summary>
    /// 注册数据逻辑层
    /// </summary>
    [ServiceInjectConfig(typeof(IRegisterDAL), ServiceLifetime.Transient)]
    internal class RegisterDAL : Repository, IRegisterDAL
    {
        /// <summary>
        /// Redis注册缓存接口
        /// </summary>
        private readonly IDatabase RegisterRedis;
        /// <summary>
        /// 密码服务接口
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;
        public RegisterDAL(RSAppDbContext rsAppDb, RedisDbContext redisDbContext, ICryptographyBLL cryptographyBLL)
        {
            this.RSAppDb = rsAppDb;
            this.RegisterRedis = redisDbContext.GetRegisterRedis();
            this.CryptographyBLL = cryptographyBLL;
        }


        /// <summary>
        /// 创建注册会话
        /// </summary>
        /// <param name="emailHashCode">注册会话邮箱哈希</param>
        /// <param name="registerSessionModel">注册会话类</param>
        /// <param name="expireTime">注册会话过期时间</param>
        /// <returns></returns>
        public async Task<OperateResult<string>> CreateEmailSessionAsync(string emailHashCode, EmailRegisterSessionModel registerSessionModel, DateTime expireTime)
        {
            //将会话提示转为字符串存储到Redis数据库
            var jsonStr = registerSessionModel.ToJson();
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.RegisterRedis.StringSetAsync(emailHashCode, jsonStr, timeSpan, When.NotExists);
            if (!result)
            {
                return new OperateResult<string>
                {
                    Message = "注册会话已存在，请不要重复发起注册"
                };
            }

            //创建邮箱注册会话主键
            string emailRegisterSessionId = Guid.NewGuid().ToString();
            //创建会话映射 只有知道这个主键的才能获取到注册信息
            result = await this.RegisterRedis.StringSetAsync(emailRegisterSessionId, emailHashCode, timeSpan, When.NotExists);
            if (!result)
            {
                //这里如果失败 说明数据库出问题了
                return new OperateResult<string>
                {
                    Message = "出错了，暂时无法进行注册"
                };
            }

            return OperateResult.CreateSuccessResult(emailRegisterSessionId);
        }

        public async Task<OperateResult<string>> GetEmailHashCodeAsync(string registerSessionId)
        {
            //根据会话Token获取会话数据
            var stringSetResult = await this.RegisterRedis.StringGetAsync(registerSessionId);
            if (!stringSetResult.HasValue)
            {
                return OperateResult.CreateFailResult<string>("未获取到注册会话");
            }

            //获取emailHashCode
            var result = stringSetResult.ToString();
            return OperateResult.CreateSuccessResult(result);
        }


        /// <summary>
        /// 通过邮箱获取注册会话
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<OperateResult<EmailRegisterSessionModel>> GetEmailRegisterSessionAsync(string emailHashCode)
        {
            //根据会话Token获取会话数据
            var stringSetResult = await this.RegisterRedis.StringGetAsync(emailHashCode);
            if (!stringSetResult.HasValue)
            {
                return OperateResult.CreateFailResult<EmailRegisterSessionModel>("未获取到注册会话");
            }

            //获取Json字符串
            var result = stringSetResult.ToString().ToObject<EmailRegisterSessionModel>();

            return OperateResult.CreateSuccessResult(result);
        }


        /// <summary>
        /// 更新注册会话
        /// </summary>
        /// <param name="token">注册会话Id</param>
        /// <param name="verify">短信验证码</param>
        /// <param name="expireTime">验证码失效时间</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateEmailSessionAsync(string token, int verify, DateTime expireTime)
        {
            //获取注册会话
            var getRegisterSessionResult = await GetEmailRegisterSessionAsync(token);
            if (!getRegisterSessionResult.IsSuccess)
            {
                return getRegisterSessionResult;
            }
            var registerSessionModel = getRegisterSessionResult.Data;

            //更新会话
            registerSessionModel.EmailVerificataion = verify.ToString();
            registerSessionModel.EmailVerifyExpireTime = expireTime.ToTimeStamp();

            //保存到数据库
            var jsonStr = registerSessionModel.ToJson();
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.RegisterRedis.StringSetAsync(token, jsonStr, timeSpan, When.Exists);
            if (!result)
            {
                return new OperateResult
                {
                    Message = "注册会话不存在，请重试"
                };
            }
            return OperateResult.CreateSuccessResult();
        }



        /// <summary>
        /// 移除注册会话
        /// </summary>
        /// <param name="emailRegisterSessionId">邮箱注册会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> RemoveSessionAsync(string emailRegisterSessionId)
        {
            await this.RegisterRedis.KeyDeleteAsync(emailRegisterSessionId);

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 通过邮箱注册账号
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> EmailRegisterAccountAsync(EmailRegisterSessionModel registerSessionModel, string token)
        {
            //获取用户注册信息
            if (registerSessionModel == null)
            {
                return OperateResult.CreateFailResult("注册会话不存在！");
            }

            //生成密码盐
            string salt = Guid.NewGuid().ToString();

            //重新生成密码
            var password = this.CryptographyBLL.GetSHA256HashCode($"{registerSessionModel.Password}-{salt}");

            //创建用户数据
            var userEntity = new UserEntity()
            {
                Email = registerSessionModel.Email,
            }.Create();

            //创建用户登录数据
            var logOnEntity = new LogOnEntity()
            {
                IsEnable = true,
                Password = password,
                Salt = salt,
                UserId = userEntity.Id
            }.Create();

            //提前准备好数据，然后开启事务处理数据 尽量减少事务时间
            using (var transaction = await this.RSAppDb.Database.BeginTransactionAsync())
            {
                try
                {
                    //插入用户数据
                    var insertResult = await this.InsertAsync(userEntity);
                    if (!insertResult.IsSuccess)
                    {
                        return insertResult;
                    }
                    //插入用户登录数据
                    insertResult = await this.InsertAsync(logOnEntity);
                    if (!insertResult.IsSuccess)
                    {
                        return insertResult;
                    }
                    await transaction.CommitAsync();
                }
                catch (Exception ex)
                {
                    await transaction.RollbackAsync();
                    throw ex;
                }
            }

            //移除注册会话
            var removeSessioResult = await this.RemoveSessionAsync(token);
            if (!removeSessioResult.IsSuccess)
            {
                return removeSessioResult;
            }

            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 邮箱是否注册
        /// </summary>
        /// <param name="emailAddress">邮箱地址</param>
        /// <param name="token">注册会话</param>
        /// <returns>如果注册返回true 未注册 返回false</returns>
        public async Task<OperateResult> IsEmailRegisteredAsync(string emailAddress)
        {
            string emailHashCode = this.CryptographyBLL.GetMD5HashCode(emailAddress);

            //从Redis查询是否已经注册过了
            var isKeyExists = await this.RegisterRedis.KeyExistsAsync($"Registerd:{emailHashCode}");
            //如果已经注册直接返回
            if (isKeyExists)
            {
                return OperateResult.CreateSuccessResult();
            }

            //如果没注册，从数据库获取判断是否已经注册过了
            var anyResult = await this.Any<UserEntity>(t => t.Email == emailAddress);
            //如果已经注册直接返回
            if (anyResult.IsSuccess)
            {
                //如果已经注册写入Redis 存储，避免重复查询数据库
                await this.RegisterRedis.StringSetAsync($"Registerd:{emailHashCode}", RedisValue.EmptyString, new TimeSpan(30 * TimeSpan.TicksPerMinute));
                return OperateResult.CreateSuccessResult();
            }

            //否则返回未注册
            return OperateResult.CreateFailResult();
        }

       
    }
}
