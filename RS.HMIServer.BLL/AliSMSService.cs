using AlibabaCloud.OpenApiClient.Models;
using AlibabaCloud.SDK.Dysmsapi20180501;
using AlibabaCloud.SDK.Dysmsapi20180501.Models;
using Microsoft.Extensions.Configuration;
using RS.HMIServer.IBLL;
using RS.Commons;
using RS.Commons.Extensions;

namespace RS.HMIServer.BLL
{
    /// <summary>
    /// 阿里云短信发送服务
    /// </summary>
    internal class AliSMSService : ISMSService
    {
        private readonly IConfiguration Configuration;
        public AliSMSService(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // 使用AK&SK初始化账号Client 
        private Client CreateDysmsapiClient(string endpoint)
        {
            string accessKeyId = Configuration["ConnectionStrings:SMSConfig:AccessKeyId"];
            string accessKeySecret = Configuration["ConnectionStrings:SMSConfig:AccessKeySecret"];

            Config config = new Config
            {
                AccessKeyId = accessKeyId,
                AccessKeySecret = accessKeySecret,
            };
            // Endpoint 请参考 https://api.aliyun.com/product/Dysmsapi
            //config.Endpoint = "dysmsapi.ap-southeast-1.aliyuncs.com";
            config.Endpoint = endpoint;
            return new Client(config);
        }

        /// <summary>
        /// 同步发送短信验证码
        /// </summary>
        /// <param name="client"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="templateCode"></param>
        /// <param name="templateParam"></param>
        /// <param name="smsUpExtendCode"></param>
        /// <returns></returns>
        private OperateResult<SendMessageWithTemplateResponse> SendMessageWithTemplate(Client client, string to, string from, string templateCode, string templateParam, string smsUpExtendCode)
        {
            SendMessageWithTemplateRequest req = new SendMessageWithTemplateRequest
            {
                To = to,
                From = from,
                TemplateCode = templateCode,
                TemplateParam = templateParam,
                SmsUpExtendCode = smsUpExtendCode,
            };
            SendMessageWithTemplateResponse resp = client.SendMessageWithTemplate(req);
            return OperateResult.CreateSuccessResult(resp);
        }

        /// <summary>
        /// 异步返送短信验证码
        /// </summary>
        /// <param name="client"></param>
        /// <param name="to"></param>
        /// <param name="from"></param>
        /// <param name="templateCode"></param>
        /// <param name="templateParam"></param>
        /// <param name="smsUpExtendCode"></param>
        /// <returns></returns>
        private async Task<OperateResult<SendMessageWithTemplateResponse>> SendMessageWithTemplateAsync(Client client, string to, string from, string templateCode, string templateParam, string smsUpExtendCode)
        {
            SendMessageWithTemplateRequest req = new SendMessageWithTemplateRequest
            {
                To = to,
                From = from,
                TemplateCode = templateCode,
                TemplateParam = templateParam,
                SmsUpExtendCode = smsUpExtendCode,
            };
            SendMessageWithTemplateResponse resp = await client.SendMessageWithTemplateAsync(req);
            return OperateResult.CreateSuccessResult(resp);
        }



        /// <summary>
        /// 发送注册短信验证码
        /// </summary>
        /// <param name="countryCode">国家区号</param>
        /// <param name="phone">电话号码</param>
        /// <param name="verification">验证码</param>
        /// <returns></returns>
        public async Task<OperateResult> SendRegisterVerificationAsync(string countryCode, string phone, int verification)
        {
            // 接收短信号码。号码格式为:国际区号+号码。例如:861503871****。
            string to = $"{countryCode}{phone}";
            // 发送方标识。发往中国传入签名,请在控制台申请短信签名;发往非中国地区传入senderId。
            // 国内短信无需填写该项；国际/港澳台短信已申请独立 SenderId 需要填写该字段，默认使用公共 SenderId，无需填写该字段。注：月度使用量达到指定量级可申请独立 SenderId 使用
            string from = "发往非中国地区传入senderId";
            // 模板code
            string templateCode = "templateCode";
            // 短信模板变量对应的实际值,参数格式为JSON格式。如果模板中存在变量,该参数为必填项。例如:{"name":"xd","value":"hello"}
            string templateParam = new
            {
                Code = $"{verification}"
            }.ToJson();


            // 上行短信扩展码 无需可以忽略
            string smsUpExtendCode = "smsUpExtendCode";

            //这个endPoint可以根据实际业务 通过获取地址位置动态判断该往哪个地址发送
            string endPoint = "dysmsapi.aliyuncs.com";

            //这里每次都创建 性能还需验证
            Client client = CreateDysmsapiClient(endPoint);

            //这里我们可以根据实际调试的结果返回记录日志 这里没有去注册阿里云短信实际
            //调用是不知道返回是什么 需要具体测试
            var sendMessageWithTemplateResult = await SendMessageWithTemplateAsync(client, to, from, templateCode, templateParam, smsUpExtendCode);
            if (!sendMessageWithTemplateResult.IsSuccess)
            {
                return sendMessageWithTemplateResult;
            }
            return OperateResult.CreateSuccessResult();
        }
    }
}
