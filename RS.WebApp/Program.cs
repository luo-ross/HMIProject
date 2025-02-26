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
        /// ����
        /// </summary>
        public static WebApplication? AppHost { get; set; }

        /// <summary>
        /// ��Կ�洢Ŀ¼
        /// </summary>
        private static readonly string KeysRepository = "Keys-Repository";

        /// <summary>
        /// RSA�ǶԳ���Կ��Կ����·��
        /// </summary>
        private static string? GlobalRsaPublicKeySavePath { get; set; }

        /// <summary>
        /// RSA�ǶԳ���Կ˽Կ����·��
        /// </summary>
        private static string? GlobalRsaPrivateKeySavePath { get; set; }


        public static void Main(string[] args)
        {

            //�� ASP.NET Core 3.x ��֮ǰ�汾�У�Ӧ�ó���Ĵ���������ͨ��ͨ�� WebHostBuilder �� HostBuilder ����ɡ�
            //Ȼ������ ASP.NET Core 3.1 ��ʼ���ر����� ASP.NET Core 5 �͸��߰汾�У�������һ�ָ������﷨������������Ӧ�ó���
            //��ͨ����ͨ������ WebApplication.CreateBuilder(args) ��ʵ�ֵģ��� ASP.NET Core 6 �����߰汾�У���
            //ASP.NET Core 6 �����߰汾�� ASP.NET Core 6 �����߰汾�У�Program.cs �ļ��Ľṹ�����������仯
            //��֧�ָ����Ķ������͸�����Ӧ�ó������á�
            //�������µĽṹ�У�WebApplication.CreateBuilder(args) �����ڴ���һ���µ� WebApplicationBuilder ʵ��
            //��ʵ����������Ӧ�ó���ĸ������棬������־��¼������ע�루DI�������еķ���ע��ȡ�
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddHttpContextAccessor();

            //AddControllersWithViews �� ASP.NET Core MVC ����е�һ��������չ����
            //��������Ӧ�ó���ķ��񼯺ϣ�IServiceCollection������Ӷ� MVC ��������֧��,������Щ������֧����ͼ��Views����
            //�������ͨ������� ASP.NET Core Ӧ�ó��������������ʹ�ã��ر�����ʹ�� MVC �ܹ�ʱ��
            //������� AddControllersWithViews ʱ��ASP.NET Core ������ MVC �����֧�ֿ���������ͼ��
            //����ζ�����Ӧ�ó����ܹ����������û��� HTTP ���󣬲�����Щ����·�ɵ���Ӧ�� MVC ��������
            //��������ִ��һЩ�߼����������ܷ���һ����ͼ��View��������ͼ���ᱻ��Ⱦ�� HTML �����͸��ͻ��ˡ�
            //ʹ�ó���
            //����� ASP.NET Core Ӧ�ó���ʹ�� MVC �ܹ�����������Ҫ���������������󲢷��ذ�����ͼ����Ӧʱ��
            //������Ҫ���� MVC ����ṩ��·�ɡ�ģ�Ͱ󶨡�ģ����֤���������ȹ���ʱ��
            builder.Services.AddControllersWithViews(configure =>
            {
                configure.Filters.Add<GlobalFilter>();
            }).AddJsonOptions(configure =>
            {
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

            //AddSingleton �� ASP.NET Core���Լ����㷺��.NET Core ��.NET Framework ������ע�루DI���������е�һ������
            //���������������ע��һ����������������Ϊ������Singleton����
            //����ζ����Ӧ�ó�����������������ڣ�����������ٴΣ��÷����ʵ����������ͬ�ġ�
            //ʹ�ó���
            //�����Ӧ�ó�������һЩ��Դ��������Ϣ��������Ӧ�ó��������������ֻ��Ҫ�����ػ��ʼ��һ��ʱ��ʹ�õ���ģʽ�Ƿǳ����ʵġ�
            //������Щ��Ҫά��״̬��״̬�ĸ���������Ӧ�ó����ж���Ҫ����֪�ķ��񣬵���ģʽҲ��һ���õ�ѡ��
            //ע�ᶯ̬���ɷֲ�ʽ��������
            builder.Services.AddSingleton<IIdGenerator<long>>(service =>
            {
                var configuration = builder.Configuration;
                int generatorId = Convert.ToInt32(configuration["GeneratorIdClient"]);
                return new IdGenerator(generatorId, IdGeneratorOptions.Default);
            });

            //AddDataProtection �� ASP.NET Core ��������Ӧ�ó���ķ��񼯺ϣ�IServiceCollection��������ݱ�������ķ�����
            //���ݱ���������Ҫ���ڱ���Ӧ�ó����е��������ݣ����û�ƾ�ݡ����Ƶȡ�
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

            //ע�᷽�����ӷ���
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

            //AddSwaggerGen �� ASP.NET Core �����ڼ��� Swagger�����ڳ�Ϊ OpenAPI����һ����չ����
            //������ Swashbuckle.AspNetCore ����
            //Swagger ��һ���淶�������Ŀ�ܣ��������ɡ����������úͿ��ӻ� RESTful ���� Web ����
            //ͨ�� AddSwaggerGen������Խ� Swagger ���ɵ���� ASP.NET Core Ӧ���У��Ӷ��Զ����� API �ĵ�
            //����� API �Ŀ��������Ժ����Ѷ��Ƿǳ��а����ġ�
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

            //������־��̨�̷߳���
            var logService = AppHost.Services.GetRequiredService<ILogService>();
            logService.InitLogService();

            //����HTTP����ܵ���
            if (AppHost.Environment.IsDevelopment())
            {

                AppHost.UseDeveloperExceptionPage();

                //UseExceptionHandler �� ASP.NET Core �е�һ���м����Middleware��
                //����ȫ�ִ���Ӧ�ó����е��쳣��
                //����м�����Բ����� ASP.NET Core ������ܵ��з�����δ�����쳣�����ṩһ��ͳһ�Ĵ������
                AppHost.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.


                // UseHsts����ʵ�� HTTP �ϸ��䰲ȫЭ�飨HTTP Strict Transport Security����� HSTS����
                // HSTS ��һ�ְ�ȫ���ܣ���������վͨ�� HTTP ��Ӧͷ��������������ڽ�������һ��ʱ���ڣ�ͨ���Ǽ����£�
                // ����վ������ͨ�Ŷ�����ͨ�� HTTPS ���У������� HTTP��
                // �������ڷ�ֹ�м��˹������������͵����繥������Ϊ��ȷ�����û�����վ֮���ͨ��ʼ���Ǽ��ܵġ�
                //�� ASP.NET Core Ӧ���У�UseHsts �м��ͨ������ӵ�Ӧ�õ�������ܵ��У���ȷ�����к���������ͨ�� HTTPS ���С�
                //����м��Ӧ�ñ������� HTTPS �ض����м����UseHttpsRedirection��֮����ȷ�������� HSTS ֮ǰ��
                //���з� HTTPS �����Ѿ����ض��� HTTPS��
                //AppHost.UseHsts();

                //UseSwagger ������ASP.NET CoreӦ�õ�������ܵ�������Swagger���ɵ�JSON�ĵ��ս�㡣
                //���JSON�ĵ�������API��Ԫ���ݣ�����·�ɡ���������Ӧ����Ϣ����Swagger UI��Swagger�����������Ļ�����
                AppHost.UseSwagger();

                //UseSwaggerUI �� ASP.NET Core ���������� Swagger UI ���м�����÷�����
                //Swagger UI ��һ������ Web ���û����棬������ʾ Swagger ���ɵ� API �ĵ�
                //���������߽��� API �Ľ���ʽ���ԡ����� ASP.NET Core Ӧ���м����� Swagger ��
                //UseSwaggerUI ����ʹ�ÿ����߿���ͨ����������� Swagger UI ҳ��Ӷ�����ز鿴 API ����ϸ��Ϣ�������������Ӧ�Ĳ��Եȡ�
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
                        var logger = context.RequestServices.GetRequiredService<ILogService>();

                        // ��¼�쳣  
                        logger.LogError(exceptionHandlerPathFeature.Error, "An unhandled exception has occurred.");
                        // ����HTTP 500��Ӧ  
                        context.Response.ContentType = "application/json";
                        OperateResult operateResult = OperateResult.CreateFailResult<object>("����˳����ˣ���ʱ�޷�����");
                        operateResult.ErrorCode = 99999;
                        await context.Response.WriteAsync(new JsonResult(operateResult)
                        {
                            StatusCode = StatusCodes.Status500InternalServerError,
                        }.ToString());
                    }
                });
            });

            // UseHttpsRedirection �� ASP.NET Core �е�һ���м����Middleware���������ڽ� HTTP �����ض��� HTTPS��
            // ������ǿ��վ��ȫ�Ե�һ��������������Ϊ HTTPS ͨ�� SSL/ TLS Э���ṩ�˼��ܵ�ͨ��ͨ�������Ա������ݵĻ����Ժ������ԡ�
            //�� ASP.NET Core Ӧ���У�UseHttpsRedirection �м��ͨ������ӵ�Ӧ�õ�������ܵ���
            //��ȷ�����зǰ�ȫ�� HTTP ���󶼱��Զ��ض��� HTTPS��
            //���������Է�ֹ������Ϣ�����û�ƾ�ݡ��Ự���Ƶȣ�����������������ʽ���䣬�Ӷ����ػ��۸ġ�
            //AppHost.UseHttpsRedirection();

            //UseAuthentication����֤����
            //���ã�UseAuthentication�м��������֤�û���ݡ�
            //�����鴫���HTTP�����Ƿ������Ч�������֤ƾ�ݣ���Cookie��JWT��JSON Web Tokens���ȡ�
            //�������̣���������а�����Ч�������֤ƾ�ݣ�UseAuthentication�м���Ὣ��Щƾ�ݽ���Ϊ�û������Ϣ
            //������洢��HttpContext.User�У����������м���Ϳ�����ʹ�á�
            //���������û����Ч�������֤ƾ�ݣ��м�����ܻὫ�û��ض��򵽵�¼ҳ��򷵻�401δ��Ȩ��Ӧ��
            //��Ҫ�ԣ�UseAuthentication����Ȩ��UseAuthorization��֮ǰ�Ļ�������
            //��Ϊ��Ȩ������Ҫ֪���û�����ݲ����ж����Ƿ���Ȩ�����ض���Դ��
            AppHost.UseAuthentication();

            // MapControllers ������
            //������·�ɣ�MapControllers �������Զ�ɨ��Ӧ���е� MVC ������
            //�����ݿ������ϵ�·�����ԣ���[Route]��[HttpGet]��[HttpPost] �ȣ�������·�ɡ�
            //����ζ�ſ����߲���Ҫ�� Startup.cs �ļ�����ʽ��Ϊÿ�����������������·��ģ�塣
            //����ԣ�ͨ��ʹ������·�ɣ�MapControllers �ṩ�˼��ߵ������
            //����������������ʽ����·��ģ�壬��������ֱ��Ӧ���ڿ������������
            //�����ã����� Startup.cs �ļ����ֶ�����·����ȣ�MapControllers ���������� MVC Ӧ�õ�·�����ù��̡�
            AppHost.MapControllers();

            //MapControllerRoute ������
            //MapControllerRoute ��������������Ӧ�õ�·�ɱ������һ������·�ɹ���
            //��Щ����ָ������ν������ URL ����ӳ�䵽�ض��� MVC �������Ͳ�����
            //ÿ��·�ɹ��򶼰���һ�����ơ�һ�� URL ģʽ��һ��Ĭ��ֵ���ϣ�����ָ���� URL ��ģʽƥ��ʱӦ�õ��õĿ������Ͳ�����
            AppHost.MapControllerRoute(
              name: "default",
              pattern: "{controller=Home}/{action=Index}/{id?}");

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

            //UseAuthorization����Ȩ����
            //���ã�UseAuthorization�м��������Ȩ�û�������Դ���������û����Ѿ�ͨ��UseAuthentication��֤���Ƿ���з����ض���Դ��Ȩ�ޡ�
            //�������̣�����û����з���Ȩ�ޣ������������������Դ������û�û�з���Ȩ�ޣ��򷵻�403��ֹ������Ӧ��
            //�����ԣ���ʹ��UseAuthorization�м��֮ǰ�������ȵ���UseAuthentication�м������ȷ���û��ѱ���֤��
            //��ע��
            //����ж�Ӧ�ó���ĵ��á�ʹ��Routing������Ӧ�ó���UseEndpoints����������Ӧ�ó���ĵ��á�
            //UseAuthorization���������������֮�䡣
            AppHost.UseAuthorization();

            // UseEndpoints ������
            //�ս�����ã�UseEndpoints ��������������Ӧ�õ�������ܵ��������ս�㡣
            //��ͨ���漰��ָ����Щ URL ģʽӦ�ñ�ӳ�䵽��Щ�������ϡ�
            //·�ɼ��ɣ��� UseRouting �м��һ��ʹ��ʱ��UseEndpoints ����ִ����·��ƥ�䵽���ս���������ί�л������
            //UseRouting ��������ӳ�䵽�ս�㣬�� UseEndpoints ����ִ����Щ�ս�㡣
            //����ԣ�UseEndpoints �ṩ����������ѡ��
            //��������Ϊ��ͬ���͵Ĵ��������� MVC ��������Razor ҳ�桢SignalR �������ȣ�����·�ɹ���
            AppHost.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

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
            var cryptographyService = appHost.Services.GetRequiredService<ICryptographyService>();
            var configuration = appHost.Services.GetRequiredService<IConfiguration>();
            var memoryCache = appHost.Services.GetRequiredService<IMemoryCache>();

            string globalRsaPublicKeyFileName = configuration["ConnectionStrings:GlobalRsaPublicKeyFileName"];
            string globalRsaPrivateKeyFileName = configuration["ConnectionStrings:GlobalRsaPrivateKeyFileName"];

            GlobalRsaPublicKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRsaPublicKeyFileName);
            GlobalRsaPrivateKeySavePath = Path.Combine(Directory.GetCurrentDirectory(), KeysRepository, globalRsaPrivateKeyFileName);

            //����ǵ�һ�ͻᴴ����Կ��˽Կ
            cryptographyService.InitServerRSAKey(GlobalRsaPublicKeySavePath, GlobalRsaPrivateKeySavePath);

            //���ع�Կ��˽Կ
            var rsaPublicKey = cryptographyService.GetRSAPublicKey(GlobalRsaPublicKeySavePath).Data;
            var rsaPrivateKey = cryptographyService.GetRSAPrivateKey(GlobalRsaPrivateKeySavePath).Data;
            memoryCache.Set(MemoryCacheKey.GlobalRSAPublicKey, rsaPublicKey);
            memoryCache.Set(MemoryCacheKey.GlobalRSAPrivateKey, rsaPrivateKey);
        }
    }
}