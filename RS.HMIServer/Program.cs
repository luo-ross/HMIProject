using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using RazorLight;
using RazorLight.Extensions;
using RS.Commons;
using RS.Commons.Converters;
using RS.Commons.Extensions;
using RS.HMIServer.BLL;
using RS.HMIServer.DAL;
using RS.HMIServer.Filters;
using RS.Models;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Channels;

namespace RS.HMIServer
{
    public class Program
    {
        /// <summary>
        /// 服务
        /// </summary>
        public static WebApplication? AppHost { get; set; }

        /// <summary>
        /// 秘钥存储目录
        /// </summary>
        private static readonly string KeysRepository = "Keys-Repository";

        /// <summary>
        /// RSA非对称秘钥加密公钥保存路径
        /// </summary>
        private static string? GlobalRSAEncryptPublicKeySavePath { get; set; }

        /// <summary>
        /// RSA非对称秘钥加密私钥保存路径
        /// </summary>
        private static string? GlobalRSAEncryptPrivateKeySavePath { get; set; }

        /// <summary>
        /// RSA非对称秘钥签名公钥保存路径
        /// </summary>
        private static string? GlobalRSASignPublicKeySavePath { get; set; }

        /// <summary>
        /// RSA非对称秘钥签名私钥保存路径
        /// </summary>
        private static string? GlobalRSASignPrivateKeySavePath { get; set; }


        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            //配置解决跨域问题
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowWebClients", policy =>
                {
                    var allowedOrigins = builder.Configuration.GetSection("AllowedClients:Origins").Get<string[]>();
                    policy.WithOrigins(allowedOrigins)
                          .AllowAnyMethod()
                          .AllowAnyHeader()
                          .AllowCredentials();
                });
            });


            builder.Services.AddHttpContextAccessor();

            builder.Services.AddControllers(configure =>
            {
                configure.Filters.Add<ExceptionFilter>();
                configure.Filters.Add<AuthorizationFilter>();
                configure.Filters.Add<ActionFilter>();
                configure.Filters.Add<ResourceFilter>();
            }).AddJsonOptions(configure =>
            {
                //设置编码形式，防止中文乱码
                configure.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                configure.JsonSerializerOptions.PropertyNamingPolicy = null;
                configure.JsonSerializerOptions.Converters.Add(new DateTimeConvert());
            });

            // 添加RazorLight服务  
            builder.Services.AddRazorLight(() =>
            {
                return new RazorLightEngineBuilder()
                    .UseFileSystemProject(Directory.GetCurrentDirectory())
                    .UseMemoryCachingProvider()
                    .Build();
            });

            //AddDataProtection 是 ASP.NET Core 中用于向应用程序的服务集合（IServiceCollection）添加数据保护服务的方法。
            //数据保护服务主要用于保护应用程序中的敏感数据，如用户凭据、令牌等。
            builder.Services.AddDataProtection(setupAction =>
            {
                var configuration = builder.Configuration;
                string applicationDiscriminator = configuration["ApplicationDiscriminator"];
                setupAction.ApplicationDiscriminator = applicationDiscriminator;
            }).PersistKeysToFileSystem(new DirectoryInfo(KeysRepository))
            .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
            {
                EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
            }).SetDefaultKeyLifetime(TimeSpan.FromDays(7));

            //AddAuthentication 是 ASP.NET Core 中用于配置身份验证服务的一个方法。
            //它属于 ASP.NET Core 的身份验证和授权框架，用于在应用程序中启用和配置多种身份验证方案。
            //以下是关于 AddAuthentication 的详细解释：
            //一、定义与作用
            //定义：AddAuthentication 是一个在 ASP.NET Core 应用程序的 Startup.cs 文件的 ConfigureServices 方法中调用的扩展方法
            //用于向服务集合中添加身份验证服务。
            //作用：它自动配置多个身份验证方案和身份验证中间件
            //以便应用程序能够根据请求中的身份验证方案选择适当的方案进行身份验证。
            //此外，它还会配置授权中间件，确保在授权过程中使用正确的用户信息。
            //二、使用场景
            //当你的应用程序需要支持多种身份验证方式时（如 Cookie 身份验证、JWT 令牌身份验证、OAuth 身份验证等）
            //可以使用 AddAuthentication 来配置这些身份验证方案。
            //它适用于需要高级身份验证功能的场景，如单页应用程序（SPA）、Web API、移动后端等。
            builder.Services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                //配置JWT Token
                var configuration = builder.Configuration;
                string? tokenSecurityKey = configuration["JWTConfig:SecurityKey"];
                string? issuer = configuration["JWTConfig:Issuer"];
                string[]? audiences = configuration.GetSection("JWTConfig:Audiences").Get<string[]>();
                configureOptions.TokenValidationParameters = new TokenValidationParameters()
                {
                    ValidIssuer = issuer,
                    ValidAudiences = audiences,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(tokenSecurityKey)),
                };
            });

            // AddMemoryCache 是 ASP.NET Core 中用于向应用程序的服务集合（IServiceCollection）添加内存缓存支持的方法。
            // 内存缓存是一种快速且高效的缓存机制，它允许你将数据存储在应用程序的内存中，
            // 以便快速访问而无需每次都从数据库或外部服务中检索。这对于提高应用程序的性能和响应速度非常有用。
            //使用场景
            //当你需要缓存频繁访问但更新不频繁的数据时。
            //当你想要减少对外部数据源（如数据库或Web服务）的调用次数时。
            //当你需要实现基于时间的缓存过期策略时。
            builder.Services.AddMemoryCache();

            //注册通用服务
            builder.Services.RegisterCommonService();

            //注册日志服务
            builder.Services.RegisterLog4netService();

            //注册方法拦截服务
            builder.Services.RegisterInterceptorService();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //在 ASP.NET Core 中，传统的 MVC 控制器和动作方法通过 ApiExplorer 服务来提供关于 API 的元数据
            //这些元数据随后可以被 Swagger 等工具用来生成 API 文档。
            //然而，最小 API 是一种更简洁的编写 API 的方式，它直接定义在路由管道中，而不依赖于 MVC 的控制器和动作模型。
            //因此，最小 API 默认情况下不会生成 ApiExplorer 所需的元数据。
            //为了解决这个问题，ASP.NET Core 引入了 AddEndpointsApiExplorer 方法。
            //这个方法添加了一个基于路由系统的 IApiDescriptionProvider 实现
            //该实现能够为最小 API 生成必要的元数据，从而使这些 API 能够在 Swagger 等工具中展示。
            builder.Services.AddEndpointsApiExplorer();

            //配置使用最新版本的Swagger 
            //Swashbuckle 在 .NET 9 或更高版本中不可用。 有关替代方法，请参阅 ASP.NET Core API 应用中的 OpenAPI 支持概述。
            builder.Services.AddSwaggerGen();

            //在 ASP.NET Core 中，AddHttpClient 是一个用于配置和注册 HttpClient 实例的扩展方法
            //它属于 Microsoft.Extensions.DependencyInjection 命名空间。
            //通过使用 AddHttpClient，开发者可以轻松地创建和管理 HttpClient 实例
            //这些实例可以被注入到需要它们的类中，从而避免了在每个需要发起 HTTP 请求的地方都手动创建和配置 HttpClient 的繁琐过程
            builder.Services.AddHttpClient();

            //注册数据业务层服务
            builder.Services.RegisterDALService(builder.Configuration);

            //注册逻辑业务层服务
            builder.Services.RegisterBLLService(builder.Configuration);

            //构建WebApplication。
            AppHost = builder.Build();

            //这个必须放在builder.Build()后才生效
            ServiceProviderExtensions.ConfigServices(AppHost);

            if (AppHost.Environment.IsDevelopment())
            {
                AppHost.UseSwagger();
                AppHost.UseSwaggerUI();
            }

            //在ASP.NET Core中，UseExceptionHandler中间件被用来捕获并处理应用程序中未处理的异常。
            //这些异常可能是在MVC控制器、中间件、过滤器或其他组件中抛出的。
            //当UseExceptionHandler捕获到异常时，它可以重定向请求到另一个管道，该管道专门用于处理这些异常，
            //或者它可以直接在捕获点处理异常，比如通过发送一个HTTP响应。
            AppHost.UseExceptionHandler(configure =>
            {
                configure.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error != null)
                    {
                        // 从DI中获取日志记录器  
                        var logger = context.RequestServices.GetRequiredService<ILogBLL>();

                        // 记录异常  
                        logger.LogError(exceptionHandlerPathFeature.Error, "An unhandled exception has occurred.");
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.Redirect("/ServerError.html");
                    }
                });
            });

            // UseHttpsRedirection 是 ASP.NET Core 中的一个中间件（Middleware），它用于将 HTTP 请求重定向到 HTTPS。
            // 这是增强网站安全性的一个常见做法，因为 HTTPS 通过 SSL/ TLS 协议提供了加密的通信通道，可以保护数据的机密性和完整性。
            //在 ASP.NET Core 应用中，UseHttpsRedirection 中间件通常被添加到应用的请求处理管道中
            //以确保所有非安全的 HTTP 请求都被自动重定向到 HTTPS。
            //这样做可以防止敏感信息（如用户凭据、会话令牌等）在网络上以明文形式传输，从而被截获或篡改。
            AppHost.UseHttpsRedirection();

            //UseAuthentication（认证）：
            //作用：UseAuthentication中间件用于验证用户身份。
            //它会检查传入的HTTP请求是否包含有效的身份验证凭据，如Cookie、JWT（JSON Web Tokens）等。
            //处理流程：如果请求中包含有效的身份验证凭据，UseAuthentication中间件会将这些凭据解析为用户身份信息
            //并将其存储在HttpContext.User中，供后续的中间件和控制器使用。
            //如果请求中没有有效的身份验证凭据，中间件可能会将用户重定向到登录页面或返回401未授权响应。
            //重要性：UseAuthentication是授权（UseAuthorization）之前的基础步骤
            //因为授权过程需要知道用户的身份才能判断其是否有权访问特定资源。
            AppHost.UseAuthentication();

            //UseStaticFiles 的作用
            //静态文件服务：UseStaticFiles 中间件能够处理对静态文件的请求
            //并直接从文件系统中读取这些文件的内容，然后将其作为响应返回给客户端。
            //灵活配置：通过配置 StaticFileOptions，开发者可以自定义静态文件服务的行为
            //包括指定静态文件的存储位置、设置默认文件、配置MIME类型映射等。
            //性能优化：静态文件服务通常比动态内容生成更快，因为它们不需要执行复杂的业务逻辑或数据库查询。
            //此外，ASP.NET Core 还支持对静态文件的缓存和压缩，以进一步提高性能。
            AppHost.UseStaticFiles();

            // UseRouting 的作用
            //路由匹配：UseRouting 中间件会查看应用中定义的终结点集合，并尝试将传入的 HTTP 请求与这些终结点进行匹配。
            //匹配过程基于请求的 URL、HTTP 方法以及任何路由约束等信息。
            //选择终结点：一旦找到匹配的终结点，UseRouting 中间件会将请求的相关信息（如选定的终结点）存储在 HttpContext 对象中
            //以便后续的中间件或处理程序可以访问这些信息。
            //传递请求：匹配过程完成后，UseRouting 中间件会将请求传递给管道中的下一个中间件
            //通常是 UseEndpoints 中间件，后者负责执行与所选终结点关联的委托或处理程序。
            AppHost.UseRouting();


            AppHost.UseCors("AllowWebClients");

            //UseAuthorization（授权）：
            //作用：UseAuthorization中间件用于授权用户访问资源。它会检查用户（已经通过UseAuthentication验证）是否具有访问特定资源的权限。
            //处理流程：如果用户具有访问权限，则允许其继续访问资源；如果用户没有访问权限，则返回403禁止访问响应。
            //依赖性：在使用UseAuthorization中间件之前，必须先调用UseAuthentication中间件，以确保用户已被验证。
            //备注：
            //如果有对应用程序的调用。使用Routing（）和应用程序。UseEndpoints（…），对应用程序的调用。
            //UseAuthorization（）必须介于两者之间。
            AppHost.UseAuthorization();

            AppHost.MapControllers();

            //初始化RSA非对称秘钥
            InitRSASecurityKeyData(AppHost);

            //运行应用程序并阻止调用线程，直到主机关闭。
            AppHost.Run();
        }


        /// <summary>
        /// 初始化RSA非对称秘钥数据
        /// </summary>
        public static void InitRSASecurityKeyData(WebApplication appHost)
        {
            if (appHost == null)
            {
                throw new ArgumentNullException(nameof(appHost));
            }
            var cryptographyBLL = appHost.Services.GetRequiredService<ICryptographyBLL>();
            var configuration = appHost.Services.GetRequiredService<IConfiguration>();
            var memoryCache = appHost.Services.GetRequiredService<IMemoryCache>();


            string globalRSASignPublicKeyFileName = configuration["GlobalRSASignPublicKeyFileName"];
            string globalRSASignPrivateKeyFileName = configuration["GlobalRSASignPrivateKeyFileName"];
            string globalRSAEncryptPublicKeyFileName = configuration["GlobalRSAEncryptPublicKeyFileName"];
            string globalRSAEncryptPrivateKeyFileName = configuration["GlobalRSAEncryptPrivateKeyFileName"];


            GlobalRSASignPublicKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRSASignPublicKeyFileName);
            GlobalRSASignPrivateKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRSASignPrivateKeyFileName);
            GlobalRSAEncryptPublicKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRSAEncryptPublicKeyFileName);
            GlobalRSAEncryptPrivateKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRSAEncryptPrivateKeyFileName);

            //创建签名密钥对
            cryptographyBLL.InitServerRSAKey(GlobalRSASignPublicKeySavePath, GlobalRSASignPrivateKeySavePath);
            //创建加密密钥对
            cryptographyBLL.InitServerRSAKey(GlobalRSAEncryptPublicKeySavePath, GlobalRSAEncryptPrivateKeySavePath);

            //加载签名公钥和私钥
            var rsaSigningPublicKey = cryptographyBLL.GetRSAPublicKey(GlobalRSASignPublicKeySavePath).Data;
            var rsaSigningPrivateKey = cryptographyBLL.GetRSAPrivateKey(GlobalRSASignPrivateKeySavePath).Data;
           
            //加载加解密公钥和私钥
            var rsaEncryptionPublicKey = cryptographyBLL.GetRSAPublicKey(GlobalRSAEncryptPublicKeySavePath).Data;
            var rsaEncryptionPrivateKey = cryptographyBLL.GetRSAPrivateKey(GlobalRSAEncryptPrivateKeySavePath).Data;

            //将密钥存入缓存
            memoryCache.Set(MemoryCacheKey.GlobalRSASignPublicKey, rsaSigningPublicKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSASignPrivateKey, rsaSigningPrivateKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSAEncryptPublicKey, rsaEncryptionPublicKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSAEncryptPrivateKey, rsaEncryptionPrivateKey);
        }
    }
}