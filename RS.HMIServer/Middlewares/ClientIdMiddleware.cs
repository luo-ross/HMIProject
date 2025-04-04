using Microsoft.IdentityModel.Tokens;
using NuGet.Protocol.Core.Types;
using RS.Commons;
using RS.HMIServer.IBLL;
using RS.HMIServer.Models;

namespace RS.HMIServer.Middlewares
{
    public class ClientIdMiddleware
    {
        private readonly RequestDelegate RequestDelegate;
        private readonly string DefaultRoute;
        private readonly ILogBLL LogBLL;
        private readonly IGeneralBLL GeneralBLL;
        public ClientIdMiddleware(IGeneralBLL generalBLL, ILogBLL logBLL, RequestDelegate requestDelegate, string defaultRoute)
        {
            this.LogBLL = logBLL;
            this.GeneralBLL = generalBLL;
            this.RequestDelegate = requestDelegate;
            this.DefaultRoute = defaultRoute;
        }

        public async Task InvokeAsync(HttpContext context)
        {

            //假如携带了clientId 则需要判断这个clientId的真假
            if (context.Request.Query.TryGetValue("ClientId", out var clientId))
            {
                //验证这个clientId是否合规
                if (string.IsNullOrEmpty(clientId)
                    || string.IsNullOrWhiteSpace(clientId)
                    || clientId.ToString().Length != 36)
                {
                    // 不合规，重定向到拒绝服务页面
                    context.Response.StatusCode = 302;
                    context.Response.Redirect("/AccessDenied.html");
                    return;
                }

                string remoteIpAddress = context.Connection.RemoteIpAddress.ToString();
                string localIpAddress = context.Connection.LocalIpAddress.ToString();
                string xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
                string userAgent = context.Request.Headers["User-Agent"].ToString();
                //验证的话还需要获取用户的IP地址 以防止串用
                OperateResult operateResult = await this.GeneralBLL.ValidCliendIdAsync(new LoginClientModel()
                {
                    LocalIpAddress = localIpAddress,
                    RemoteIpAddress = remoteIpAddress,
                    UserAgent = userAgent,
                    XForwardedFor = xForwardedFor
                }, clientId);
                //如果验证不通过 不好意思 退到登录去 或者跳转到拒绝服务业
                if (!operateResult.IsSuccess)
                {
                    if (operateResult.ErrorCode >= 1000000)
                    {
                        context.Response.Redirect("/ServerError.html");
                        return;
                    }
                    else
                    {
                        // 不合规，重定向到拒绝服务页面
                        context.Response.StatusCode = 302;
                        context.Response.Redirect("/AccessDenied.html");
                    }
                    return;
                }
                await this.RequestDelegate(context);
                return;
            }

            var routeValues = context.Request.RouteValues;
            var controller = routeValues["controller"]?.ToString();
            var action = routeValues["action"]?.ToString();

            //如果是默认页 则不进行处理
            if (!string.IsNullOrEmpty(controller)
               && !string.IsNullOrEmpty(action)
               && controller.Equals("Login")
               && action.Equals("Default"))
            {
                await this.RequestDelegate(context);
                return;
            }

            context.Response.Redirect(this.DefaultRoute);
        }
    }
}
