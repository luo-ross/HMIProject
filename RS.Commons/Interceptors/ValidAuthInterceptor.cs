﻿using Castle.DynamicProxy;

namespace RS.Commons.Interceptors
{
    /// <summary>
    /// 异常拦截器
    /// </summary>
    public class ValidAuthInterceptor : IInterceptor
    {
        private readonly ILogService LogService;
        public ValidAuthInterceptor(ILogService logService)
        {
            LogService = logService;
        }

        public void Intercept(IInvocation invocation)
        {
            try
            {
                LogService.LogInformation($"鉴权拦截:{invocation.Method.Name} 触发");
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                LogService.LogCritical($"{invocation.Method.Name} 异常：{ex.ToString()}");
            }

        }
    }
}
