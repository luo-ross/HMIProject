using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using StackExchange.Redis;

namespace RS.HMIServer.DAL
{

    /// <summary>
    /// 用户数据逻辑层
    /// </summary>
    [ServiceInjectConfig(typeof(ISecurityDAL), ServiceLifetime.Transient)]
    internal class SecurityDAL : Repository, ISecurityDAL
    {
        /// <summary>
        /// Redis密码重置缓存接口
        /// </summary>
        private readonly IDatabase PasswordResetRedis;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;


        public SecurityDAL(RSAppDbContext rsAppDb, ICryptographyBLL cryptographyBLL, RedisDbContext redisDbContext)
        {
            this.RSAppDb = rsAppDb;
            this.CryptographyBLL = cryptographyBLL;
            this.PasswordResetRedis = redisDbContext.GetPasswordResetRedis();
        }

        /// <summary>
        /// 创建密码重置会话
        /// </summary>
        /// <param name="token">密码重置会话主键</param>
        /// <param name="EmailSecurityModel">密码重置实体信息</param>
        /// <returns></returns>
        public async Task<OperateResult> CreateEmailPasswordResetSessionAsync(string token, EmailSecurityModel EmailSecurityModel)
        {
            //检查秘密重置会话是否已经存在
            var emailHashCode = this.CryptographyBLL.GetMD5HashCode(EmailSecurityModel.Email);
            var isSessionExist = this.PasswordResetRedis.KeyExists(emailHashCode);
            if (isSessionExist)
            {
                return new OperateResult
                {
                    Message = "密码重置会话已存在，请勿重复发起！"
                };
            }

            //将会话提示转为字符串存储到Redis数据库
            var jsonStr = EmailSecurityModel.ToJson();
            //生成有效期
            DateTime expireTime = DateTime.Now.AddSeconds(60 * 5);
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.PasswordResetRedis.StringSetAsync(emailHashCode, jsonStr, timeSpan, When.NotExists);
            if (!result)
            {
                return new OperateResult
                {
                    Message = "密码重置会话已存在，请不要重复发起"
                };
            }

            //然后创建一个键值映射
            result = await this.PasswordResetRedis.StringSetAsync(token, jsonStr, timeSpan, When.NotExists);
            if (!result)
            {
                return new OperateResult
                {
                    Message = "密码重置会话已存在，请不要重复发起"
                };
            }

            return OperateResult.CreateSuccessResult();
        }



        /// <summary>
        /// 密码重置会话验证
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> PasswordResetSessionValidAsync(string email, string token)
        {
            //根据会话Token获取会话数据
            var emailHashCode = await this.PasswordResetRedis.StringGetAsync(token);
            if (!emailHashCode.HasValue)
            {
                return OperateResult.CreateFailResult("密码重置会话不存在");
            }

            var stringSetResult = await this.PasswordResetRedis.StringGetAsync(emailHashCode.ToString());
            if (!stringSetResult.HasValue)
            {
                return OperateResult.CreateFailResult("密码重置会话不存在");
            }

            //获取密码重置会话验证邮箱是否一致
            var EmailSecurityModel = stringSetResult.ToString().ToObject<EmailSecurityModel>();
            if (!EmailSecurityModel.Email.Equals(email))
            {
                return OperateResult.CreateFailResult("密码重置会话不存在");
            }

            //如果一致 将有效时间往后再延长5分钟
            DateTime expireTime = DateTime.Now.AddSeconds(60 * 5);
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.PasswordResetRedis.StringSetAsync(token, EmailSecurityModel.ToJson(), timeSpan, When.Exists);
            if (!result)
            {
                return new OperateResult
                {
                    Message = "密码重置会话不存在"
                };
            }

            return OperateResult.CreateSuccessResult();
        }
    }
}
