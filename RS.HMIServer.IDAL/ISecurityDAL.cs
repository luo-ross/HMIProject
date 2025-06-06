﻿using RS.HMIServer.Models;
using RS.Commons;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.HMIServer.IDAL
{
    public interface ISecurityDAL : IRepository
    {

        /// <summary>
        /// 创建密码重置会话
        /// </summary>
        /// <param name="token">密码重置会话主键</param>
        /// <param name="EmailSecurityModel">密码重置实体信息</param>
        /// <returns></returns>
        Task<OperateResult> CreateEmailPasswordResetSessionAsync(string token, EmailSecurityModel EmailSecurityModel);
       
        /// <summary>
        /// 密码重置会话验证
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        Task<OperateResult<EmailSecurityModel>> EmailPasswordResetSessionValidAsync(string email, string token);

        /// <summary>
        /// 密码重置确认
        /// </summary>
        /// <param name="emailPasswordConfirmModel">密码重置信息</param>
        /// <returns></returns>
        Task<OperateResult> EmailPasswordResetConfirmAsync(EmailPasswordConfirmModel emailPasswordConfirmModel);
        
        /// <summary>
        /// 创建验证会话
        /// </summary>
        /// <param name="verifyImgInitModel"></param>
        /// <returns></returns>
        Task<OperateResult<string>> CreateVerifySessionModelAsync(ImgVerifyInitModel verifyImgInitModel,string sessionId);
      
        /// <summary>
        /// 验证是否可以创建验证会话
        /// </summary>
        /// <param name="sessionId"></param>
        /// <returns></returns>
        Task<OperateResult> IsCanCreateImgVerifySessionAsync(string sessionId);
      
        /// <summary>
        /// 获取验证码会话数据
        /// </summary>
        /// <param name="verifySessionId">验证码会话Id</param>
        /// <returns></returns>
        Task<OperateResult<ImgVerifySessionModel>> GetImgVerifySessionModelAsync(string verifySessionId);
    }
}
