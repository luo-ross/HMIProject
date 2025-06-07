using Microsoft.AspNetCore.Mvc.Filters;
using RS.Commons.Attributs;
using RS.HMIServer.Controllers;

namespace RS.HMIServer.Filters
{
    /// <summary>
    /// 异步Action过滤器：用于在Action方法执行前后进行异步自定义处理
    /// </summary>
    public class AsyncActionFilter : IAsyncActionFilter
    {
        /// <summary>
        /// Action方法执行前后触发（支持异步操作）
        /// </summary>
        /// <param name="context">Action执行上下文</param>
        /// <param name="next">委托，用于执行下一个中间件或Action本身</param>
        /// <returns>异步任务</returns>
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            // Action执行前逻辑（如异步日志、参数校验等）
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var controller = context.Controller as BaseController;
            if (controller != null)
            {
                controller.ViewData["TimeStamp"] = timeStamp;
            }

            if (context.HttpContext.Request.Query.TryGetValue("ClientId", out var clientId))
            {
                controller.ViewData["ClientId"] = clientId;
            }

            // 执行Action（必须await，否则Action不会被执行）
            var resultContext = await next();

           
        }

      
    }
}
