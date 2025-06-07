
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Models;
using RS.HMIServer.IBLL;

namespace RS.HMIServer.Controllers
{
    [ApiController]
    [Route("/api/v1/[controller]/[action]")]
    [Authorize]
    public class RoleController : BaseController
    {
        /// <summary>
        /// 角色服务接口
        /// </summary>
        private readonly IRoleBLL RoleBLL;

        private readonly ILogBLL LogBLL;
        public RoleController(IRoleBLL roleBLL, ILogBLL logBLL)
        {
            this.RoleBLL = roleBLL;
            this.LogBLL = logBLL;
        }

        /// <summary>
        /// 获取角色接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult<AESEncryptModel>> GetRole(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.GetRolesAsync(aesEncryptModel, SessionId);
        }

        /// <summary>
        /// 新增角色接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult<AESEncryptModel>> AddRole(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.AddRoleAsync(aesEncryptModel, SessionId);
        }

        /// <summary>
        /// 新增角色接口
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateRole(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.UpdateRoleAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 更新角色名称
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateName(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.UpdateNameAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 更新角色备注
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateDescription(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.UpdateDescriptionAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 更新角色备注
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult> UpdateCompanyId(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.UpdateCompanyIdAsync(aesEncryptModel, SessionId);
        }


        /// <summary>
        /// 删除角色
        /// </summary>
        /// <returns></returns>
        [HttpPost]
        public async Task<OperateResult<AESEncryptModel>> DeleteRole(AESEncryptModel aesEncryptModel)
        {
            return await this.RoleBLL.DeleteRoleAsync(aesEncryptModel, SessionId);
        }
    }
}
