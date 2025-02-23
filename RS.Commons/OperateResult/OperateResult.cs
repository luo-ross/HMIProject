using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons
{
    /// <summary>
    /// 操作结果类
    /// </summary>
    public class OperateResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OperateResult()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorCode">错误码</param>
        /// <param name="message">错误消息</param>
        public OperateResult(int errorCode, string message)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
        }

        /// <summary>
        /// 构成函数
        /// </summary>
        /// <param name="message">错误消息</param>
        public OperateResult(string message)
        {
            this.Message = message;
        }

        /// <summary>
        /// 操作是否成功
        /// </summary>
        public bool IsSuccess { get; set; }

        /// <summary>
        /// 消息
        /// </summary>
        public string Message { get; set; }

        /// <summary>
        /// 错误代码
        /// </summary>
        public int ErrorCode { get; set; }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <returns></returns>
        public static OperateResult CreateSuccessResult()
        {
            return new OperateResult()
            {
                IsSuccess = true,
                Message = "OK"
            };
        }

        /// <summary>
        /// 创建成功结果
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="data">数据</param>
        /// <returns></returns>
        public static OperateResult<T> CreateSuccessResult<T>(T data)
        {
            return new OperateResult<T>()
            {
                IsSuccess = true,
                Message = "OK",
                Data = data
            };
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <returns></returns>
        public static OperateResult<T> CreateFailResult<T>()
        {
            return new OperateResult<T>();
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="message">失败消息</param>
        /// <returns></returns>
        public static OperateResult<T> CreateFailResult<T>(string message)
        {
            return new OperateResult<T>(message);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <returns></returns>
        public static OperateResult CreateFailResult()
        {
            return new OperateResult();
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <param name="message">失败消息</param>
        /// <returns></returns>
        public static OperateResult CreateFailResult(string message)
        {
            return new OperateResult(message);
        }

        /// <summary>
        /// 创建失败结果
        /// </summary>
        /// <typeparam name="T">数据类型</typeparam>
        /// <param name="errorOperateResult">错误操作结果</param>
        /// <returns></returns>
        public static OperateResult<T> CreateFailResult<T>(OperateResult errorOperateResult)
        {
            return new OperateResult<T>()
            {
                IsSuccess = errorOperateResult.IsSuccess,
                ErrorCode = errorOperateResult.ErrorCode,
                Message = errorOperateResult.Message
            };
        }
    }

    /// <summary>
    /// 泛型操作结果
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OperateResult<T> : OperateResult
    {
        /// <summary>
        /// 构造函数
        /// </summary>
        public OperateResult() : base()
        {

        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="errorCode">错误代码</param>
        /// <param name="message">错误消息</param>
        public OperateResult(int errorCode, string message) : base(errorCode, message)
        {
            this.ErrorCode = errorCode;
            this.Message = message;
        }

        /// <summary>
        /// 构造函数
        /// </summary>
        /// <param name="message">错误消息</param>
        public OperateResult(string message) : base(message)
        {
            this.Message = message;
        }

        /// <summary>
        /// 返回的泛型数据
        /// </summary>
        public T Data { get; set; }

    }

}
