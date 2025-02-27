using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons.Extensions;
using RS.HMI.Client.Views.Home;
using RS.HMI.Client.Views.Logoin;
using RS.Widgets.Controls;
using System.Reflection;
using System.Windows;
using RS.HMI.BLL;
namespace RS.HMI.Client
{

    public partial class App : ApplicationBase
    {

        public App()
        {

        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //配置服务
            this.OnConfigServices += App_OnConfigServices;
            base.OnStartup(e);

            var loginView = AppHost.Services.GetRequiredService<LoginView>();
            loginView.Show();
            //var homeView = AppHost.Services.GetRequiredService<HomeView>();
            //homeView.Show();
        }


        private void App_OnConfigServices(HostApplicationBuilder builder)
        {
            //注册当前程序集服务
            builder.Services.RegisterBLLService();
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());
        }
      
    }

}
