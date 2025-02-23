using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    /// <summary>
    /// 服务扩展
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 服务宿主
        /// </summary>
        public static IHost AppHost { get; set; }

        /// <summary>
        /// 配置服务
        /// </summary>
        /// <param name="appHost"></param>
        public static void ConfigServices(IHost appHost)
        {
            AppHost = appHost;
        }

        /// <summary>
        /// 获取服务
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <returns></returns>
        public static TResult GetService<TResult>()
        {
            return AppHost.Services.GetService<TResult>();
        }

        public static HttpClient GetHttpClient(string clientName)
        {
           return GetService<IHttpClientFactory>().CreateClient(clientName);
        }
    }
}
