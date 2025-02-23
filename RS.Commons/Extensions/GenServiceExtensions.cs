
using Castle.DynamicProxy;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using RS.Commons.Attributs;
using RS.Commons.Interceptors;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RS.Commons.Extensions
{
    public static class GenServiceExtensions
    {
        /// <summary>
        /// 注册服务扩展
        /// </summary>
        /// <param name="services">依赖注入服务</param>
        /// <param name="assembly">程序集</param>
        /// <returns></returns>
        public static IServiceCollection RegisterService(this IServiceCollection services, Assembly assembly)
        {
            var typeList = assembly.GetTypes();
            foreach (var type in typeList)
            {
                var serviceInjectList = type.GetCustomAttributes<ServiceInjectConfig>();
                foreach (var serviceInject in serviceInjectList)
                {
                    //处理依赖注入
                    ServiceDescriptor serviceDescriptor = null;
                    if (serviceInject.ServiceKey != null && serviceInject.ServiceType != null)
                    {
                        serviceDescriptor = new ServiceDescriptor(serviceInject.ServiceType ?? type, serviceInject.ServiceKey, type, serviceInject.Lifetime);
                        services.Add(serviceDescriptor);
                    }
                    else
                    {
                        serviceDescriptor = new ServiceDescriptor(serviceInject.ServiceType ?? type, type, serviceInject.Lifetime);
                        services.TryAdd(serviceDescriptor);
                    }

                    //这里是使用Castle Core进行方法拦截
                    if (serviceInject.ServiceType != null && serviceInject.IsInterceptor)
                    {
                        //处理默认服务方法拦截
                        HandleInterceptor(services, serviceInject, serviceDescriptor, type);
                    }
                }
            }
            return services;
        }

        /// <summary>
        /// 注册日志服务
        /// </summary>
        /// <param name="services">依赖注入服务</param>
        /// <returns></returns>
        public static IServiceCollection RegisterLog4netService(this IServiceCollection services)
        {
            services.AddLogging(configure =>
            {
                //configure.AddDebug();
                configure.ClearProviders();
                configure.AddLog4Net("Configs/log4net.config");
                configure.AddFilter((str, logLevel) =>
                {
                    if (str.Contains("Microsoft"))
                    {
                        return false;
                    }
                    return true;
                });
            });
            return services;
        }

        /// <summary>
        /// 注册拦截服务
        /// </summary>
        /// <param name="services">依赖注入服务</param>
        /// <returns></returns>
        public static IServiceCollection RegisterInterceptorService(this IServiceCollection services)
        {
            #region 使用Castle Core进行面向AOP编程处理日志 异常 鉴权等工作
            services.AddSingleton<IProxyGenerator, ProxyGenerator>();
            //拦截器一旦有多个存在 就会形成拦截器链 会按照我们注册服务的顺序触发
            //需要优先级最高的放在最前面
            services.AddTransient<IInterceptor, ValidAuthInterceptor>();
            services.AddTransient<IInterceptor, LogInterceptor>();
            services.AddTransient<IInterceptor, ExceptionInterceptor>();
            //注册拦截筛选器服务
            services.AddTransient<CustomInterceptorSelector, CustomInterceptorSelector>();
            #endregion
            return services;
        }

        /// <summary>
        /// 处理方法拦截
        /// </summary>
        /// <param name="services">依赖注入服务</param>
        /// <param name="serviceInject">注入服务</param>
        /// <param name="serviceShouldRemove">需移除的服务</param>
        /// <param name="type">创建代理类型</param>
        private static void HandleInterceptor(IServiceCollection services, ServiceInjectConfig serviceInject, ServiceDescriptor serviceShouldRemove, Type type)
        {
            services.Remove(serviceShouldRemove);
            if (string.IsNullOrEmpty(serviceInject.ServiceKey))
            {
                var itemNew = new ServiceDescriptor(serviceInject.ServiceType ?? type, serviceProvider =>
                {
                    return GetProxyGenerator(serviceProvider, serviceInject, type);
                }, serviceInject.Lifetime);
                services.Add(itemNew);
            }
            else
            {
                //这里时处理带key值的服务
                var itemNew = new ServiceDescriptor(serviceInject.ServiceType ?? type, serviceInject.ServiceKey, (serviceProvider, b) =>
                {
                    return GetProxyGenerator(serviceProvider, serviceInject, type);
                }, serviceInject.Lifetime);
                services.Add(itemNew);
            }
        }

        /// <summary>
        /// 生成拦截代理
        /// </summary>
        /// <param name="serviceProvider">依赖注入服务</param>
        /// <param name="serviceInject">注入无法</param>
        /// <param name="type">创建代理类型</param>
        /// <returns></returns>
        public static object GetProxyGenerator(IServiceProvider serviceProvider, ServiceInjectConfig serviceInject, Type type)
        {
            var proxyGenerator = serviceProvider.GetService<IProxyGenerator>();
            var interceptorList = serviceProvider.GetServices<IInterceptor>();
            var customInterceptorSelector = serviceProvider.GetService<CustomInterceptorSelector>();
            ProxyGenerationOptions options = new ProxyGenerationOptions();
            options.Selector = customInterceptorSelector;
            //这里动态去创建构造函数
            var constructors = type.GetConstructors().First().GetParameters().Select(t => serviceProvider.GetService(t.ParameterType)).ToArray();
            //动态创建类型实例
            var target = Activator.CreateInstance(type, constructors);
            //动态构建代理服务
            var targetProxy = proxyGenerator.CreateInterfaceProxyWithTarget(serviceInject.ServiceType, target, options, interceptorList.ToArray());
            return targetProxy;
        }
    }
}
