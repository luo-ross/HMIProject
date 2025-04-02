using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RS.Commons;
using RS.Commons.Attributs;
using RS.HMIServer.Areas;
using System.Net;

namespace RS.HMIServer.Filters
{
    /// <summary>
    /// 全局过滤器
    /// </summary>
    public class GlobalFilter : IExceptionFilter, IAuthorizationFilter, IActionFilter, IResourceFilter
    {
        private readonly ILogService LogService;
        public GlobalFilter(ILogService logService)
        {
            LogService = logService;
        }

        /// <summary>
        /// Action执行完出发
        /// </summary>
        /// <param name="context">请求上下文</param>

        public void OnActionExecuted(ActionExecutedContext context)
        {
            var filterConfig = GetFilterConfig(context);
            if (filterConfig != null && !filterConfig.IsActionFilter)
            {
                return;
            }
        }

        /// <summary>
        /// Action开始时出发
        /// </summary>
        /// <param name="context">请求上下文</param>

        public void OnActionExecuting(ActionExecutingContext context)
        {
            //动态给每一个请求添加时间戳
            var timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var controller = context.Controller as BaseController;
            if (controller != null)
            {
                controller.ViewData["Timestamp"] = timestamp;
            }
            var filterConfig = GetFilterConfig(context);
            if (filterConfig != null && !filterConfig.IsActionFilter)
            {
                return;
            }
        }

        /// <summary>
        /// 鉴权出发
        /// </summary>
        /// <param name="context">请求上下文</param>
        public void OnAuthorization(AuthorizationFilterContext context)
        {
            var filterConfig = GetFilterConfig(context);
            if (filterConfig != null && !filterConfig.IsAuthorizationFilter)
            {
                return;
            }
        }

        /// <summary>
        /// 获取过滤器配置
        /// </summary>
        /// <param name="context">请求上下文</param>
        /// <returns></returns>
        private FilterConfig GetFilterConfig(FilterContext context)
        {
            var filterConfig = (FilterConfig)context.ActionDescriptor.EndpointMetadata.FirstOrDefault(t => t.GetType() == typeof(FilterConfig));
            return filterConfig;
        }

        /// <summary>
        /// 异常时触发
        /// </summary>
        /// <param name="context">请求上下文</param>
        public void OnException(ExceptionContext context)
        {
            LogService.LogCritical(context.Exception, context.ActionDescriptor.DisplayName);
            OperateResult operateResult = OperateResult.CreateFailResult<object>("内部错误，暂时无法访问");
            operateResult.ErrorCode = 99999;
            context.Result = new JsonResult(operateResult)
            {
                StatusCode = (int)HttpStatusCode.ExpectationFailed
            };
        }

        /// <summary>
        /// 资源获取完成触发
        /// </summary>
        /// <param name="context">请求上下文</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
            var filterConfig = GetFilterConfig(context);
            if (filterConfig != null && !filterConfig.IsResourceFilter)
            {
                return;
            }
        }

        /// <summary>
        /// 资源获取时触发
        /// </summary>
        /// <param name="context">请求上下文</param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
            var filterConfig = GetFilterConfig(context);
            if (filterConfig != null && !filterConfig.IsResourceFilter)
            {
                return;
            }
        }
    }
}
