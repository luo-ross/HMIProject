﻿using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons.Extensions;
using RS.Widgets.Controls;
using RS.Annotation.BLL;
using RS.Annotation.Views;
using RS.Annotation.Views.Home;
using System.Reflection;
using System.Windows;
using IdGen;

namespace RS.Annotation
{
    public partial class App : ApplicationBase
    {

        /// <summary>
        /// 可以重新赋值主机地址
        /// </summary>
        public override string AppHostAddress { get; set; } = "http://localhost:7000/";

        /// <summary>
        /// 程序入口
        /// </summary>
        public App()
        {
         
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            //配置服务
            this.OnConfigIocServices += App_OnConfigServices;
            base.OnStartup(e);
            //var loginView = AppHost.Services.GetRequiredService<LoginView>();
            //loginView.Show();
            var homeView = App.ServiceProvider?.GetRequiredService<HomeView>();
            homeView.Show();
            //var singnalChartView = AppHost.Services.GetRequiredService<SingnalChartView>();
            //singnalChartView.Show();
        }

        private void App_OnConfigServices(HostApplicationBuilder builder)
        {
            builder.Services.AddSingleton<IIdGenerator<long>>(service =>
            {
                int generatorId = Convert.ToInt32(builder.Configuration["IdGenClientId"]);
                return new IdGenerator(generatorId, IdGeneratorOptions.Default);
            });
            //注册业务逻辑服务
            builder.Services.RegisterBLLService();
            //注册当前程序集服务
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());
        }


    }
}
