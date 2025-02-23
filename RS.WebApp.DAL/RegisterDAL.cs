
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.WebApp.Entity;
using RS.WebApp.DAL.Redis;
using RS.WebApp.DAL.SqlServer;
using RS.WebApp.IDAL;
using RS.WebApp.Models;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Models;
using RTools_NTS.Util;
using StackExchange.Redis;
using System;

namespace RS.WebApp.DAL
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
        private readonly ICryptographyService CryptographyService;
        public RegisterDAL(RSAppDbContext rsAppDb, RedisDbContext redisDbContext, ICryptographyService cryptographyService)
        {
            this.RSAppDb = rsAppDb;
            this.RegisterRedis = redisDbContext.GetRegisterRedis();
            this.CryptographyService = cryptographyService;
        }


        /// <summary>
        /// 获取注册会话
        /// </summary>
        /// <param name="token"></param>
        /// <returns></returns>
        public async Task<OperateResult<RegisterSessionModel>> GetSessionAsync(string token)
        {
            //根据会话Token获取会话数据
            var stringSetResult = await this.RegisterRedis.StringGetAsync(token);
            if (!stringSetResult.HasValue)
            {
                return OperateResult.CreateFailResult<RegisterSessionModel>("未获取到注册会话");
            }

            //获取Json字符串
            var result = stringSetResult.ToString().ToObject<RegisterSessionModel>();

            return OperateResult.CreateSuccessResult(result);
        }


        /// <summary>
        /// 更新注册会话
        /// </summary>
        /// <param name="token">注册会话Id</param>
        /// <param name="verification">短信验证码</param>
        /// <param name="expireTime">验证码失效时间</param>
        /// <returns></returns>
        public async Task<OperateResult> UpdateSessionAsync(string token, int verification, DateTime expireTime)
        {
            //获取注册会话
            var getRegisterSessionResult = await GetSessionAsync(token);
            if (!getRegisterSessionResult.IsSuccess)
            {
                return getRegisterSessionResult;
            }
            var registerSessionModel = getRegisterSessionResult.Data;

            //更新会话
            registerSessionModel.PhoneVerificataion = verification.ToString();
            registerSessionModel.PhoneVerificationExpireTime = expireTime.ToTimeStamp();


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
        /// 创建注册会话
        /// </summary>
        /// <param name="token">注册会话主键</param>
        /// <param name="registerSessionModel">注册会话类</param>
        /// <param name="expireTime">注册会话过期时间</param>
        /// <returns></returns>
        public async Task<OperateResult> CreateSessionAsync(string token, RegisterSessionModel registerSessionModel, DateTime expireTime)
        {
            //将会话提示转为字符串存储到Redis数据库
            var jsonStr = registerSessionModel.ToJson();
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.RegisterRedis.StringSetAsync(token, jsonStr, timeSpan, When.NotExists);
            if (!result)
            {
                return new OperateResult
                {
                    Message = "注册会话已存在，请不要重复发起注册"
                };
            }
            return OperateResult.CreateSuccessResult();
        }


        /// <summary>
        /// 移除注册会话
        /// </summary>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> RemoveSessionAsync(string token)
        {
            await this.RegisterRedis.KeyDeleteAsync(token);

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <returns></returns>
        public async Task<OperateResult> RegisterAccountAsync(RegisterSessionModel registerSessionModel, string token)
        {
            //获取用户注册信息
            if (registerSessionModel == null)
            {
                return OperateResult.CreateFailResult("注册会话不存在！");
            }

            //生成密码盐
            string salt = Guid.NewGuid().ToString();

            //重新生成密码
            var password = this.CryptographyService.GetSHA256HashCode($"{registerSessionModel.Password}-{salt}");


            //创建用户数据
            var userEntity = new UserEntity()
            {
                Email = registerSessionModel.Email,
                Phone = registerSessionModel.Phone,
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
                return OperateResult.CreateFailResult<AESEncryptModel>(removeSessioResult);
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
            string token = this.CryptographyService.GetMD5HashCode(emailAddress);

            //从Redis查询是否已经注册过了
            var isKeyExists = await this.RegisterRedis.KeyExistsAsync($"Registerd:{token}");
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
                await this.RegisterRedis.StringSetAsync($"Registerd:{token}", RedisValue.EmptyString, new TimeSpan(30 * TimeSpan.TicksPerMinute));
                return OperateResult.CreateSuccessResult();
            }

            //否则返回未注册
            return OperateResult.CreateFailResult();
        }
    }
}
