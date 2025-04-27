using RS.HMIServer.Models;
using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IDAL
{
    public interface IGeneralDAL : IRepository
    {
        /// <summary>
        /// 保存会话
        /// </summary>
        /// <param name="sessionModel"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<OperateResult> SaveSessionModelAsync(SessionModel sessionModel, string sessionId);

        /// <summary>
        /// 获取会话
        /// </summary>
        /// <param name="sessionModelKey"></param>
        /// <returns></returns>
        Task<OperateResult<SessionModel>> GetSessionModelAsync(string sessionModelKey);

        /// <summary>
        /// 移除会话
        /// </summary>
        /// <param name="sessionModelKey"></param>
        /// <returns></returns>
        Task<OperateResult> RemoveSessionModelAsync(string sessionModelKey);

        /// <summary>
        /// 获取登录客户端信息
        /// </summary>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<OperateResult<LoginClientModel>> GetLoginClientModelAsync(string clientId);

        /// <summary>
        /// 检查客户端Ip是否存在
        /// </summary>
        /// <param name="loginClientModel"></param>
        /// <param name="clientId"></param>
        /// <returns></returns>
        Task<OperateResult> IsClientIPExistAsync(LoginClientModel loginClientModel, string clientId);

        /// <summary>
        /// 保存客户端信息
        /// </summary>
        /// <param name="loginClientModel"></param>
        /// <returns></returns>
        Task<OperateResult<string>> SaveClientIdAsync(LoginClientModel loginClientModel);
    }
}
