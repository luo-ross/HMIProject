using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RS.HMIServer.IBLL;
using RS.Commons;
using RazorLight;
using RS.Models;
using RS.HMIServer.Models;
using TencentCloud.Teo.V20220901.Models;
using System.Net.NetworkInformation;
using System.Security.Cryptography;
using Org.BouncyCastle.Bcpg.OpenPgp;
using System.Dynamic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using TencentCloud.Ame.V20190916.Models;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 腾讯邮箱发送服务
    /// </summary>
    internal class TencentEmailBLL : IEmailBLL
    {
        /// <summary>
        /// 程序配置接口
        /// </summary>
        private readonly IConfiguration Configuration;

        /// <summary>
        /// Razor引擎服务
        /// </summary>
        private readonly IRazorLightEngine RazorLightEngine;
  

        /// <summary>
        /// 邮件发送客户端
        /// </summary>
        private SmtpClient SmtpClient { get; set; }
        public TencentEmailBLL(IConfiguration configuration, IRazorLightEngine razorLightEngine)
        {
            this.Configuration = configuration;
            this.RazorLightEngine = razorLightEngine;
        }

        /// <summary>
        /// 发送邮箱验证码
        /// </summary>
        /// <param name="emailRegisterVerificationModel">邮箱注册验证码实体</param>
        /// <returns></returns>
        public async Task<OperateResult> SendVerificationAsync(EmailRegisterVerificationModel emailRegisterVerificationModel)
        {
            (string userName, string password, string host, int port) = GetEmailConfig();
            var messageBody = await GetMessageBody("RegisterVerification", emailRegisterVerificationModel);
            var mimeMessage = GetMimeMessage(userName, emailRegisterVerificationModel.Email, "注册验证码", messageBody);
            return await SendEmailAsync(host, port, userName, password, mimeMessage);
        }


        /// <summary>
        /// 发送密码重置链接
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="passwordResetToken">密码重置会话Token</param>
        /// <returns></returns>
        public async Task<OperateResult> SendPassResetAsync(EmailPasswordResetModel emailPasswordResetModel)
        {
            (string userName, string password, string host, int port) = GetEmailConfig();
            var messageBody = await GetMessageBody("PasswordReset", emailPasswordResetModel);
            var mimeMessage = GetMimeMessage(userName, emailPasswordResetModel.Email, "密码重置", messageBody);
            return await SendEmailAsync(host, port, userName, password, mimeMessage);
        }

        private (string userName, string password, string host, int port) GetEmailConfig()
        {
            string userName = Configuration["ConnectionStrings:EmailService:UserName"];
            string password = Configuration["ConnectionStrings:EmailService:Password"];
            string host = Configuration["ConnectionStrings:EmailService:Host"];
            int port = int.Parse(Configuration["ConnectionStrings:EmailService:Port"]);
            return (userName, password, host, port);
        }

        /// <summary>
        /// 获取邮件消息
        /// </summary>
        /// <param name="userName">发送邮件地址</param>
        /// <param name="email">接收邮件地址</param>
        /// <param name="subject">主题</param>
        /// <param name="body">邮件内容</param>
        /// <returns></returns>
        private MimeMessage GetMimeMessage(string userName, string email, string subject, MimeEntity body)
        {
            MimeMessage mimeMessage = new MimeMessage();
            mimeMessage.From.Add(new MailboxAddress(userName, userName));
            mimeMessage.To.Add(new MailboxAddress(email, email));
            mimeMessage.Subject = subject;
            mimeMessage.Body = body;
            return mimeMessage;
        }

        /// <summary>
        /// 获取邮件内容
        /// </summary>
        /// <typeparam name="T">模版实体类型</typeparam>
        /// <param name="key">模版键值</param>
        /// <param name="model">模版实体</param>
        /// <param name="viewBag">模板的动态视图包</param>
        /// <returns></returns>
        private async Task<MimeEntity> GetMessageBody<T>(string key, T model, ExpandoObject viewBag = null)
        {
            BodyBuilder bodyBuilder = new BodyBuilder();
            string htmlBody = await RazorLightEngine.CompileRenderAsync($"HtmlTemplates/{key}.cshtml", model, viewBag);
            bodyBuilder.HtmlBody = htmlBody;
            return bodyBuilder.ToMessageBody();
        }


        private async Task<OperateResult> SendEmailAsync(string host, int port, string userName, string password, MimeMessage mimeMessage)
        {

            if (this.SmtpClient == null || (this.SmtpClient != null && !this.SmtpClient.IsConnected))
            {
                this.SmtpClient = new SmtpClient { ServerCertificateValidationCallback = (s, c, h, e) => true };
                await this.SmtpClient.ConnectAsync(host, port);
                await this.SmtpClient.AuthenticateAsync(userName, password);
            }
            var sendResult = await this.SmtpClient.SendAsync(mimeMessage);
            if (sendResult != null && sendResult.StartsWith("OK"))
            {
                return OperateResult.CreateSuccessResult();
            }
            else
            {
                return new OperateResult()
                {
                    Message = sendResult
                };
            }
        }

    }
}

