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
        /// 更新注册会话
        /// </summary>
        /// <param name="token">注册会话Id</param>
        /// <param name="verification">短信验证码</param>
        /// <param name="expireTime">验证码失效时间</param>
        /// <returns></returns>
        Task<OperateResult> UpdateSessionAsync(string token, int verification, DateTime expireTime);

        /// <summary>
        /// 获取注册会话数据
        /// </summary>
        /// <param name="token">注册会话Id</param>
        /// <returns></returns>
        Task<OperateResult<RegisterSessionModel>> GetSessionAsync(string token);

        /// <summary>
        /// 注册账号
        /// </summary>
        /// <returns></returns>
        Task<OperateResult> RegisterAccountAsync(RegisterSessionModel registerSessionModel,string token);


        /// <summary>
        /// 创建注册会话
        /// </summary>
        /// <param name="token">注册会话主键</param>
        /// <param name="registerSessionModel">注册会话类</param>
        /// <param name="expireTime">注册会话过期时间</param>
        /// <returns></returns>
        Task<OperateResult> CreateSessionAsync(string token, RegisterSessionModel registerSessionModel, DateTime expireTime);


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
