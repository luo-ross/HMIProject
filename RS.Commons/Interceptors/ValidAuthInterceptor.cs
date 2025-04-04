using Castle.DynamicProxy;

namespace RS.Commons.Interceptors
{
    /// <summary>
    /// 异常拦截器
    /// </summary>
    public class ValidAuthInterceptor : IInterceptor
    {
        private readonly ILogBLL LogBLL;
        public ValidAuthInterceptor(ILogBLL logBLL)
        {
            LogBLL = logBLL;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                LogBLL.LogInformation($"鉴权拦截:{invocation.Method.Name} 触发");
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                LogBLL.LogCritical($"{invocation.Method.Name} 异常：{ex.ToString()}");
            }

        }
    }
}
