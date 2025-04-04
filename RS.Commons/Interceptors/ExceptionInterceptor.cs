using Castle.DynamicProxy;

namespace RS.Commons.Interceptors
{
    /// <summary>
    /// 异常拦截器
    /// </summary>
    public class ExceptionInterceptor : IInterceptor, IAsyncInterceptor
    {
        private readonly ILogBLL LogBLL;
        public ExceptionInterceptor(ILogBLL logBLL)
        {
            LogBLL = logBLL;
        }

        public void Intercept(IInvocation invocation)
        {
            this.ToInterceptor().Intercept(invocation);
        }

        /// <summary>
        /// 同步拦截器
        /// </summary>
        /// <param name="invocation"></param>
        public void InterceptSynchronous(IInvocation invocation)
        {
            try
            {
                invocation.Proceed();
            }
            catch (Exception ex)
            {
                var returnType = invocation.Method.ReturnType;
                if (returnType is OperateResult)
                {
                    var returnTypeInstance = (OperateResult)Activator.CreateInstance(returnType);
                    returnTypeInstance.Message = "出错啦，暂时无法访问";
                    returnTypeInstance.ErrorCode = 9999;
                    invocation.ReturnValue = returnTypeInstance;
                }
                LogBLL.LogCritical(ex, $"{invocation.Method.Name}产生异常未处理");
            }
        }

        public void InterceptAsynchronous(IInvocation invocation)
        {
            //这个接口就可以不用处理
        }

        /// <summary>
        /// 异步拦截器
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="invocation"></param>
        public async void InterceptAsynchronous<TResult>(IInvocation invocation)
        {
            invocation.Proceed();
            //说明调用任务产生异常了 并且这个异常是没有在方法里主动解决，这个异常是未知的
            if (invocation.ReturnValue is Task task && task.IsFaulted && task.Exception != null)
            {
                var returnType = typeof(TResult);
                if (returnType is OperateResult)
                {
                    var returnTypeInstance = (OperateResult)Activator.CreateInstance(returnType);
                    returnTypeInstance.Message = "错误啦，暂时无法访问";
                    returnTypeInstance.ErrorCode = 9999;
                    TaskCompletionSource<TResult> taskCompletionSource = new TaskCompletionSource<TResult>();
                    taskCompletionSource.GetType().GetMethod("TrySetResult").Invoke(taskCompletionSource, new[] { returnTypeInstance });
                    invocation.ReturnValue = taskCompletionSource.Task;
                }
                LogBLL.LogCritical(task.Exception, $"{invocation.Method.Name}产生异常未处理");
            }

        }
    }
}
