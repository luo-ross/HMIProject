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
        /// 删除用户
        /// </summary>
        /// <returns></returns>
        Task<OperateResult<UserModel>> DeleteUserAsync(UserModel userModel);

        /// <summary>
        /// 获取用户列表
        /// </summary>
        /// <returns></returns>
        Task<OperateResult<PageDataModel<UserModel>>> GetUsersAsync(Pagination pagination);

        /// <summary>
        /// 更新邮箱
        /// </summary>
        /// <returns></returns>
        Task<OperateResult> UpdateEmailAsync(UserModel userModel);

        /// <summary>
        /// 更新用户是否禁用
        /// </summary>
        /// <param name="userModel">参数</param>
        /// <returns></returns>
        Task<OperateResult> UpdateIsDisableAsync(UserModel userModel);

        /// <summary>
        /// 更新用户昵称
        /// </summary>
        /// <returns></returns>
        Task<OperateResult> UpdateNickNameAsync(UserModel userModel);
    }
}
