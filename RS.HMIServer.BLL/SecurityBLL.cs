﻿using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using NetTopologySuite.Utilities;
using RS.HMIServer.Entity;
using RS.HMIServer.DAL;
using RS.HMIServer.IBLL;
using RS.HMIServer.IDAL;
using RS.HMIServer.Models;
using RS.Commons;
using RS.Commons.Attributs;
using RS.Commons.Enums;
using RS.Commons.Extensions;
using RS.Models;
using System;
using System.Data.SqlTypes;
using System.Net.Mail;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using TencentCloud.Ciam.V20220331.Models;
using RS.Commons.Helper;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 安全服务
    /// </summary>
    [ServiceInjectConfig(typeof(ISecurityBLL), ServiceLifetime.Transient, IsInterceptor = true)]
    internal class SecurityBLL : ISecurityBLL
    {

        /// <summary>
        /// 邮箱服务
        /// </summary>
        private readonly IEmailBLL EmailBLL;

        /// <summary>
        /// 密码服务
        /// </summary>
        private readonly ICryptographyBLL CryptographyBLL;

        /// <summary>
        /// 注册数据仓储接口
        /// </summary>
        private readonly ISecurityDAL SecurityDAL;

        public SecurityBLL(ISecurityDAL securityDAL, IEmailBLL emailBLL, ICryptographyBLL cryptographyBLL)
        {
            this.EmailBLL = emailBLL;
            this.CryptographyBLL = cryptographyBLL;
            this.SecurityDAL = securityDAL;
        }


        public async Task<OperateResult> PasswordResetAsync(string hostWithScheme, PasswordResetModel passwordResetModel)
        {
            if (passwordResetModel == null)
            {
                throw new ArgumentNullException(nameof(passwordResetModel));
            }
            if (string.IsNullOrEmpty(hostWithScheme) || string.IsNullOrWhiteSpace(hostWithScheme))
            {
                throw new ArgumentNullException(nameof(passwordResetModel));
            }

            if (string.IsNullOrEmpty(passwordResetModel.Email) || string.IsNullOrWhiteSpace(passwordResetModel.Email))
            {
                throw new ArgumentNullException(nameof(passwordResetModel.Email));
            }

            //验证邮箱格式是否正确
            if (!passwordResetModel.Email.IsEmail())
            {
                return OperateResult.CreateFailResult("邮箱格式不正确！");
            }

            //验证用户是否存在
            var userEntity = this.SecurityDAL.FirstOrDefaultAsync<UserEntity>(t => t.Email == passwordResetModel.Email);
            if (userEntity == null)
            {
                return OperateResult.CreateFailResult("用户不存在！");
            }

            //创建修改密码会话
            string passwordResetToken = Guid.NewGuid().ToString();  //创建会话主键
            var operateResult = await this.SecurityDAL.CreatePasswordResetSessionAsync(passwordResetToken, new PasswordResetSessionModel()
            {
                Email = passwordResetModel.Email,
            });
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            //发送邮件到用户
            var emailPasswordResetModel = new EmailPasswordResetModel
            {
                Email = passwordResetModel.Email,
                ResetLink = $"{hostWithScheme}/password/edit?email={Uri.EscapeDataString(passwordResetModel.Email)}&token={passwordResetToken}",
            };
            operateResult = await this.EmailBLL.SendPassResetAsync(emailPasswordResetModel);
            if (!operateResult.IsSuccess)
            {
                return operateResult;
            }

            //发送给用户后，等待用户点击链接进行密码修改

            return operateResult;

        }


        /// <summary>
        /// 密码重置会话验证
        /// </summary>
        /// <param name="email">邮箱</param>
        /// <param name="token">会话主键</param>
        /// <returns></returns>
        public async Task<OperateResult> PasswordResetSessionValidAsync(string email, string token)
        {
            return await this.SecurityDAL.PasswordResetSessionValidAsync(email, token);
        }
    }
}
