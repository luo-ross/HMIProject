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
    /// <summary>
    /// 安全服务接口
    /// </summary>
    public interface ISecurityService
    {
        /// <summary>
        /// 密码重置
        /// </summary>
        /// <param name="hostWithScheme">主机名协议</param>
        /// <param name="passwordResetModel">名称重置实体</param>
        /// <returns></returns>
        Task<OperateResult> PasswordResetAsync(string hostWithScheme, PasswordResetModel passwordResetModel);

        /// <summary>
        /// 密码重置会话验证
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        Task<OperateResult> PasswordResetSessionValidAsync(string email, string token);
    }
}
