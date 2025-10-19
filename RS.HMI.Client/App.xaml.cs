using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons.Extensions;
using RS.HMI.BLL;
using RS.HMI.Client.Views;
using RS.Widgets.Controls;
using SixLabors.Fonts.Tables.AdvancedTypographic;
using System.Reflection;
using System.Security.Cryptography;
using System.Windows;
namespace RS.HMI.Client
{
    public partial class App : ApplicationBase
    {
        /// <summary>
        /// 可以重新赋值主机地址
        /// </summary>
#if DEBUG
        public override string AppHostAddress { get; set; } = "http://localhost:7000/";
        //public override string AppHostAddress { get; set; } = "http://localhost:7109/";
#else
        public override string AppHostAddress { get; set; } = "http://localhost:7000/";
#endif

        public App()
        {
            //配置依赖注入服务
            this.OnConfigIocServices += App_OnConfigIocServices;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            var loginView = App.ServiceProvider?.GetRequiredService<LoginView>();
            loginView.Show();
            //var homeView = ServiceProvider?.GetRequiredService<HomeView>();
            //homeView?.Show();
        }

        private void App_OnConfigIocServices(HostApplicationBuilder builder)
        {
            //注册当前程序集服务
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());

            builder.Services.RegisterBLLService();
        }



    }

}
