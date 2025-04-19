using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.IBLL;
using RS.Commons.Extensions;
using System.Reflection;

namespace RS.HMIServer.BLL
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
            string emailBLL = configuration["EmailService"];
            switch (emailBLL)
            {
                case "Tencent":
                    services.AddSingleton<IEmailBLL, TencentEmailBLL>();
                    break;
            }

            //动态配置选择哪一种短信发送服务
            string sMSService = configuration["SMSService"];
            switch (sMSService)
            {
                case "Ali":
                    services.AddSingleton<ISMSBLL, AliSMSBLL>();
                    break;
                case "Huawei":
                    services.AddSingleton<ISMSBLL, HuaweiSMSBLL>();
                    break;
                case "Tencent":
                    services.AddSingleton<ISMSBLL, TencentSMSBLL>();
                    break;
            }

            //自动注册服务
            services.RegisterService(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
