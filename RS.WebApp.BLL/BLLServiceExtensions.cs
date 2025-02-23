using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.WebApp.IBLL;
using RS.Commons.Extensions;
using System.Reflection;

namespace RS.WebApp.BLL
{
    /// <summary>
    /// 业务数据管理层服务扩展
    /// </summary>
    public static class BLLServiceExtensions
    {
        /// <summary>
        /// 注册业务逻辑层服务
        /// </summary>
        /// <param name="services">依赖注入服务</param>
        /// <param name="configuration">程序配置</param>
        /// <returns></returns>
        public static IServiceCollection RegisterBLLService(this IServiceCollection services, IConfiguration configuration)
        {
            //动态配置选择哪一种邮箱发送服务
            string emailService = configuration["ConnectionStrings:EmailService"];
            switch (emailService)
            {
                case "Tencent":
                    services.AddSingleton<IEmailService, TencentEmailService>();
                    break;
            }

            //动态配置选择哪一种短信发送服务
            string sMSService = configuration["ConnectionStrings:SMSService"];
            switch (sMSService)
            {
                case "Ali":
                    services.AddSingleton<ISMSService, AliSMSService>();
                    break;
                case "Huawei":
                    services.AddSingleton<ISMSService, HuaweiSMSService>();
                    break;
                case "Tencent":
                    services.AddSingleton<ISMSService, TencentSMSService>();
                    break;
            }

            //自动注册服务
            services.RegisterService(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
