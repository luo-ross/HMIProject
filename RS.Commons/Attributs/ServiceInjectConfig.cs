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
    /// 依赖注入配置
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true, Inherited = false)]
    public class ServiceInjectConfig : Attribute
    {
        /// <summary>
        /// 服务主键
        /// </summary>
        public string ServiceKey { get; set; }

        /// <summary>
        /// 是否需要进行拦截
        /// </summary>
        public bool IsInterceptor { get; set; }

        /// <summary>
        /// 接口类型
        /// </summary>
        public Type ServiceType { get; set; }

        /// <summary>
        /// 依赖注入服务类型
        /// </summary>
        public ServiceLifetime Lifetime { get; set; }

        public ServiceInjectConfig(Type serviceType, ServiceLifetime lifetime)
        {
            ServiceType=serviceType;
            Lifetime=lifetime;
        }
        public ServiceInjectConfig( ServiceLifetime lifetime)
        {
            Lifetime = lifetime;
        }
    }

}
