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
    public interface IUserDAL: IRepository
    {
        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        Task<OperateResult<List<UserModel>>> GetUsersAsync();
    }
}
