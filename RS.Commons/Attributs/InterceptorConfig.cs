using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Attributs
{
    /// <summary>
    /// 拦截器配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class| AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class InterceptorConfig : Attribute
    {
        /// <summary>
        /// 是否需要进行拦截 这个级别最高
        /// </summary>
        public bool IsInterceptor { get; set; } = true;

        /// <summary>
        /// 是否进行鉴权拦截
        /// </summary>
        public bool IsAuthInterceptor { get; set; }=true;

        /// <summary>
        /// 是否进行日志记录拦截
        /// </summary>
        public bool IsLogInterceptor { get; set; } = false;

        /// <summary>
        /// 是否进行异常处理拦截
        /// </summary>
        public bool IsExceptionInterceptor { get; set; } = true;

        public InterceptorConfig( )
        { 
        }
    }

}
