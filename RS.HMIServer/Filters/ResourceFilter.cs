using Microsoft.AspNetCore.Mvc.Filters;
using RS.Commons.Attributs;

namespace RS.HMIServer.Filters
{
    /// <summary>
    /// 资源过滤器：用于在资源获取前后进行自定义处理
    /// </summary>
    public class ResourceFilter : IResourceFilter
    {
        /// <summary>
        /// 资源获取前触发
        /// </summary>
        /// <param name="context">资源执行上下文</param>
        public void OnResourceExecuting(ResourceExecutingContext context)
        {
           
        }

        /// <summary>
        /// 资源获取后触发
        /// </summary>
        /// <param name="context">资源执行上下文</param>
        public void OnResourceExecuted(ResourceExecutedContext context)
        {
           
        }

        
    }
}
