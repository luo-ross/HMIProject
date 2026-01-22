using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RS.Commons;
using RS.Commons.Extensions;
using RS.WPFClient.BLL;
using RS.WPFClient.ViewModels;
using RS.WPFClient.Views;
using RS.Models;
using RS.Server.WebAPI;
using RS.Widgets;
using RS.Widgets.Interfaces;
using System.IO;
using System.Net.Http.Headers;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;
namespace RS.WPFClient
{
    public partial class App : Application
    {
        ///// <summary>
        ///// 程序ViewModel
        ///// </summary>
        //public static ApplicationViewModel ViewModel { get; private set; }

        /// <summary>
        /// 秘钥存储路径
        /// </summary>
        public static readonly string KeysRepository = "Keys-Repository";

        public static HostApplicationBuilder ApplicationBuilder;

        public static IHost ApplicationHost;

        public static IServiceProvider ServiceProvider { get; set; }

        /// <summary>
        /// 日志服务
        /// </summary>
        public ILogService LogService { get; private set; }

        /// <summary>
        /// 可以重新赋值主机地址
        /// </summary>
#if DEBUG
        public string AppHostAddress { get; set; } = "http://localhost:7000/";
        //public string AppHostAddress { get; set; } = "http://localhost:7109/";
#else
        public string AppHostAddress { get; set; } = "http://localhost:7000/";
#endif

        public App()
        {
            //配置依赖注入服务
            ApplicationBuilder = Host.CreateApplicationBuilder();

            //注入缓存服务
            ApplicationBuilder.Services.AddMemoryCache();

            //注册控件库的服务
            ApplicationBuilder.Services.RegisterWidgetsService();

            //注册通用服务
            ApplicationBuilder.Services.RegisterCommonService();

            //注册日志服务
            ApplicationBuilder.Services.RegisterLog4netService();

            //注册方法拦截服务
            ApplicationBuilder.Services.RegisterInterceptorService();

            //注册当前程序集服务
            ApplicationBuilder.Services.RegisterService(Assembly.GetExecutingAssembly());

            //注册DPAPI加解密服务
            ApplicationBuilder.Services.AddDataProtection()
            .PersistKeysToFileSystem(new DirectoryInfo(KeysRepository))
            .ProtectKeysWithDpapi()
            .SetDefaultKeyLifetime(TimeSpan.FromDays(90));

            ApplicationBuilder.Services.RegisterBLLService();

            ApplicationBuilder.Services.AddHttpClient(nameof(HMIWebAPI), (serviceProvider, configClient) =>
            {
                configClient.BaseAddress = new Uri(AppHostAddress);
                configClient.Timeout = TimeSpan.FromSeconds(30);
                var memoryCache = serviceProvider.GetService<IMemoryCache>();
                SessionModel? sessionModel = null;
                memoryCache?.TryGetValue(MemoryCacheKey.SessionModelKey, out sessionModel);
                if (sessionModel != null)
                {
                    configClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, sessionModel.Token);
                }
            });

            //必须调用Build方法
            ApplicationHost = ApplicationBuilder.Build();

            ServiceProvider = ApplicationHost.Services;

            //开始异步执行
            ApplicationHost.RunAsync();


            //配置全局服务这样在其他程序集也可以访问服务
            ServiceProviderExtensions.ConfigServices(ServiceProvider);

            // 获取日志服务
            LogService = ServiceProvider.GetRequiredService<ILogService>();

            //程序未处理异常
            this.RegisterUnknowExceptionsHandler();
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // 获取Window服务
            var windowService = ServiceProvider.GetRequiredService<IWindowService>();
            //windowService.ShowAsync<LoginViewModel,LoginView>();
            windowService.ShowAsync<HomeViewModel, HomeView>();

        }


        /// <summary>
        /// 初始化RSA非对称秘钥数据
        /// </summary>
        private void InitRSASecurityKeyData()
        {
            //var cryptographyBLL = ServiceProvider.GetRequiredService<ICryptographyBLL>();
            //var memoryCache = ServiceProvider.GetRequiredService<IMemoryCache>();
            ////如果是第一就会创建公钥和私钥
            //(byte[] rsaSigningPrivateKey, byte[] rsaSigningPublicKey) = cryptographyBLL.GenerateRSAKey();
            //(byte[] rsaEncryptionPrivateKey, byte[] rsaEncryptionPublicKey) = cryptographyBLL.GenerateRSAKey();
            //memoryCache.Set<string>(MemoryCacheKey.GlobalRSASignPublicKey, Convert.ToBase64String(rsaSigningPublicKey));
            //memoryCache.Set<byte[]>(MemoryCacheKey.GlobalRSASignPrivateKey, rsaSigningPrivateKey);
            //memoryCache.Set<string>(MemoryCacheKey.GlobalRSAEncryptPublicKey, Convert.ToBase64String(rsaEncryptionPublicKey));
            //memoryCache.Set<byte[]>(MemoryCacheKey.GlobalRSAEncryptPrivateKey, rsaEncryptionPrivateKey);
        }


        /// <summary>
        /// 和服务端连接成功时触发事件
        /// </summary>
        private async void ApplicationBase_OnServerConnect()
        {
            ////如果未和服务端创建会话
            //if (!ViewModel.IsGetSessionModelSuccess)
            //{
            //    //从服务端获取会话Token和数据交换密钥
            //    var getSessionModelResult = await this.GetSessionModelAsync();
            //    if (!getSessionModelResult.IsSuccess)
            //    {
            //        ViewModel.IsGetSessionModelSuccess = false;
            //        return;
            //    }
            //    ViewModel.IsGetSessionModelSuccess = true;
            //}
        }


        /// <summary>
        /// 启动心跳检测
        /// </summary>
        private void StartHeartbeatCheckAsync()
        {
            //heartbeatThread = new Thread(async () =>
            //{
            //    while (!HeartbeatCancellation.Token.IsCancellationRequested)
            //    {
            //        if (!NetworkInterface.GetIsNetworkAvailable())
            //        {
            //            ViewModel.IsNetworkAvailable = false;
            //            return;
            //        }

            //        ViewModel.IsNetworkAvailable = true;

            //        //这里还需要再处理一下
            //        var heartBeatCheckResult = await HMIWebAPI.General.HeartBeatCheck.HttpGetAsync(nameof(HMIWebAPI));
            //        if (heartBeatCheckResult.IsSuccess)
            //        {
            //            ViewModel.IsServerConnectSuccess = true;
            //        }
            //        else
            //        {
            //            // 检查是否有可用的网络连接
            //            ViewModel.IsServerConnectSuccess = false;
            //        }

            //        try
            //        {
            //            await Task.Delay(this.HeartbeatInterval, HeartbeatCancellation.Token);
            //        }
            //        catch (TaskCanceledException)
            //        {

            //        }
            //    }
            //})
            //{
            //    IsBackground = true
            //};

            //heartbeatThread.Start();
        }

        ///// <summary>
        ///// 创建会话
        ///// </summary>
        ///// <returns></returns>
        //private async Task<OperateResult> GetSessionModelAsync()
        //{
        //    //获取客户端加密公钥
        //    MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAEncryptPublicKey, out string globalRSAEncryptPublicKey);
        //    if (string.IsNullOrEmpty(globalRSAEncryptPublicKey))
        //    {
        //        return OperateResult.CreateFailResult("获取客户端加密公钥失败！");
        //    }

        //    //获取客户端解密私钥
        //    MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAEncryptPrivateKey, out byte[] globalRSAEncryptPrivateKey);
        //    if (globalRSAEncryptPrivateKey == null)
        //    {
        //        return OperateResult.CreateFailResult("获取客户端加密私钥失败！");
        //    }


        //    //获取客户端签名公钥
        //    MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSASignPublicKey, out string globalRSASignPublicKey);
        //    if (string.IsNullOrEmpty(globalRSASignPublicKey))
        //    {
        //        return OperateResult.CreateFailResult("获取客户端签名公钥失败！");
        //    }


        //    //创建会话请求
        //    SessionRequestModel sessionRequestModel = new SessionRequestModel()
        //    {
        //        RSAEncryptPublicKey = globalRSAEncryptPublicKey,
        //        RSASignPublicKey = globalRSASignPublicKey,
        //        Nonce = CryptographyBLL.CreateRandCode(10),
        //        TimeStamp = DateTime.UtcNow.ToTimeStampString(),
        //        AudiencesType = AudiencesType.Windows,
        //    };

        //    //数据按照顺序组成数组
        //    ArrayList arrayList = new ArrayList
        //    {
        //        sessionRequestModel.RSASignPublicKey,
        //        sessionRequestModel.RSAEncryptPublicKey,
        //        sessionRequestModel.TimeStamp,
        //        sessionRequestModel.Nonce
        //    };

        //    //获取会话的Hash数据
        //    var getRSAHashResult = CryptographyBLL.GetRSAHash(arrayList);
        //    if (!getRSAHashResult.IsSuccess)
        //    {
        //        return getRSAHashResult;
        //    }

        //    //获取客户端签名私钥
        //    MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSASignPrivateKey, out byte[]? globalRSASignPrivateKey);
        //    if (globalRSASignPrivateKey == null || globalRSASignPrivateKey.Length == 0)
        //    {
        //        return OperateResult.CreateFailResult("获取客户端私钥失败！");
        //    }

        //    //进行RSA数据签名
        //    var rsaSignDataResult = CryptographyBLL.RSASignData(getRSAHashResult.Data, globalRSASignPrivateKey);
        //    if (!rsaSignDataResult.IsSuccess)
        //    {
        //        return rsaSignDataResult;
        //    }
        //    sessionRequestModel.MsgSignature = rsaSignDataResult.Data;

        //    //往服务端发送数据 并获取回传数据
        //    var aesEncryptModelResult = await HMIWebAPI.General.GetSessionModel.HttpPostAsync<SessionRequestModel, SessionResultModel>(sessionRequestModel, nameof(HMIWebAPI));
        //    if (!aesEncryptModelResult.IsSuccess)
        //    {
        //        return aesEncryptModelResult;
        //    }
        //    var sessionResultModel = aesEncryptModelResult.Data;

        //    //数据按照顺序组成数组
        //    arrayList = new ArrayList
        //    {
        //        sessionResultModel.SessionModel.AesKey,
        //        sessionResultModel.SessionModel.Token,
        //        sessionResultModel.SessionModel.AppId,
        //        sessionResultModel.RSASignPublicKey,
        //        sessionResultModel.RSAEncryptPublicKey,
        //        sessionResultModel.TimeStamp,
        //        sessionResultModel.Nonce
        //    };

        //    //获取会话的Hash数据
        //    getRSAHashResult = CryptographyBLL.GetRSAHash(arrayList);
        //    if (!getRSAHashResult.IsSuccess)
        //    {
        //        return getRSAHashResult;
        //    }

        //    //获取签名
        //    var signature = Convert.FromBase64String(sessionResultModel.MsgSignature);
        //    var verifyDataResult = CryptographyBLL.RSAVerifyData(getRSAHashResult.Data, signature, sessionResultModel.RSASignPublicKey);

        //    if (!verifyDataResult.IsSuccess)
        //    {
        //        return verifyDataResult;
        //    }

        //    //解密AesKey
        //    var rsaDecryptResult = CryptographyBLL.RSADecrypt(sessionResultModel.SessionModel.AesKey, globalRSAEncryptPrivateKey);
        //    if (!rsaDecryptResult.IsSuccess)
        //    {
        //        return rsaDecryptResult;
        //    }
        //    sessionResultModel.SessionModel.AesKey = rsaDecryptResult.Data;

        //    //解密AppId
        //    rsaDecryptResult = CryptographyBLL.RSADecrypt(sessionResultModel.SessionModel.AppId, globalRSAEncryptPrivateKey);
        //    if (!rsaDecryptResult.IsSuccess)
        //    {
        //        return rsaDecryptResult;
        //    }
        //    sessionResultModel.SessionModel.AppId = rsaDecryptResult.Data;

        //    //把会话数据存储在缓存里
        //    MemoryCache.Set(MemoryCacheKey.SessionModelKey, sessionResultModel.SessionModel);

        //    //将服务端公钥存储在缓存里
        //    MemoryCache.Set(MemoryCacheKey.ServerGlobalRSASignPublicKey, sessionResultModel.RSASignPublicKey);
        //    MemoryCache.Set(MemoryCacheKey.ServerGlobalRSAEncryptPublicKey, sessionResultModel.RSAEncryptPublicKey);

        //    return OperateResult.CreateSuccessResult();
        //}


        #region 处理系统未知异常
        private void RegisterUnknowExceptionsHandler()
        {
#if RELEASE
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
#endif
        }

        private void App_DispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            LogService.LogCritical(e.Exception, "App_DispatcherUnhandledException");
            e.Handled = true;
        }


        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception ex)
            {
                LogService.LogCritical(ex, "CurrentDomain_UnhandledException");
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            LogService.LogCritical(e.Exception, "TaskScheduler_UnobservedTaskException");
        }
        #endregion


    }

}
