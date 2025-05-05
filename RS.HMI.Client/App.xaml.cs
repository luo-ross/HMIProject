using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons.Extensions;
using RS.Widgets.Controls;
using System.Reflection;
using System.Windows;
using RS.HMI.BLL;
using RS.HMI.Client.Views;
namespace RS.HMI.Client
{
    public partial class App : ApplicationBase
    {
        /// <summary>
        /// 可以重新赋值主机地址
        /// </summary>
        public override string AppHostAddress { get; set; } = "http://www.hmiproject.com/";
      
        public App()
        {
            //配置依赖注入服务
            this.OnConfigIocServices += App_OnConfigIocServices;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginView = AppHost.Services.GetRequiredService<LoginView>();
            loginView.Show();
            //var homeView = AppHost.Services.GetRequiredService<HomeView>();
            //homeView.Show();
        }
     

        private void App_OnConfigIocServices(HostApplicationBuilder builder)
        {
            //注册当前程序集服务
            builder.Services.RegisterBLLService();
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());
        }
      
    }

}
