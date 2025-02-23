using Castle.DynamicProxy;
using Microsoft.Extensions.Logging;
using RS.Commons.Attributs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Interceptors
{
    /// <summary>
    /// 自定义拦截选择器
    /// </summary>
    public class CustomInterceptorSelector : IInterceptorSelector
    {
        private readonly ILogService LogService;
        public CustomInterceptorSelector(ILogService logService)
        {
            LogService = logService;
        }

        /// <summary>
        /// 获取拦截选择器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="method">方法信息</param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        public IInterceptor[] SelectInterceptors(Type type, MethodInfo method, IInterceptor[] interceptors)
        {
            //第一步我们需要来处理最顶级类的拦截筛选
            var defaultInterceptorFilter = HandTypeInterceptorFilter(type, interceptors.ToList());

            //第二步我们再来处理方法级别拦截筛选
            var finalInterceptorFilter = HandleMethodInterceptorFilter(type, method, defaultInterceptorFilter);
            return finalInterceptorFilter.ToArray();
        }

        /// <summary>
        /// 处理方法拦截筛选器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="method">方法</param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        private List<IInterceptor> HandleMethodInterceptorFilter(Type type, MethodInfo method, List<IInterceptor> interceptors)
        {
            var origianlMethod = type.GetMethod(method.Name);
            List<IInterceptor> interceptorFilter = new List<IInterceptor>();
            if (origianlMethod.IsDefined(typeof(InterceptorConfig), false))
            {
                var interceptorConfig = origianlMethod.GetCustomAttribute<InterceptorConfig>();
                interceptorFilter = HandInterceptorFilter(interceptors, interceptorConfig);
            }
            else
            {
                interceptorFilter = interceptors.ToList();
            }
            return interceptorFilter;
        }

        /// <summary>
        /// 处理类的拦截筛选器
        /// </summary>
        /// <param name="type">类型</param>
        /// <param name="interceptors">拦截器</param>
        /// <returns></returns>
        private List<IInterceptor> HandTypeInterceptorFilter(Type type, List<IInterceptor> interceptors)
        {
            List<IInterceptor> interceptorFilter = new List<IInterceptor>();
            if (type.IsDefined(typeof(InterceptorConfig), false))
            {
                var interceptorConfig = type.GetCustomAttribute<InterceptorConfig>();
                interceptorFilter = HandInterceptorFilter(interceptors, interceptorConfig);
            }
            else
            {
                interceptorFilter = interceptors.ToList();
            }
            return interceptorFilter;
        }

        /// <summary>
        /// 根据筛选器处理拦截
        /// </summary>
        /// <param name="interceptors">拦截器</param>
        /// <param name="interceptorConfig">拦截配置</param>
        /// <returns></returns>
        private List<IInterceptor> HandInterceptorFilter(List<IInterceptor> interceptors, InterceptorConfig interceptorConfig)
        {
            List<IInterceptor> interceptorFilter = new List<IInterceptor>();

            if (interceptorConfig == null)
            {
                return interceptorFilter;
            }
            //鉴权拦截器处理
            if (interceptorConfig.IsAuthInterceptor)
            {
                var validAuthInterceptor = interceptors.FirstOrDefault(t => t.GetType() == typeof(ValidAuthInterceptor));
                if (validAuthInterceptor != null)
                {
                    interceptorFilter.Add(validAuthInterceptor);
                }
            }

            //日志拦截器处理
            if (interceptorConfig.IsLogInterceptor)
            {
                var logInterceptor = interceptors.FirstOrDefault(t => t.GetType() == typeof(LogInterceptor));
                if (logInterceptor != null)
                {
                    interceptorFilter.Add(logInterceptor);
                }
            }

            //异常拦截器处理
            if (interceptorConfig.IsExceptionInterceptor)
            {
                var exceptionInterceptor = interceptors.FirstOrDefault(t => t.GetType() == typeof(ExceptionInterceptor));
                if (exceptionInterceptor != null)
                {
                    interceptorFilter.Add(exceptionInterceptor);
                }
            }

            //级别最高的 如果不拦截就全部清除
            if (!interceptorConfig.IsInterceptor)
            {
                interceptorFilter.Clear();
            }

            return interceptorFilter;
        }
    }
}
