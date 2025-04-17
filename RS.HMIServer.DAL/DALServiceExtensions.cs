using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using RS.HMIServer.DAL.SqlServer;
using RS.Commons.Extensions;
using System.Reflection;

namespace RS.HMIServer.DAL
{
    /// <summary>
    /// 数据管理层服务扩展
    /// </summary>
    public static class DALServiceExtensions
    {
       /// <summary>
       /// 注册数据管理层服务
       /// </summary>
       /// <param name="services">依赖入住服务</param>
       /// <param name="configuration">程序配置</param>
       /// <returns></returns>
        public static IServiceCollection RegisterDALService(this IServiceCollection services,IConfiguration configuration)
        {
            //注册SQL Server服务
            services.AddDbContext<RSAppDbContext>(optionsAction =>
            {
                string connectionString = configuration["ConnectionStrings:RSAppSqlSever"];
                var sqlConnectionStringBuilder = new SqlConnectionStringBuilder(connectionString);
                optionsAction.UseSqlServer(sqlConnectionStringBuilder.ConnectionString);
            });

            //自动注册服务
            services.RegisterService(Assembly.GetExecutingAssembly());
            return services;
        }
    }
}
