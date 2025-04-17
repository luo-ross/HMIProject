using RS.HMIServer.Models;
using RS.Commons;
using RS.Commons.Extensions;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IDAL
{
    public interface IRegisterDAL : IRepository
    {

        /// <summary>
        /// 创建注册会话
        /// </summary>
        /// <param name="token">注册会话主键</param>
        /// <param name="registerSessionModel">注册会话类</param>
        /// <param name="expireTime">注册会话过期时间</param>
        /// <returns></returns>
        Task<OperateResult<string>> CreateEmailSessionAsync(string emailHashCode, EmailRegisterSessionModel registerSessionModel, DateTime expireTime);


        /// <summary>
        /// 通过Email注册会话Id获取EmailHashCode
        /// </summary>
        /// <param name="registerSessionId"></param>
        /// <returns></returns>
        Task<OperateResult<string>> GetEmailHashCodeAsync(string registerSessionId);

        /// <summary>
        /// 获取邮箱注册会话数据
        /// </summary>
        /// <param name="token">注册会话Id</param>
        /// <returns></returns>
        Task<OperateResult<EmailRegisterSessionModel>> GetEmailRegisterSessionAsync(string emailHashCode);



        /// <summary>
        /// 更新注册会话
        /// </summary>
        /// <param name="token">注册会话Id</param>
        /// <param name="verify">短信验证码</param>
        /// <param name="expireTime">验证码失效时间</param>
        /// <returns></returns>
        Task<OperateResult> UpdateEmailSessionAsync(string token, int verify, DateTime expireTime);

    
        /// <summary>
        /// 注册账号
        /// </summary>
        /// <returns>返回用户注册主键</returns>
        Task<OperateResult> EmailRegisterAccountAsync(EmailRegisterSessionModel registerSessionModel,string registerSessionId);

    
        /// <summary>
        /// 移除注册会话
        /// </summary>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        Task<OperateResult> RemoveSessionAsync(string token);

        /// <summary>
        /// 邮箱是否注册
        /// </summary>
        /// <param name="emailAddress">邮箱地址</param>
        /// <returns></returns>
        Task<OperateResult> IsEmailRegisteredAsync(string emailAddress);

   }
}
