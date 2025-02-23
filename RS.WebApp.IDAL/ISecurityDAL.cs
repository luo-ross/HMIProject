using RS.WebApp.Models;
using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.WebApp.IDAL
{
    public interface ISecurityDAL : IRepository
    {

        /// <summary>
        /// 创建密码重置会话
        /// </summary>
        /// <param name="token">密码重置会话主键</param>
        /// <param name="passwordResetSessionModel">密码重置实体信息</param>
        /// <returns></returns>
        Task<OperateResult> CreatePasswordResetSessionAsync(string token, PasswordResetSessionModel passwordResetSessionModel);


        /// <summary>
        /// 密码重置会话验证
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        Task<OperateResult> PasswordResetSessionValidAsync(string email, string token);
    }
}
