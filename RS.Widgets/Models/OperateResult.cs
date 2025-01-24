using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    public class OperateResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object Data { get; set; }

        public static OperateResult CreateResult()
        {
            return new OperateResult()
            {
                IsSuccess=true,
                Message = "成功",
                Data = null,
            };
        }

        public static OperateResult CreateResult(object data )
        {
            return new OperateResult()
            {
                IsSuccess = true,
                Message = "成功",
                Data = data,
            };
        }
    }

    public class ErrorOperateResult : OperateResult
    {
        /// <summary>
        /// 默认0 就是无错误
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 返回异常
        /// </summary>
        public Exception Exception { get; set; }

        public static OperateResult CreateResult(int errorCode, string errorMsg, Exception exception)
        {
            return new ErrorOperateResult()
            {
                IsSuccess = false,
                ErrorCode = errorCode,
                Exception = exception,
                Message = errorMsg,
            };
        }
    }

    public class WarningOperateResult : OperateResult
    {
        public static OperateResult CreateResult(string warningMsg)
        {
            return new WarningOperateResult()
            {
                IsSuccess = false,
                Message = warningMsg,
            };
        }
    }
}
