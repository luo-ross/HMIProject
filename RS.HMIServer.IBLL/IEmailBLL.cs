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
    public interface IEmailBLL
    {
        /// <summary>
        /// 发送邮箱验证码
        /// </summary>
        /// <param name="emailRegisterVerifyModel">邮箱注册验证码实体</param>
        /// <returns></returns>
        Task<OperateResult> SendVerifyAsync(EmailRegisterVerifyModel emailRegisterVerifyModel);

        /// <summary>
        /// 发送密码重置链接
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="passwordResetToken">密码重置会话Token</param>
        /// <returns></returns>
        Task<OperateResult> SendPassResetAsync(EmailPasswordResetModel emailPasswordResetModel);
    }
}
