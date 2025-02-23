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
    /// 过滤器配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = false, Inherited = false)]
    public class FilterConfig : Attribute
    {
        public bool IsExceptionFilter = true;
        public bool IsAuthorizationFilter = true; 
        public bool IsActionFilter = true; 
        public bool IsResourceFilter = true;
        public FilterConfig()
        {
        }
    }

}
