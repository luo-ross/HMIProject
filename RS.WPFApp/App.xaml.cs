using IdGen;
using IdGen.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons;
using RS.Commons.Extensions;
using RS.Commons.Interceptors;
using RS.Models;
using RS.RESTfulApi;
using RS.WPFApp.BLL;
using RS.WPFApp.Views;
using RS.WPFApp.Views.Charts;
using RS.WPFApp.Views.Home;
using System;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace RS.WPFApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        /// <summary>
        /// 服务宿主
        /// </summary>
        public static IHost AppHost;
        /// <summary>
        /// 日志服务
        /// </summary>
        public static ILogService LogService;
        /// <summary>
        /// 秘钥存储路径
        /// </summary>
        private static readonly string KeysRepository = "Keys-Repository";

        /// <summary>
        /// 主机地址
        /// </summary>
        internal const string HostAddress = "http://localhost:9000";
        //internal const string HostAddress = "http://localhost:7109";

        /// <summary>
        /// 程序入口
        /// </summary>
        public App()
        {
            this.ConfigServices();
            this.Startup += App_Startup;
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            base.OnStartup(e);
        }
     


        #region 系统事件
        private async void App_Startup(object sender, StartupEventArgs e)
        {
            //处理系统异常
            RegisterUnknowExceptionsHandler();
            //var loginView = AppHost.Services.GetRequiredService<LoginView>();
            //loginView.Show();
            var homeView = AppHost.Services.GetRequiredService<HomeView>();
            homeView.Show();

            //var singnalChartView = AppHost.Services.GetRequiredService<SingnalChartView>();
            //singnalChartView.Show();
        }
        #endregion

        #region 配置依赖服务
        private void ConfigServices()
        {
            var builder = Host.CreateApplicationBuilder();
            //注册Id生成器
            builder.Services.AddIdGen(123, () => new IdGeneratorOptions());
            //注册DPAPI加解密服务
            builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(KeysRepository))
              .ProtectKeysWithDpapi()
              .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
            //注入缓存服务
            builder.Services.AddMemoryCache();
            //注册通用服务
            builder.Services.RegisterCommonService();
            //注册日志服务
            builder.Services.RegisterLog4netService();
            //注册方法拦截服务
            builder.Services.RegisterInterceptorService();
            #region 注册依赖服务
            //注册WebClient客户端
            builder.Services.AddHttpClient(nameof(RSAppAPI), (serviceProvider, configClient) =>
            {
                configClient.BaseAddress = new Uri(HostAddress);
                configClient.Timeout = TimeSpan.FromSeconds(30);
                var memoryCache = serviceProvider.GetService<IMemoryCache>();
                memoryCache.TryGetValue(MemoryCacheKey.SessionModelKey, out SessionModel sessionModel);
                if (sessionModel != null)
                {
                    configClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, sessionModel.Token);
                }
            });
            //注册业务逻辑服务
            builder.Services.RegisterBLLService();
            //注册当前程序集服务
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());
            #endregion
            AppHost = builder.Build();
            //这个必须放在builder.Build()后才生效
            ServiceProviderExtensions.ConfigServices(AppHost);

            //启动日志后台线程服务
            LogService = AppHost.Services.GetRequiredService<ILogService>();
            LogService.InitLogService();

            //初始化RSA 秘钥
            InitRSASecurityKeyData();
            AppHost.RunAsync();
        }
        #endregion

        /// <summary>
        /// 初始化RSA非对称秘钥数据
        /// </summary>
        public static void InitRSASecurityKeyData()
        {
            var cryptographyService = AppHost.Services.GetRequiredService<ICryptographyService>();
            var memoryCache = AppHost.Services.GetRequiredService<IMemoryCache>();
            //如果是第一就会创建公钥和私钥
            (byte[] rsaPrivateKey, byte[] rsaPublicKey) = cryptographyService.GenerateRSAKey();
            memoryCache.Set<string>(MemoryCacheKey.GlobalRSAPublicKey, Convert.ToBase64String(rsaPublicKey));
            memoryCache.Set<byte[]>(MemoryCacheKey.GlobalRSAPrivateKey, rsaPrivateKey);
        }

        #region 处理系统未知异常
        private void RegisterUnknowExceptionsHandler()
        {
#if RELEASE
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
#endif
        }
        //主线程未处理异常
        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogService.LogCritical(e.Exception, "App_DispatcherUnhandledException");
            e.Handled = true;
        }

        //未处理线程异常(如果主线程未处理异常已经处理，该异常不会触发)
        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogService.LogCritical(ex, "CurrentDomain_UnhandledException");
            }
        }

        //未处理的Task内异常
        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogService.LogCritical(e.Exception, "TaskScheduler_UnobservedTaskException");
        }
        #endregion
    }
}
