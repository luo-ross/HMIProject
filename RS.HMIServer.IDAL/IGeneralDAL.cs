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
    public interface IGeneralDAL: IRepository
    {
        /// <summary>
        /// 保存会话
        /// </summary>
        /// <param name="sessionModel"></param>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<OperateResult> SaveSessionModelAsync(SessionModel sessionModel,string sessionId);

        /// <summary>
        /// 获取会话
        /// </summary>
        /// <param name="sessionModelKey"></param>
        /// <returns></returns>
        Task<OperateResult<SessionModel>> GetSessionModelAsync(string sessionModelKey);


        Task<OperateResult<LoginClientModel>> GetLoginClientModelAsync(string clientId);
        Task<OperateResult> IsClientIPExistAsync(LoginClientModel loginClientModel, string clientId);
        Task<OperateResult<string>> SaveClientIdAsync(LoginClientModel loginClientModel);
    }
}
