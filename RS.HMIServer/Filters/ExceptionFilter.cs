using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using RS.Commons;
using System.Net;

namespace RS.HMIServer.Filters
{
    /// <summary>
    /// 全局异常过滤器：用于捕获和处理未处理的异常，统一返回友好错误信息并记录日志
    /// </summary>
    public class ExceptionFilter : IExceptionFilter
    {
        private readonly ILogBLL LogBLL;

        /// <summary>
        /// 构造函数，注入日志服务
        /// </summary>
        public ExceptionFilter(ILogBLL logBLL)
        {
            LogBLL = logBLL;
        }

        /// <summary>
        /// 异常发生时触发
        /// </summary>
        /// <param name="context">异常上下文</param>
        public void OnException(ExceptionContext context)
        {
            // 记录异常日志
            this.LogBLL.LogCritical(context.Exception, context.ActionDescriptor.DisplayName);

            // 构造统一的错误返回结果
            OperateResult operateResult = OperateResult.CreateFailResult<object>("内部错误，暂时无法访问");
            operateResult.ErrorCode = 99999;
            context.Result = new JsonResult(operateResult)
            {
                StatusCode = (int)HttpStatusCode.ExpectationFailed
            };
        }
    }
}
