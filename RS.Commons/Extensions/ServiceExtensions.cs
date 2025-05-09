using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    /// <summary>
    /// 服务扩展 就是方便全局获取服务
    /// </summary>
    public static class ServiceProviderExtensions
    {
        /// <summary>
        /// 服务宿主
        /// </summary>
        private static IHost AppHost { get; set; }

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
            if (AppHost == null)
            {
                return default(TResult);
            }

            return AppHost.Services.GetService<TResult>();
        }

        public static HttpClient GetHttpClient(string clientName,string token)
        {
            var httpClient = GetService<IHttpClientFactory>().CreateClient(clientName);
            if (!string.IsNullOrEmpty(token))
            {
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, token);
            }
            return httpClient;
        }
      
    }
}
