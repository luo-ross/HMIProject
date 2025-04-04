using Castle.DynamicProxy;

namespace RS.Commons.Interceptors
{
    /// <summary>
    /// 日志拦截器
    /// </summary>
    public class LogInterceptor : IInterceptor
    {

        private readonly ILogBLL LogBLL;
        public LogInterceptor(ILogBLL logBLL)
        {
            LogBLL = logBLL;
        }

        /// <summary>
        /// 拦截器
        /// </summary>
        /// <param name="invocation"></param>
        public void Intercept(IInvocation invocation)
        {
            try
            {
                LogBLL.LogInformation($"日志拦截:{invocation.Method.Name} 触发");
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                LogBLL.LogCritical($"{invocation.Method.Name} 异常：{ex.ToString()}");
            }
        }

       
    }
}
