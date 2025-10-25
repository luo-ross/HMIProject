using MailKit.Net.Smtp;
using Microsoft.Extensions.Configuration;
using MimeKit;
using RazorLight;
using RS.Commons;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;
using System.Dynamic;
using System.IO.Pipelines;

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
        /// <param name="emailRegisterVerifyModel">邮箱注册验证码实体</param>
        /// <returns></returns>
        public async Task<OperateResult> SendVerifyAsync(EmailRegisterVerifyModel emailRegisterVerifyModel)
        {
            (string userName, string password, string host, int port) = GetEmailConfig();
            var messageBody = await GetMessageBody("RegisterVerify", emailRegisterVerifyModel);
            var mimeMessage = GetMimeMessage(userName, emailRegisterVerifyModel.Email, "注册验证码", messageBody);
            return await SendEmailAsync(host, port, userName, password, mimeMessage);
        }


        /// <summary>
        /// 发送密码重置链接
        /// </summary>
        /// <param name="email">邮箱地址</param>
        /// <param name="passwordResetToken">密码重置会话Token</param>
        /// <returns></returns>
        public async Task<OperateResult> SendPassResetAsync(EmailSecurityModel emailSecurityModel)
        {
            (string userName, string password, string host, int port) = GetEmailConfig();
            var messageBody = await GetMessageBody("PasswordReset", emailSecurityModel);
            var mimeMessage = GetMimeMessage(userName, emailSecurityModel.Email, "密码重置", messageBody);
            return await SendEmailAsync(host, port, userName, password, mimeMessage);
        }

        private (string userName, string password, string host, int port) GetEmailConfig()
        {
            string userName = Configuration["EmailService:UserName"];
            string password = Configuration["EmailService:Password"];
            string host = Configuration["EmailService:Host"];
            int port = int.Parse(Configuration["EmailService:Port"]);
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
            string? sendResult = null;
            int tryTime = 0;
        ILResend:
            try
            {
                sendResult = await this.SmtpClient.SendAsync(mimeMessage);
            }
            catch (Exception)
            {
                //尝试3次 如果还是失败
                if (tryTime < 3)
                {
                    tryTime++;
                    goto ILResend;
                }
            }

            if (string.IsNullOrEmpty(sendResult) || sendResult.StartsWith("OK"))
            {
                return OperateResult.CreateFailResult("邮件发送失败，请稍后再试");
            }

            return OperateResult.CreateSuccessResult();
        }

    }
}

