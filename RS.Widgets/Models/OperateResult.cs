using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    /// <summary>
    /// 这里是向HSL的Richard 胡工学习的
    /// </summary>
    public class OperateResult
    {
        /// <summary>
        /// 是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 默认0 就是无错误
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 返回消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 返回异常
        /// </summary>
        public Exception Exception { get; set; }

        /// <summary>
        /// 返回的数据
        /// </summary>
        public object Data { get; set; }

        public static OperateResult CreateSuccessResult()
        {
            return new OperateResult()
            {
                IsSuccess=true,
                ErrorCode = 0,
                Message = "成功",
                Data = null,
            };
        }

        public static OperateResult CreateErrorResult(int errorCode, string errorMsg, Exception exception)
        {
            return new OperateResult()
            {
                IsSuccess= false,
                ErrorCode = errorCode,
                Exception = exception,
                Message = errorMsg,
            };
        }
    }
}
