using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Exceptions
{
    /// <summary>
    /// 日志存储回调的异常信息
    /// </summary>
    public class LogNetException : Exception
    {
        /// <summary>
        /// 使用其他的异常信息来初始化日志异常
        /// </summary>
        /// <param name="innerException">异常信息</param>
        public LogNetException(Exception innerException) : base(innerException.Message, innerException)
        {

        }
    }

}
