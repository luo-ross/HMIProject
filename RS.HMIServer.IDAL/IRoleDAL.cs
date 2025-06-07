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
    public interface IRoleDAL : IRepository
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="roleModel">角色数据</param>
        /// <returns></returns>
        Task<OperateResult<RoleModel>> AddRoleAsync(RoleModel roleModel);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="roleModel">更新角色</param>
        /// <returns></returns>
        Task<OperateResult> UpdateRoleAsync(RoleModel roleModel);


        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="roleModel">角色参数</param>
        /// <returns></returns>
        Task<OperateResult<RoleModel>> DeleteRoleAsync(RoleModel roleModel);

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <returns></returns>
        Task<OperateResult<PageDataModel<RoleModel>>> GetRolesAsync(Pagination pagination);

        /// <summary>
        /// 更新绑定公司
        /// </summary>
        /// <param name="roleModel">角色参数</param>
        /// <returns></returns>
        Task<OperateResult> UpdateCompanyIdAsync(RoleModel roleModel);

        /// <summary>
        /// 更新角色备注
        /// </summary>
        /// <param name="roleModel">角色参数</param>
        /// <returns></returns>
        Task<OperateResult> UpdateDescriptionAsync(RoleModel roleModel);

        /// <summary>
        /// 更新角色名称
        /// </summary>
        /// <param name="roleModel">更新角色</param>
        /// <returns></returns>
        Task<OperateResult> UpdateNameAsync(RoleModel roleModel);

     
    }
}
