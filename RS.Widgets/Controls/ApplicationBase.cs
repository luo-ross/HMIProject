using IdGen;
using IdGen.DependencyInjection;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTK.Compute.OpenCL;
using RS.Commons;
using RS.Commons.Extensions;
using RS.Models;
using RS.RESTfulApi;
using RS.Widgets.Enums;
using RS.Widgets.Models;
using System.Collections;
using System.ComponentModel;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.NetworkInformation;
using System.Reflection;
using System.Windows;
using System.Windows.Threading;

namespace RS.Widgets.Controls
{
    public class ApplicationBase : Application
    {
        /// <summary>
        /// 服务宿主
        /// </summary>
        public static IHost AppHost { get; set; }

        /// <summary>
        /// 主机地址
        /// </summary>
        public static string AppHostAddress = "http://localhost:9000";

        /// <summary>
        /// 秘钥存储路径
        /// </summary>
        private string KeysRepository = "Keys-Repository";

        /// <summary>
        /// 程序ViewModel
        /// </summary>
        public static ApplicationViewModel ViewModel;

        /// <summary>
        /// 服务器连接是否成功
        /// </summary>
        public static RSWinInfoBar RSWinInfoBar;

        #region 事件处理
        /// <summary>
        /// 配置自定义依赖注入服务事件
        /// </summary>
        public event Action<HostApplicationBuilder> OnConfigIocServices;

        /// <summary>
        /// 服务端断开连接触发事件
        /// </summary>
        public event Action OnServerDisconnect;

        /// <summary>
        /// 服务连接成功触发事件
        /// </summary>
        public event Action OnServerConnect;
        #endregion

        #region 依赖注入服务
        /// <summary>
        /// 日志服务
        /// </summary>
        private ILogService LogService;
        /// <summary>
        /// 缓存服务
        /// </summary>
        private IMemoryCache MemoryCache;
        /// <summary>
        /// 加密服务
        /// </summary>
        private ICryptographyService CryptographyService;
        #endregion


        #region 心跳检测
        /// <summary>
        /// 心跳检测间隔（毫秒）
        /// </summary>
        private const int HeartbeatInterval = 1000;

        /// <summary>
        /// 心跳检测线程
        /// </summary>
        private Thread heartbeatThread;

        /// <summary>
        /// 心跳检测取消标记
        /// </summary>
        private CancellationTokenSource heartbeatCancellation = new CancellationTokenSource();

        #endregion

        /// <summary>
        /// 静态构造方法
        /// </summary>
        static ApplicationBase()
        {
            ViewModel = new ApplicationViewModel();
        }

        /// <summary>
        /// 默认构造方法
        /// </summary>
        public ApplicationBase()
        {
            this.OnServerConnect += ApplicationBase_OnServerConnect;
        }

        /// <summary>
        /// 和服务端连接成功时触发事件
        /// </summary>
        private async void ApplicationBase_OnServerConnect()
        {
            if (!ViewModel.IsGetSessionModelSuccess)
            {
                //从服务端获取会话Token和数据交换密钥
                var getSessionModelResult = await this.GetSessionModelAsync();
                if (!getSessionModelResult.IsSuccess)
                {
                    ViewModel.IsGetSessionModelSuccess = false;
                    RSWinInfoBar.ShowInfoAsync(getSessionModelResult.Message, InfoType.Error);
                }
                ViewModel.IsGetSessionModelSuccess = true;
#if DEBUG
                RSWinInfoBar.ShowInfoAsync("成功创建会话", InfoType.Information);
#endif
            }
        }


        /// <summary>
        /// 启动心跳检测
        /// </summary>
        private void StartHeartbeatCheckAsync()
        {
            heartbeatThread = new Thread(async () =>
            {
                while (!heartbeatCancellation.Token.IsCancellationRequested)
                {
                    if (!NetworkInterface.GetIsNetworkAvailable())
                    {
                        ViewModel.IsNetworkAvailable = false;
                        OnServerDisconnect?.Invoke();
                        return;
                    }

                    ViewModel.IsNetworkAvailable = true;

                    //这里还需要再处理一下
                    var heartBeatCheckResult = await RSAppAPI.General.HeartBeatCheck.HttpGetAsync(nameof(RSAppAPI));
                    if (heartBeatCheckResult.IsSuccess)
                    {
                        ViewModel.IsServerConnectSuccess = true;
                        OnServerConnect();
                    }
                    else
                    {
                        // 检查是否有可用的网络连接
                        ViewModel.IsServerConnectSuccess = false;
                        OnServerDisconnect?.Invoke();
                    }

                    try
                    {
                        await Task.Delay(HeartbeatInterval, heartbeatCancellation.Token);
                    }
                    catch (TaskCanceledException)
                    {

                    }
                }
            })
            {
                IsBackground = true
            };

            heartbeatThread.Start();
        }

        /// <summary>
        /// 创建会话
        /// </summary>
        /// <returns></returns>
        private async Task<OperateResult> GetSessionModelAsync()
        {
            //获取客户端公钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAPublicKey, out string rSAPublicKey);
            if (string.IsNullOrEmpty(rSAPublicKey))
            {
                return OperateResult.CreateFailResult("获取客户端公钥失败！");
            }


            //创建会话请求
            SessionRequestModel sessionRequestModel = new SessionRequestModel()
            {
                RsaPublicKey = rSAPublicKey,
                Nonce = CryptographyService.CreateRandCode(10),
                TimeStamp = DateTime.UtcNow.ToTimeStampString(),
                AudienceType = AudienceType.WindowsAudience,
            };

            //数据按照顺序组成数组
            ArrayList arrayList = new ArrayList
            {
                sessionRequestModel.RsaPublicKey,
                sessionRequestModel.TimeStamp,
                sessionRequestModel.Nonce
            };

            //获取会话的Hash数据
            var getRSAHashResult = CryptographyService.GetRSAHash(arrayList);
            if (!getRSAHashResult.IsSuccess)
            {
                return getRSAHashResult;
            }

            //获取客户端私钥
            MemoryCache.TryGetValue(MemoryCacheKey.GlobalRSAPrivateKey, out byte[]? rsaPrivateKey);
            if (rsaPrivateKey == null || rsaPrivateKey.Length == 0)
            {
                return OperateResult.CreateFailResult("获取客户端私钥失败！");
            }

            //进行RSA数据签名
            var rsaSignDataResult = CryptographyService.RSASignData(getRSAHashResult.Data, rsaPrivateKey);
            if (!rsaSignDataResult.IsSuccess)
            {
                return rsaSignDataResult;
            }
            sessionRequestModel.MsgSignature = rsaSignDataResult.Data;

            //往服务端发送数据 并获取回传数据
            var aesEncryptModelResult = await RSAppAPI.General.GetSessionModel.HttpPostAsync<SessionRequestModel, SessionResultModel>(nameof(RSAppAPI), sessionRequestModel);
            if (!aesEncryptModelResult.IsSuccess)
            {
                return aesEncryptModelResult;
            }
            var sessionResultModel = aesEncryptModelResult.Data;

            //数据按照顺序组成数组
            arrayList = new ArrayList
            {
                sessionResultModel.SessionModel.AesKey,
                sessionResultModel.SessionModel.Token,
                sessionResultModel.SessionModel.AppId,
                sessionResultModel.RsaPublicKey,
                sessionResultModel.TimeStamp,
                sessionResultModel.Nonce
            };

            //获取会话的Hash数据
            getRSAHashResult = CryptographyService.GetRSAHash(arrayList);
            if (!getRSAHashResult.IsSuccess)
            {
                return getRSAHashResult;
            }

            //获取签名
            var signature = Convert.FromBase64String(sessionResultModel.MsgSignature);
            var verifyDataResult = CryptographyService.RSAVerifyData(getRSAHashResult.Data, signature, sessionResultModel.RsaPublicKey);

            if (!verifyDataResult.IsSuccess)
            {
                return verifyDataResult;
            }

            //解密AesKey
            var rsaDecryptResult = CryptographyService.RSADecrypt(sessionResultModel.SessionModel.AesKey, rsaPrivateKey);
            if (!rsaDecryptResult.IsSuccess)
            {
                return rsaDecryptResult;
            }
            sessionResultModel.SessionModel.AesKey = rsaDecryptResult.Data;

            //解密AppId
            rsaDecryptResult = CryptographyService.RSADecrypt(sessionResultModel.SessionModel.AppId, rsaPrivateKey);
            if (!rsaDecryptResult.IsSuccess)
            {
                return rsaDecryptResult;
            }
            sessionResultModel.SessionModel.AppId = rsaDecryptResult.Data;

            //把会话数据存储在缓存里
            MemoryCache.Set(MemoryCacheKey.SessionModelKey, sessionResultModel.SessionModel);

            //将服务端公钥存储在缓存里
            MemoryCache.Set(MemoryCacheKey.GlobalRSAPublicKey, sessionResultModel.RsaPublicKey);

            return OperateResult.CreateSuccessResult();
        }

        /// <summary>
        /// 配置依赖注入服务
        /// </summary>
        private void ConfigIocServices()
        {
            var builder = Host.CreateApplicationBuilder();
            //注入缓存服务
            builder.Services.AddMemoryCache();
            //注册通用服务
            builder.Services.RegisterCommonService();
            //注册日志服务
            builder.Services.RegisterLog4netService();
            //注册方法拦截服务
            builder.Services.RegisterInterceptorService();
            //注册当前程序集服务
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());
            //注册Id生成器
            builder.Services.AddIdGen(1, () => new IdGeneratorOptions());
            //注册DPAPI加解密服务
            builder.Services.AddDataProtection().PersistKeysToFileSystem(new DirectoryInfo(KeysRepository))
              .ProtectKeysWithDpapi()
              .SetDefaultKeyLifetime(TimeSpan.FromDays(90));
            //注册WebClient客户端
            builder.Services.AddHttpClient(nameof(RSAppAPI), (serviceProvider, configClient) =>
            {
                //在这里统一注册WebApi服务地址
                configClient.BaseAddress = new Uri(AppHostAddress);
                configClient.Timeout = TimeSpan.FromSeconds(30);
                //这里如果从缓存里获取不到从服务端的Token,那么用户也是无法使用WebApi的
                var memoryCache = serviceProvider.GetService<IMemoryCache>();
                SessionModel? sessionModel = null;
                memoryCache?.TryGetValue(MemoryCacheKey.SessionModelKey, out sessionModel);
                if (sessionModel != null)
                {
                    configClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(JwtBearerDefaults.AuthenticationScheme, sessionModel.Token);
                }
            });
            // 注册当前程序集的服务
            builder.Services.RegisterService(Assembly.GetExecutingAssembly());
            // 这里是用户自己添加需要的服务
            OnConfigIocServices?.Invoke(builder);
            //必须调用Build方法
            AppHost = builder.Build();
            //开始异步执行
            AppHost.RunAsync();
            //配置全局服务这样在任何程序集都可以调用服务
            ServiceProviderExtensions.ConfigServices(AppHost);
        }

        /// <summary>
        /// 初始化RSA非对称秘钥数据
        /// </summary>
        private void InitRSASecurityKeyData()
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

        /// <summary>
        /// 程序开始执行时触发事件
        /// </summary>
        protected override void OnStartup(StartupEventArgs e)
        {
            System.Windows.FrameworkCompatibilityPreferences.KeepTextBoxDisplaySynchronizedWithTextProperty = false;
            base.OnStartup(e);

            //配置依赖注入服务
            this.ConfigIocServices();

            // 初始化InfoBar消息窗体
            RSWinInfoBar = AppHost.Services.GetRequiredService<RSWinInfoBar>();

            // 获取日志服务
            LogService = AppHost.Services.GetRequiredService<ILogService>();

            //获取缓存服务
            MemoryCache = AppHost.Services.GetRequiredService<IMemoryCache>();

            //获取加密服务
            CryptographyService = AppHost.Services.GetRequiredService<ICryptographyService>();

            //初始化RSA 秘钥
            this.InitRSASecurityKeyData();

            //程序未处理异常
            this.RegisterUnknowExceptionsHandler();

            // 启动心跳检测线程
            this.StartHeartbeatCheckAsync();
        }


        protected override void OnExit(ExitEventArgs e)
        {
            // 停止心跳检测线程
            heartbeatCancellation?.Cancel();
            //主动关闭消息提示窗，不然程序无法正常退出
            RSWinInfoBar?.Close();
        }

    }
}
