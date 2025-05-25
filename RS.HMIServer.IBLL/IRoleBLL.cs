using RS.HMIServer.Models;
using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IBLL
{
    public interface IRoleBLL
    {
        /// <summary>
        /// 新增角色
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> AddRoleAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 删除角色
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> DeleteRoleAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 获取角色列表
        /// </summary>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult<AESEncryptModel>> GetRolesAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 更新公司绑定
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult> UpdateCompanyIdAsync(AESEncryptModel aesEncryptModel, string sessionId);
       
        /// <summary>
        /// 更新备注
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话Id</param>
        /// <returns></returns>
        Task<OperateResult> UpdateDescriptionAsync(AESEncryptModel aesEncryptModel, string sessionId);

        /// <summary>
        /// 更新角色
        /// </summary>
        /// <param name="aesEncryptModel">加密数据</param>
        /// <param name="sessionId">会话主键</param>
        /// <returns></returns>
        Task<OperateResult> UpdateNameAsync(AESEncryptModel aesEncryptModel, string sessionId);
    }
}
