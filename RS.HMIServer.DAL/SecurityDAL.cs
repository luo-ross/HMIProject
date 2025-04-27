using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json.Linq;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Extensions;
using RS.Commons.Helper;
using RS.HMIServer.DAL.Redis;
using RS.HMIServer.DAL.SqlServer;
using RS.HMIServer.Entity;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Models;
using RTools_NTS.Util;
using StackExchange.Redis;
using System;

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
        /// Redis密码重置缓存接口
        /// </summary>
        private readonly IDatabase ImgVerifyRedis;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;


        public SecurityDAL(RSAppDbContext rsAppDb, ICryptographyBLL cryptographyBLL, RedisDbContext redisDbContext)
        {
            this.RSAppDb = rsAppDb;
            this.CryptographyBLL = cryptographyBLL;
            this.PasswordResetRedis = redisDbContext.GetPasswordResetRedis();
            this.ImgVerifyRedis = redisDbContext.GetImgVerifyRedis();
        }

        /// <summary>
        /// 创建密码重置会话
        /// </summary>
        /// <param name="token">密码重置会话主键</param>
        /// <param name="EmailSecurityModel">密码重置实体信息</param>
        /// <returns></returns>
        public async Task<OperateResult> CreateEmailPasswordResetSessionAsync(string token, EmailSecurityModel emailSecurityModel)
        {
            //检查秘密重置会话是否已经存在
            var emailHashCode = this.CryptographyBLL.GetMD5HashCode(emailSecurityModel.Email);
            var isSessionExist = this.PasswordResetRedis.KeyExists(emailHashCode);
            if (isSessionExist)
            {
                return new OperateResult
                {
                    Message = "密码重置会话已存在，请勿重复发起！"
                };
            }

            //将会话提示转为字符串存储到Redis数据库
            var jsonStr = emailSecurityModel.ToJson();
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
            result = await this.PasswordResetRedis.StringSetAsync(token, emailHashCode, timeSpan, When.NotExists);
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
        public async Task<OperateResult<EmailSecurityModel>> EmailPasswordResetSessionValidAsync(string email, string token)
        {
            //根据会话Token获取会话数据
            var emailHashCode = await this.PasswordResetRedis.StringGetAsync(token);
            if (!emailHashCode.HasValue)
            {
                return OperateResult.CreateFailResult<EmailSecurityModel>("密码重置会话不存在");
            }

            var stringGetResult = await this.PasswordResetRedis.StringGetAsync(emailHashCode.ToString());
            if (!stringGetResult.HasValue)
            {
                return OperateResult.CreateFailResult<EmailSecurityModel>("密码重置会话不存在");
            }

            //获取密码重置会话验证邮箱是否一致
            var emailSecurityModel = stringGetResult.ToString().ToObject<EmailSecurityModel>();
            if (emailSecurityModel == null || !emailSecurityModel.Email.Equals(email))
            {
                return OperateResult.CreateFailResult<EmailSecurityModel>("密码重置会话不存在");
            }

            //将会话提示转为字符串存储到Redis数据库
            var jsonStr = emailSecurityModel.ToJson();
            //生成有效期
            DateTime expireTime = DateTime.Now.AddSeconds(60 * 5);
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.PasswordResetRedis.StringSetAsync(emailHashCode.ToString(), jsonStr, timeSpan, When.Exists);
            if (!result)
            {
                return OperateResult.CreateFailResult<EmailSecurityModel>("密码重置会话不存在");
            }

            //然后创建一个键值映射
            result = await this.PasswordResetRedis.StringSetAsync(token, emailHashCode, timeSpan, When.Exists);
            if (!result)
            {
                return OperateResult.CreateFailResult<EmailSecurityModel>("密码重置会话不存在");
            }

            return OperateResult.CreateSuccessResult(emailSecurityModel);
        }


        /// <summary>
        /// 更新密码
        /// </summary>
        /// <param name="emailPasswordConfirmModel">密码更新实体</param>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException">参数为Null</exception>
        public async Task<OperateResult> EmailPasswordResetConfirmAsync(EmailPasswordConfirmModel emailPasswordConfirmModel)
        {
            //验证用户是否存在
            if (emailPasswordConfirmModel == null)
            {
                throw new ArgumentNullException(nameof(emailPasswordConfirmModel));
            }

            if (!emailPasswordConfirmModel.Email.IsEmail())
            {
                return OperateResult.CreateFailResult("邮件格式不存在");
            }

            if (string.IsNullOrEmpty(emailPasswordConfirmModel.Password) ||
                string.IsNullOrWhiteSpace(emailPasswordConfirmModel.Password))
            {
                return OperateResult.CreateFailResult("密码不能为空");
            }

            var validResult = await this.EmailPasswordResetSessionValidAsync(emailPasswordConfirmModel.Email, emailPasswordConfirmModel.Token);
            if (!validResult.IsSuccess)
            {
                return validResult;
            }

            string email = emailPasswordConfirmModel.Email;
            //查询用户是否存在
            var userFindResult = await this.FirstOrDefaultAsync<UserEntity>(t => t.Email == email);
            if (!userFindResult.IsSuccess)
            {
                return OperateResult.CreateSuccessResult("用户不存在");
            }
            var userEntity = userFindResult.Data;
            if (userEntity.IsDisabled == true)
            {
                return OperateResult.CreateFailResult("用户已禁用");
            }

            //获取登录信息
            var logonFindResult = await this.FirstOrDefaultAsync<LogOnEntity>(t => t.UserId == userEntity.Id);
            if (!logonFindResult.IsSuccess)
            {
                return OperateResult.CreateFailResult("用户登录信息不存在");
            }

            var logonEntity = logonFindResult.Data;

            //密码重置
            //生成密码盐
            string salt = Guid.NewGuid().ToString();
            //重新生成密码
            var password = this.CryptographyBLL.GetSHA256HashCode($"{emailPasswordConfirmModel.Password}-{salt}");

            logonEntity.Salt = salt;
            logonEntity.Password = password;

            var updateResult = await this.UpdateAsync(logonEntity);
            if (!updateResult.IsSuccess)
            {
                return OperateResult.CreateFailResult("密码更新失败");
            }


            //更新成功后清除会话
            var emailHashCode = await this.PasswordResetRedis.StringGetAsync(emailPasswordConfirmModel.Token);
            if (emailHashCode.HasValue)
            {
                await this.PasswordResetRedis.KeyDeleteAsync(emailHashCode.ToString());
            }
            return OperateResult.CreateSuccessResult();
        }

        public async Task<OperateResult<string>> CreateVerifySessionModelAsync(ImgVerifyInitModel verifyImgInitModel, string sessionId)
        {
            string verifySessionId = Guid.NewGuid().ToString();

            ImgVerifySessionModel model = new ImgVerifySessionModel()
            {
                CreateCount = 1,
                ImgBtnPositionX = verifyImgInitModel.ImgBtnPositionX,
                ImgBtnPositionY = verifyImgInitModel.ImgBtnPositionY,
                Rect = verifyImgInitModel.Rect,
                VerifySessionId = verifySessionId,
            };

            //获取已创建的验证码会话
            var getImgVerifySessionModelResult = await this.GetImgVerifySessionModelAsync(sessionId);
            if (getImgVerifySessionModelResult.IsSuccess)
            {
                var verifySessionModelExist = getImgVerifySessionModelResult.Data;
                verifySessionId = verifySessionModelExist.VerifySessionId;
                model.CreateCount = model.CreateCount + verifySessionModelExist.CreateCount;
            }
            //如果在2分钟内连续20次请求 则返回失败
            if (model.CreateCount > 20)
            {
                return OperateResult.CreateFailResult<string>("请求太频繁了，先歇一会吧");
            }

            //将会话提示转为字符串存储到Redis数据库
            var jsonStr = model.ToJson();
            //生成有效期 2分钟内有效
            DateTime expireTime = DateTime.Now.AddSeconds(60 * 2);
            TimeSpan timeSpan = expireTime.Subtract(DateTime.Now);
            var result = await this.ImgVerifyRedis.StringSetAsync(sessionId, jsonStr, timeSpan, When.Always);
            if (!result)
            {
                return OperateResult.CreateFailResult<string>("创建图像验证会话失败");
            }

            //再创建一组映射
            result = await this.ImgVerifyRedis.StringSetAsync(verifySessionId, sessionId, timeSpan, When.Always);
            if (!result)
            {
                return OperateResult.CreateFailResult<string>("创建图像验证会话失败");
            }

            return OperateResult.CreateSuccessResult(verifySessionId);
        }

        /// <summary>
        /// 验证是否可以创建验证会话
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        public async Task<OperateResult> IsCanCreateImgVerifySessionAsync(string sessionId)
        {
            ImgVerifySessionModel verifySessionModelExist = null;
            var stringGetResult = await this.ImgVerifyRedis.StringGetAsync(sessionId);
            if (stringGetResult.HasValue)
            {
                verifySessionModelExist = stringGetResult.ToString().ToObject<ImgVerifySessionModel>();
            }
            if (verifySessionModelExist != null && verifySessionModelExist.CreateCount > 20)
            {
                return OperateResult.CreateFailResult<string>("请求太频繁了，先歇一会吧");
            }
            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 获取验证码会话数据
        /// </summary>
        /// <param name="verifySessionId">验证码会话Id</param>
        /// <returns></returns>
        public async Task<OperateResult<ImgVerifySessionModel>> GetImgVerifySessionModelAsync(string verifySessionId)
        {
            ImgVerifySessionModel verifySessionModelExist = null;
            var stringGetResult = await this.ImgVerifyRedis.StringGetAsync(verifySessionId);
            if (stringGetResult.HasValue)
            {
                verifySessionModelExist = stringGetResult.ToString().ToObject<ImgVerifySessionModel>();
            }

            if (verifySessionModelExist == null)
            {
                return OperateResult.CreateFailResult<ImgVerifySessionModel>("验证码不存在或者已失效");
            }
            return OperateResult.CreateSuccessResult(verifySessionModelExist);
        }
    }
}
