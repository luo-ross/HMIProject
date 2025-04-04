using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using RS.Commons.Attributs;
using RS.Models;
using System.Collections.Concurrent;

namespace RS.Commons
{
    /// <summary>
    /// 日志服务
    /// </summary>
    [ServiceInjectConfig(typeof(ILogBLL), ServiceLifetime.Singleton)]
    public class LogBLL : ILogBLL
    {
        private ILogger<LogBLL> Logger;
        private static ConcurrentBag<LogModel> LogModelDataSource;
        private Thread HandleLogThread;
        private bool IsEndLogEvent;
        static LogBLL()
        {
            LogModelDataSource = new ConcurrentBag<LogModel>();
        }
        public LogBLL(ILogger<LogBLL> logger)
        {
            Logger = logger;
            HandleLogThread = new Thread(HandleLogEvent);
            HandleLogThread.IsBackground = true;
            HandleLogThread.Start();
        }

        private void HandleLogEvent()
        {
            while (!IsEndLogEvent)
            {
                if (LogModelDataSource.Count > 0 && LogModelDataSource.TryTake(out LogModel logModel))
                {
                    switch (logModel.LogLevel)
                    {
                        case LogLevel.Trace:
                            break;
                        case LogLevel.Debug:
                            break;
                        case LogLevel.Information:
                            Logger.LogInformation(logModel.Message);
                            break;
                        case LogLevel.Warning:
                            Logger.LogWarning(logModel.Message);
                            break;
                        case LogLevel.Error:
                            Logger.LogError(logModel.Exception, logModel.Message);
                            break;
                        case LogLevel.Critical:
                            Logger.LogCritical(logModel.Exception, logModel.Message);
                            break;
                        case LogLevel.None:
                            break;
                        default:
                            break;
                    }
                }

                if (LogModelDataSource.Count == 0)
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// 异常消息日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        public void LogInformation(Exception exception, string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                Exception = exception,
                LogLevel = LogLevel.Information,
                Message = message
            });
        }

        /// <summary>
        /// 消息日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public void LogInformation(string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                LogLevel = LogLevel.Information,
                Message = message
            });
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        public void LogWarning(Exception exception, string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                Exception = exception,
                LogLevel = LogLevel.Warning,
                Message = message
            });
        }

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public void LogWarning(string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                LogLevel = LogLevel.Warning,
                Message = message
            });
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        public void LogError(Exception exception, string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                Exception = exception,
                LogLevel = LogLevel.Error,
                Message = message
            });
        }

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public void LogError(string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                LogLevel = LogLevel.Error,
                Message = message
            });
        }

        /// <summary>
        /// 致命错误日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        public void LogCritical(Exception exception, string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                Exception = exception,
                LogLevel = LogLevel.Critical,
                Message = message
            });
        }

        /// <summary>
        /// 致命错误日志
        /// </summary>
        /// <param name="message">日志内容</param>
        public void LogCritical(string message)
        {
            LogModelDataSource.Add(new LogModel()
            {
                LogLevel = LogLevel.Critical,
                Message = message
            });
        }
    }
}
