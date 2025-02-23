using Microsoft.Extensions.DependencyInjection;
using RS.WPFApp.IBLL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using RS.Commons.Extensions;

namespace RS.WPFApp.BLL
{
    /// <summary>
    /// 业务逻辑层服务扩展类
    /// </summary>
    public static class GenBLLServiceExtensions
    {
        /// <summary>
        /// 注册业务逻辑层服务
        /// </summary>
        /// <param name="services">依赖注入服务</param>
        /// <returns></returns>
        public static IServiceCollection RegisterBLLService(this IServiceCollection services)
        {
            services.RegisterService(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
