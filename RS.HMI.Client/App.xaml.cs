using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons.Extensions;
using RS.Widgets.Controls;
using System.Reflection;
using System.Windows;
using RS.HMI.BLL;
using RS.HMI.Client.Views;
using IdGen;
using Microsoft.Extensions.Configuration;
using System.IO;
using RS.HMI.Client.Models;
using System.Windows.Forms.Design;
using System.ComponentModel;
using RS.Widgets.Interfaces;
using RS.Widgets.Models;
namespace RS.HMI.Client
{
    public partial class App : ApplicationBase
    {
        /// <summary>
        /// 可以重新赋值主机地址
        /// </summary>
        /// 
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
            //var loginView = App.ServiceProvider?.GetRequiredService<LoginView>();
            //loginView.Show();
            var homeView = ServiceProvider?.GetRequiredService<HomeView>();
            homeView?.Show();
        }

        private void App_OnConfigIocServices(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IIdGenerator<long>>(service =>
            {
                int generatorId = Convert.ToInt32(builder.Configuration["IdGenClientId"]);
                return new IdGenerator(generatorId, IdGeneratorOptions.Default);
            });
          
            //注册当前程序集服务
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());

            builder.Services.RegisterBLLService();
        }



    }

}
