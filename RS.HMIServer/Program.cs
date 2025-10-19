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
        /// ����
        /// </summary>
        public static WebApplication? AppHost { get; set; }

        /// <summary>
        /// ��Կ�洢Ŀ¼
        /// </summary>
        private static readonly string KeysRepository = "Keys-Repository";

        /// <summary>
        /// RSA�ǶԳ���Կ���ܹ�Կ����·��
        /// </summary>
        private static string? GlobalRSAEncryptPublicKeySavePath { get; set; }

        /// <summary>
        /// RSA�ǶԳ���Կ����˽Կ����·��
        /// </summary>
        private static string? GlobalRSAEncryptPrivateKeySavePath { get; set; }

        /// <summary>
        /// RSA�ǶԳ���Կǩ����Կ����·��
        /// </summary>
        private static string? GlobalRSASignPublicKeySavePath { get; set; }

        /// <summary>
        /// RSA�ǶԳ���Կǩ��˽Կ����·��
        /// </summary>
        private static string? GlobalRSASignPrivateKeySavePath { get; set; }


        public static void Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            //���ý����������
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
                //���ñ�����ʽ����ֹ��������
                configure.JsonSerializerOptions.Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping;
                configure.JsonSerializerOptions.PropertyNamingPolicy = null;
                configure.JsonSerializerOptions.Converters.Add(new DateTimeConvert());
            });

            // ���RazorLight����  
            builder.Services.AddRazorLight(() =>
            {
                return new RazorLightEngineBuilder()
                    .UseFileSystemProject(Directory.GetCurrentDirectory())
                    .UseMemoryCachingProvider()
                    .Build();
            });

            //AddDataProtection �� ASP.NET Core ��������Ӧ�ó���ķ��񼯺ϣ�IServiceCollection��������ݱ�������ķ�����
            //���ݱ���������Ҫ���ڱ���Ӧ�ó����е��������ݣ����û�ƾ�ݡ����Ƶȡ�
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

            //AddAuthentication �� ASP.NET Core ���������������֤�����һ��������
            //������ ASP.NET Core �������֤����Ȩ��ܣ�������Ӧ�ó��������ú����ö��������֤������
            //�����ǹ��� AddAuthentication ����ϸ���ͣ�
            //һ������������
            //���壺AddAuthentication ��һ���� ASP.NET Core Ӧ�ó���� Startup.cs �ļ��� ConfigureServices �����е��õ���չ����
            //��������񼯺�����������֤����
            //���ã����Զ����ö�������֤�����������֤�м��
            //�Ա�Ӧ�ó����ܹ����������е������֤����ѡ���ʵ��ķ������������֤��
            //���⣬������������Ȩ�м����ȷ������Ȩ������ʹ����ȷ���û���Ϣ��
            //����ʹ�ó���
            //�����Ӧ�ó�����Ҫ֧�ֶ��������֤��ʽʱ���� Cookie �����֤��JWT ���������֤��OAuth �����֤�ȣ�
            //����ʹ�� AddAuthentication ��������Щ�����֤������
            //����������Ҫ�߼������֤���ܵĳ������絥ҳӦ�ó���SPA����Web API���ƶ���˵ȡ�
            builder.Services.AddAuthentication(configureOptions =>
            {
                configureOptions.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                configureOptions.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;

            }).AddJwtBearer(configureOptions =>
            {
                //����JWT Token
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

            // AddMemoryCache �� ASP.NET Core ��������Ӧ�ó���ķ��񼯺ϣ�IServiceCollection������ڴ滺��֧�ֵķ�����
            // �ڴ滺����һ�ֿ����Ҹ�Ч�Ļ�����ƣ��������㽫���ݴ洢��Ӧ�ó�����ڴ��У�
            // �Ա���ٷ��ʶ�����ÿ�ζ������ݿ���ⲿ�����м�������������Ӧ�ó�������ܺ���Ӧ�ٶȷǳ����á�
            //ʹ�ó���
            //������Ҫ����Ƶ�����ʵ����²�Ƶ��������ʱ��
            //������Ҫ���ٶ��ⲿ����Դ�������ݿ��Web���񣩵ĵ��ô���ʱ��
            //������Ҫʵ�ֻ���ʱ��Ļ�����ڲ���ʱ��
            builder.Services.AddMemoryCache();

            //ע��ͨ�÷���
            builder.Services.RegisterCommonService();

            //ע����־����
            builder.Services.RegisterLog4netService();

            //ע�᷽�����ط���
            builder.Services.RegisterInterceptorService();

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            //�� ASP.NET Core �У���ͳ�� MVC �������Ͷ�������ͨ�� ApiExplorer �������ṩ���� API ��Ԫ����
            //��ЩԪ���������Ա� Swagger �ȹ����������� API �ĵ���
            //Ȼ������С API ��һ�ָ����ı�д API �ķ�ʽ����ֱ�Ӷ�����·�ɹܵ��У����������� MVC �Ŀ������Ͷ���ģ�͡�
            //��ˣ���С API Ĭ������²������� ApiExplorer �����Ԫ���ݡ�
            //Ϊ�˽��������⣬ASP.NET Core ������ AddEndpointsApiExplorer ������
            //������������һ������·��ϵͳ�� IApiDescriptionProvider ʵ��
            //��ʵ���ܹ�Ϊ��С API ���ɱ�Ҫ��Ԫ���ݣ��Ӷ�ʹ��Щ API �ܹ��� Swagger �ȹ�����չʾ��
            builder.Services.AddEndpointsApiExplorer();

            //����ʹ�����°汾��Swagger 
            //Swashbuckle �� .NET 9 ����߰汾�в����á� �й��������������� ASP.NET Core API Ӧ���е� OpenAPI ֧�ָ�����
            builder.Services.AddSwaggerGen();

            //�� ASP.NET Core �У�AddHttpClient ��һ���������ú�ע�� HttpClient ʵ������չ����
            //������ Microsoft.Extensions.DependencyInjection �����ռ䡣
            //ͨ��ʹ�� AddHttpClient�������߿������ɵش����͹��� HttpClient ʵ��
            //��Щʵ�����Ա�ע�뵽��Ҫ���ǵ����У��Ӷ���������ÿ����Ҫ���� HTTP ����ĵط����ֶ����������� HttpClient �ķ�������
            builder.Services.AddHttpClient();

            //ע������ҵ������
            builder.Services.RegisterDALService(builder.Configuration);

            //ע���߼�ҵ������
            builder.Services.RegisterBLLService(builder.Configuration);

            //����WebApplication��
            AppHost = builder.Build();

            //����������builder.Build()�����Ч
            ServiceProviderExtensions.ConfigServices(AppHost);

            if (AppHost.Environment.IsDevelopment())
            {
                AppHost.UseSwagger();
                AppHost.UseSwaggerUI();
            }

            //��ASP.NET Core�У�UseExceptionHandler�м�����������񲢴���Ӧ�ó�����δ������쳣��
            //��Щ�쳣��������MVC���������м����������������������׳��ġ�
            //��UseExceptionHandler�����쳣ʱ���������ض���������һ���ܵ����ùܵ�ר�����ڴ�����Щ�쳣��
            //����������ֱ���ڲ���㴦���쳣������ͨ������һ��HTTP��Ӧ��
            AppHost.UseExceptionHandler(configure =>
            {
                configure.Run(async context =>
                {
                    var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
                    if (exceptionHandlerPathFeature?.Error != null)
                    {
                        // ��DI�л�ȡ��־��¼��  
                        var logger = context.RequestServices.GetRequiredService<ILogBLL>();

                        // ��¼�쳣  
                        logger.LogError(exceptionHandlerPathFeature.Error, "An unhandled exception has occurred.");
                        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                        context.Response.Redirect("/ServerError.html");
                    }
                });
            });

            // UseHttpsRedirection �� ASP.NET Core �е�һ���м����Middleware���������ڽ� HTTP �����ض��� HTTPS��
            // ������ǿ��վ��ȫ�Ե�һ��������������Ϊ HTTPS ͨ�� SSL/ TLS Э���ṩ�˼��ܵ�ͨ��ͨ�������Ա������ݵĻ����Ժ������ԡ�
            //�� ASP.NET Core Ӧ���У�UseHttpsRedirection �м��ͨ������ӵ�Ӧ�õ�������ܵ���
            //��ȷ�����зǰ�ȫ�� HTTP ���󶼱��Զ��ض��� HTTPS��
            //���������Է�ֹ������Ϣ�����û�ƾ�ݡ��Ự���Ƶȣ�����������������ʽ���䣬�Ӷ����ػ��۸ġ�
            AppHost.UseHttpsRedirection();

            //UseAuthentication����֤����
            //���ã�UseAuthentication�м��������֤�û���ݡ�
            //�����鴫���HTTP�����Ƿ������Ч�������֤ƾ�ݣ���Cookie��JWT��JSON Web Tokens���ȡ�
            //�������̣���������а�����Ч�������֤ƾ�ݣ�UseAuthentication�м���Ὣ��Щƾ�ݽ���Ϊ�û������Ϣ
            //������洢��HttpContext.User�У����������м���Ϳ�����ʹ�á�
            //���������û����Ч�������֤ƾ�ݣ��м�����ܻὫ�û��ض��򵽵�¼ҳ��򷵻�401δ��Ȩ��Ӧ��
            //��Ҫ�ԣ�UseAuthentication����Ȩ��UseAuthorization��֮ǰ�Ļ�������
            //��Ϊ��Ȩ������Ҫ֪���û�����ݲ����ж����Ƿ���Ȩ�����ض���Դ��
            AppHost.UseAuthentication();

            //UseStaticFiles ������
            //��̬�ļ�����UseStaticFiles �м���ܹ�����Ծ�̬�ļ�������
            //��ֱ�Ӵ��ļ�ϵͳ�ж�ȡ��Щ�ļ������ݣ�Ȼ������Ϊ��Ӧ���ظ��ͻ��ˡ�
            //������ã�ͨ������ StaticFileOptions�������߿����Զ��徲̬�ļ��������Ϊ
            //����ָ����̬�ļ��Ĵ洢λ�á�����Ĭ���ļ�������MIME����ӳ��ȡ�
            //�����Ż�����̬�ļ�����ͨ���ȶ�̬�������ɸ��죬��Ϊ���ǲ���Ҫִ�и��ӵ�ҵ���߼������ݿ��ѯ��
            //���⣬ASP.NET Core ��֧�ֶԾ�̬�ļ��Ļ����ѹ�����Խ�һ��������ܡ�
            AppHost.UseStaticFiles();

            // UseRouting ������
            //·��ƥ�䣺UseRouting �м����鿴Ӧ���ж�����ս�㼯�ϣ������Խ������ HTTP ��������Щ�ս�����ƥ�䡣
            //ƥ����̻�������� URL��HTTP �����Լ��κ�·��Լ������Ϣ��
            //ѡ���ս�㣺һ���ҵ�ƥ����ս�㣬UseRouting �м���Ὣ����������Ϣ����ѡ�����ս�㣩�洢�� HttpContext ������
            //�Ա�������м�����������Է�����Щ��Ϣ��
            //��������ƥ�������ɺ�UseRouting �м���Ὣ���󴫵ݸ��ܵ��е���һ���м��
            //ͨ���� UseEndpoints �м�������߸���ִ������ѡ�ս�������ί�л������
            AppHost.UseRouting();


            AppHost.UseCors("AllowWebClients");

            //UseAuthorization����Ȩ����
            //���ã�UseAuthorization�м��������Ȩ�û�������Դ���������û����Ѿ�ͨ��UseAuthentication��֤���Ƿ���з����ض���Դ��Ȩ�ޡ�
            //�������̣�����û����з���Ȩ�ޣ������������������Դ������û�û�з���Ȩ�ޣ��򷵻�403��ֹ������Ӧ��
            //�����ԣ���ʹ��UseAuthorization�м��֮ǰ�������ȵ���UseAuthentication�м������ȷ���û��ѱ���֤��
            //��ע��
            //����ж�Ӧ�ó���ĵ��á�ʹ��Routing������Ӧ�ó���UseEndpoints����������Ӧ�ó���ĵ��á�
            //UseAuthorization���������������֮�䡣
            AppHost.UseAuthorization();

            AppHost.MapControllers();

            //��ʼ��RSA�ǶԳ���Կ
            InitRSASecurityKeyData(AppHost);

            //����Ӧ�ó�����ֹ�����̣߳�ֱ�������رա�
            AppHost.Run();
        }


        /// <summary>
        /// ��ʼ��RSA�ǶԳ���Կ����
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

            //����ǩ����Կ��
            cryptographyBLL.InitServerRSAKey(GlobalRSASignPublicKeySavePath, GlobalRSASignPrivateKeySavePath);
            //����������Կ��
            cryptographyBLL.InitServerRSAKey(GlobalRSAEncryptPublicKeySavePath, GlobalRSAEncryptPrivateKeySavePath);

            //����ǩ����Կ��˽Կ
            var rsaSigningPublicKey = cryptographyBLL.GetRSAPublicKey(GlobalRSASignPublicKeySavePath).Data;
            var rsaSigningPrivateKey = cryptographyBLL.GetRSAPrivateKey(GlobalRSASignPrivateKeySavePath).Data;
           
            //���ؼӽ��ܹ�Կ��˽Կ
            var rsaEncryptionPublicKey = cryptographyBLL.GetRSAPublicKey(GlobalRSAEncryptPublicKeySavePath).Data;
            var rsaEncryptionPrivateKey = cryptographyBLL.GetRSAPrivateKey(GlobalRSAEncryptPrivateKeySavePath).Data;

            //����Կ���뻺��
            memoryCache.Set(MemoryCacheKey.GlobalRSASignPublicKey, rsaSigningPublicKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSASignPrivateKey, rsaSigningPrivateKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSAEncryptPublicKey, rsaEncryptionPublicKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSAEncryptPrivateKey, rsaEncryptionPrivateKey);
        }
    }
}