using Microsoft.Extensions.Logging;
using RS.Models;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Interceptors
{
    /// <summary>
    /// 日志服务接口
    /// </summary>
    public interface ILogService
    {
        /// <summary>
        /// 初始化日志服务
        /// </summary>
        void InitLogService();

        /// <summary>
        /// 异常消息日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        void LogInformation(Exception exception, string message);

        /// <summary>
        /// 消息日志
        /// </summary>
        /// <param name="message">日志内容</param>
        void LogInformation(string message);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        void LogWarning(Exception exception, string message);

        /// <summary>
        /// 警告日志
        /// </summary>
        /// <param name="message">日志内容</param>
        void LogWarning(string message);


        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        void LogError(Exception exception, string message);

        /// <summary>
        /// 错误日志
        /// </summary>
        /// <param name="message">日志内容</param>
        void LogError(string message);


        /// <summary>
        /// 致命错误日志
        /// </summary>
        /// <param name="exception">异常</param>
        /// <param name="message">日志内容</param>
        void LogCritical(Exception exception, string message);


        /// <summary>
        /// 致命错误日志
        /// </summary>
        /// <param name="message">日志内容</param>
        void LogCritical(string message);

    }
}
