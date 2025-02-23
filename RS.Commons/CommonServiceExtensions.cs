using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Extensions;
using System.Reflection;

namespace RS.Commons
{
    public static class CommonServiceExtensions
    {
        public static IServiceCollection RegisterCommonService(this IServiceCollection services)
        {
            services.RegisterService(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
