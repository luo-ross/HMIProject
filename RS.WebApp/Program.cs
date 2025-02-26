using IdGen;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption;
using Microsoft.AspNetCore.DataProtection.AuthenticatedEncryption.ConfigurationModel;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using RazorLight;
using RazorLight.Extensions;
using RS.Commons;
using RS.Commons.Converters;
using RS.Commons.Extensions;
using RS.Models;
using RS.WebApp.BLL;
using RS.WebApp.DAL;
using RS.WebApp.Filters;
using System.Text;

namespace RS.WebApp
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
        /// RSA非对称秘钥公钥保存路径
        /// </summary>
        private static string? GlobalRsaPublicKeySavePath { get; set; }

        /// <summary>
        /// RSA非对称秘钥私钥保存路径
        /// </summary>
        private static string? GlobalRsaPrivateKeySavePath { get; set; }


        public static void Main(string[] args)
        {

            //在 ASP.NET Core 3.x 及之前版本中，应用程序的创建和配置通常通过 WebHostBuilder 或 HostBuilder 来完成。
            //然而，从 ASP.NET Core 3.1 开始，特别是在 ASP.NET Core 5 和更高版本中，引入了一种更简洁的语法来启动和配置应用程序
            //这通常是通过调用 WebApplication.CreateBuilder(args) 来实现的（在 ASP.NET Core 6 及更高版本中）。
            //ASP.NET Core 6 及更高版本在 ASP.NET Core 6 及更高版本中，Program.cs 文件的结构发生了显著变化
            //以支持更简洁的顶级语句和更简洁的应用程序配置。
            //在这种新的结构中，WebApplication.CreateBuilder(args) 被用于创建一个新的 WebApplicationBuilder 实例
            //该实例用于配置应用程序的各个方面，包括日志记录、依赖注入（DI）容器中的服务注册等。
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();

            //AddControllersWithViews 是 ASP.NET Core MVC 框架中的一个服务扩展方法
            //它用于在应用程序的服务集合（IServiceCollection）中添加对 MVC 控制器的支持,并且这些控制器支持视图（Views）。
            //这个方法通常在你的 ASP.NET Core 应用程序的启动配置中使用，特别是在使用 MVC 架构时。
            //当你调用 AddControllersWithViews 时，ASP.NET Core 会配置 MVC 框架以支持控制器和视图。
            //这意味着你的应用程序将能够处理来自用户的 HTTP 请求，并将这些请求路由到相应的 MVC 控制器。
            //控制器将执行一些逻辑处理，并可能返回一个视图（View），该视图随后会被渲染成 HTML 并发送给客户端。
            //使用场景
            //当你的 ASP.NET Core 应用程序使用 MVC 架构，并且你需要控制器来处理请求并返回包含视图的响应时。
            //当你需要利用 MVC 框架提供的路由、模型绑定、模型验证、过滤器等功能时。
            builder.Services.AddControllersWithViews(configure =>
            {
                configure.Filters.Add<GlobalFilter>();
            }).AddJsonOptions(configure =>
            {
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

            //AddSingleton 是 ASP.NET Core（以及更广泛的.NET Core 和.NET Framework 的依赖注入（DI）容器）中的一个方法
            //用于向服务容器中注册一个服务，其生命周期为单例（Singleton）。
            //这意味着在应用程序的整个生命周期内，无论请求多少次，该服务的实例都将是相同的。
            //使用场景
            //当你的应用程序中有一些资源或配置信息，它们在应用程序的生命周期内只需要被加载或初始化一次时，使用单例模式是非常合适的。
            //对于那些需要维护状态或状态的更改在整个应用程序中都需要被感知的服务，单例模式也是一个好的选择。
            //注册动态生成分布式主键服务
            builder.Services.AddSingleton<IIdGenerator<long>>(service =>
            {
                var configuration = builder.Configuration;
                int generatorId = Convert.ToInt32(configuration["GeneratorIdClient"]);
                return new IdGenerator(generatorId, IdGeneratorOptions.Default);
            });

            //AddDataProtection 是 ASP.NET Core 中用于向应用程序的服务集合（IServiceCollection）添加数据保护服务的方法。
            //数据保护服务主要用于保护应用程序中的敏感数据，如用户凭据、令牌等。
            builder.Services.AddDataProtection(setupAction =>
            {
                var configuration = builder.Configuration;
                string applicationDiscriminator = configuration["ConnectionStrings:ApplicationDiscriminator"];
                setupAction.ApplicationDiscriminator = applicationDiscriminator;
            }).PersistKeysToFileSystem(new DirectoryInfo(KeysRepository))
              .UseCryptographicAlgorithms(new AuthenticatedEncryptorConfiguration()
              {
                  EncryptionAlgorithm = EncryptionAlgorithm.AES_256_CBC,
                  ValidationAlgorithm = ValidationAlgorithm.HMACSHA256
              })
              .SetDefaultKeyLifetime(TimeSpan.FromDays(7));

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
                string? tokenSecurityKey = configuration["ConnectionStrings:JWTConfig:SecurityKey"];
                string? issuer = configuration["ConnectionStrings:JWTConfig:Issuer"];
                string[]? audiences = configuration.GetSection("ConnectionStrings:JWTConfig:Audiences").Get<string[]>();
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

            //注册方法连接服务
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

            //AddSwaggerGen 是 ASP.NET Core 中用于集成 Swagger（现在称为 OpenAPI）的一个扩展方法
            //它属于 Swashbuckle.AspNetCore 包。
            //Swagger 是一个规范和完整的框架，用于生成、描述、调用和可视化 RESTful 风格的 Web 服务。
            //通过 AddSwaggerGen，你可以将 Swagger 集成到你的 ASP.NET Core 应用中，从而自动生成 API 文档
            //这对于 API 的开发、测试和消费都是非常有帮助的。
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

            //启动日志后台线程服务
            var logService = AppHost.Services.GetRequiredService<ILogService>();
            logService.InitLogService();

            //配置HTTP请求管道。
            if (AppHost.Environment.IsDevelopment())
            {

                AppHost.UseDeveloperExceptionPage();

                //UseExceptionHandler 是 ASP.NET Core 中的一个中间件（Middleware）
                //用于全局处理应用程序中的异常。
                //这个中间件可以捕获在 ASP.NET Core 请求处理管道中发生的未处理异常，并提供一个统一的处理机制
                AppHost.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.


                // UseHsts用于实现 HTTP 严格传输安全协议（HTTP Strict Transport Security，简称 HSTS）。
                // HSTS 是一种安全功能，它允许网站通过 HTTP 响应头来告诉浏览器，在接下来的一段时间内（通常是几个月）
                // 该网站的所有通信都必须通过 HTTPS 进行，而不是 HTTP。
                // 这有助于防止中间人攻击和其他类型的网络攻击，因为它确保了用户与网站之间的通信始终是加密的。
                //在 ASP.NET Core 应用中，UseHsts 中间件通常被添加到应用的请求处理管道中，以确保所有后续的请求都通过 HTTPS 进行。
                //这个中间件应该被放置在 HTTPS 重定向中间件（UseHttpsRedirection）之后，以确保在启用 HSTS 之前，
                //所有非 HTTPS 请求都已经被重定向到 HTTPS。
                //AppHost.UseHsts();

                //UseSwagger 方法在ASP.NET Core应用的请求处理管道中启用Swagger生成的JSON文档终结点。
                //这个JSON文档描述了API的元数据，包括路由、参数、响应等信息，是Swagger UI和Swagger代码生成器的基础。
                AppHost.UseSwagger();

                //UseSwaggerUI 是 ASP.NET Core 中用于启用 Swagger UI 的中间件配置方法。
                //Swagger UI 是一个基于 Web 的用户界面，用于显示 Swagger 生成的 API 文档
                //并允许开发者进行 API 的交互式测试。当在 ASP.NET Core 应用中集成了 Swagger 后
                //UseSwaggerUI 方法使得开发者可以通过浏览器访问 Swagger UI 页面从而方便地查看 API 的详细信息、进行请求和响应的测试等。
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
                        var logger = context.RequestServices.GetRequiredService<ILogService>();

                        // 记录异常  
                        logger.LogError(exceptionHandlerPathFeature.Error, "An unhandled exception has occurred.");
                        // 发送HTTP 500响应  
                        context.Response.ContentType = "application/json";
                        OperateResult operateResult = OperateResult.CreateFailResult<object>("服务端出错了，暂时无法访问");
                        operateResult.ErrorCode = 99999;
                        await context.Response.WriteAsync(new JsonResult(operateResult)
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                        }.ToString());
                    }
                });
            });

            // UseHttpsRedirection 是 ASP.NET Core 中的一个中间件（Middleware），它用于将 HTTP 请求重定向到 HTTPS。
            // 这是增强网站安全性的一个常见做法，因为 HTTPS 通过 SSL/ TLS 协议提供了加密的通信通道，可以保护数据的机密性和完整性。
            //在 ASP.NET Core 应用中，UseHttpsRedirection 中间件通常被添加到应用的请求处理管道中
            //以确保所有非安全的 HTTP 请求都被自动重定向到 HTTPS。
            //这样做可以防止敏感信息（如用户凭据、会话令牌等）在网络上以明文形式传输，从而被截获或篡改。
            //AppHost.UseHttpsRedirection();

            //UseAuthentication（认证）：
            //作用：UseAuthentication中间件用于验证用户身份。
            //它会检查传入的HTTP请求是否包含有效的身份验证凭据，如Cookie、JWT（JSON Web Tokens）等。
            //处理流程：如果请求中包含有效的身份验证凭据，UseAuthentication中间件会将这些凭据解析为用户身份信息
            //并将其存储在HttpContext.User中，供后续的中间件和控制器使用。
            //如果请求中没有有效的身份验证凭据，中间件可能会将用户重定向到登录页面或返回401未授权响应。
            //重要性：UseAuthentication是授权（UseAuthorization）之前的基础步骤
            //因为授权过程需要知道用户的身份才能判断其是否有权访问特定资源。
            AppHost.UseAuthentication();

            // MapControllers 的作用
            //控制器路由：MapControllers 方法会自动扫描应用中的 MVC 控制器
            //并根据控制器上的路由属性（如[Route]、[HttpGet]、[HttpPost] 等）来配置路由。
            //这意味着开发者不需要在 Startup.cs 文件中显式地为每个控制器或操作定义路由模板。
            //灵活性：通过使用属性路由，MapControllers 提供了极高的灵活性
            //允许开发者以声明方式定义路由模板，并将它们直接应用于控制器或操作。
            //简化配置：与在 Startup.cs 文件中手动定义路由相比，MapControllers 方法大大简化了 MVC 应用的路由配置过程。
            AppHost.MapControllers();

            //MapControllerRoute 的作用
            //MapControllerRoute 方法允许开发者在应用的路由表中添加一个或多个路由规则
            //这些规则指定了如何将传入的 URL 请求映射到特定的 MVC 控制器和操作。
            //每个路由规则都包括一个名称、一个 URL 模式和一个默认值集合，用于指定当 URL 与模式匹配时应该调用的控制器和操作。
            AppHost.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");

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

            //UseAuthorization（授权）：
            //作用：UseAuthorization中间件用于授权用户访问资源。它会检查用户（已经通过UseAuthentication验证）是否具有访问特定资源的权限。
            //处理流程：如果用户具有访问权限，则允许其继续访问资源；如果用户没有访问权限，则返回403禁止访问响应。
            //依赖性：在使用UseAuthorization中间件之前，必须先调用UseAuthentication中间件，以确保用户已被验证。
            //备注：
            //如果有对应用程序的调用。使用Routing（）和应用程序。UseEndpoints（…），对应用程序的调用。
            //UseAuthorization（）必须介于两者之间。
            AppHost.UseAuthorization();

            // UseEndpoints 的作用
            //终结点配置：UseEndpoints 方法允许开发者在应用的请求处理管道中配置终结点。
            //这通常涉及到指定哪些 URL 模式应该被映射到哪些处理器上。
            //路由集成：与 UseRouting 中间件一起使用时，UseEndpoints 负责执行与路由匹配到的终结点相关联的委托或处理程序。
            //UseRouting 负责将请求映射到终结点，而 UseEndpoints 则负责执行这些终结点。
            //灵活性：UseEndpoints 提供了灵活的配置选项
            //允许开发者为不同类型的处理器（如 MVC 控制器、Razor 页面、SignalR 集线器等）定义路由规则。
            AppHost.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

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
            var cryptographyService = appHost.Services.GetRequiredService<ICryptographyService>();
            var configuration = appHost.Services.GetRequiredService<IConfiguration>();
            var memoryCache = appHost.Services.GetRequiredService<IMemoryCache>();

            string globalRsaPublicKeyFileName = configuration["ConnectionStrings:GlobalRsaPublicKeyFileName"];
            string globalRsaPrivateKeyFileName = configuration["ConnectionStrings:GlobalRsaPrivateKeyFileName"];

            GlobalRsaPublicKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRsaPublicKeyFileName);
            GlobalRsaPrivateKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRsaPrivateKeyFileName);

            //如果是第一就会创建公钥和私钥
            cryptographyService.InitServerRSAKey(GlobalRsaPublicKeySavePath, GlobalRsaPrivateKeySavePath);

            //加载公钥和私钥
            var rsaPublicKey = cryptographyService.GetRSAPublicKey(GlobalRsaPublicKeySavePath).Data;
            var rsaPrivateKey = cryptographyService.GetRSAPrivateKey(GlobalRsaPrivateKeySavePath).Data;
            memoryCache.Set(MemoryCacheKey.GlobalRSAPublicKey, rsaPublicKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSAPrivateKey, rsaPrivateKey);
        }
    }
}