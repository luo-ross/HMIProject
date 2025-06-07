using Microsoft.AspNetCore.Mvc.Filters;
using RS.Commons.Attributs;
using RS.HMIServer.Controllers;

namespace RS.HMIServer.Filters
{
    /// <summary>
    /// Action过滤器：用于在Action方法执行前后进行自定义处理
    /// </summary>
    public class ActionFilter : IActionFilter
    {
        /// <summary>
        /// Action方法执行前触发
        /// </summary>
        /// <param name="context">Action执行上下文</param>
        public void OnActionExecuting(ActionExecutingContext context)
        {
            // 动态给每一个请求添加时间戳
            var timeStamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            var controller = context.Controller as BaseController;
            if (controller != null)
            {
                controller.ViewData["TimeStamp"] = timeStamp;
            }

            // 获取ClientId参数并传递到视图
            if (context.HttpContext.Request.Query.TryGetValue("ClientId", out var clientId))
            {
                controller.ViewData["ClientId"] = clientId;
            }
        }

        /// <summary>
        /// Action方法执行后触发
        /// </summary>
        /// <param name="context">Action执行上下文</param>
        public void OnActionExecuted(ActionExecutedContext context)
        {
           
        }
    }
}
