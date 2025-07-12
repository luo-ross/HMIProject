using Microsoft.Extensions.DependencyInjection;
using RS.Commons.Attributs;
using RS.Widgets.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Models
{
    [ServiceInjectConfig(typeof(IViewModelManager), ServiceLifetime.Singleton)]
    public class ViewModelManager : IViewModelManager
    {
        private readonly IServiceProvider ServiceProvider;

        public ViewModelManager(IServiceProvider serviceProvider)
        {
            ServiceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }


        public Type? GetViewModelType(string viewKey)
        {
            Type? resolvedType = this.ResolveType(viewKey);
            return resolvedType;
        }

        public object GetViewModel(string viewKey)
        {
            Type? viewModelType = GetViewModelType(viewKey);
            if (viewModelType == null)
            {
                return null;
            }
            return ServiceProvider.GetRequiredService(viewModelType);
        }


        public T? GetViewModel<T>(string viewKey) where T : INotifyPropertyChanged
        {
            var viewModel = GetViewModel(viewKey);
            if (viewModel == null)
            {

                return default(T);
            }
            return (T)viewModel;
        }



        public Type? ResolveType(string formattedType)
        {
            if (string.IsNullOrEmpty(formattedType))
            {
                return null;
            }

            int separatorIndex = formattedType.IndexOf('/');
            if (separatorIndex < 0)
            {
                return null;
            }

            string assemblyName = formattedType.Substring(0, separatorIndex);
            string typePart = formattedType.Substring(separatorIndex + 1);
            string fullTypeName = $"{assemblyName}.{typePart}";
            return LoadTypeFromAssembly(assemblyName, fullTypeName);
        }

        private Type? LoadTypeFromAssembly(string assemblyName, string fullTypeName)
        {
            var assembly = AppDomain.CurrentDomain.GetAssemblies()
                .FirstOrDefault(a => a.GetName().Name == assemblyName);

            if (assembly != null)
            {
                return assembly.GetType(fullTypeName);
            }

            string[] searchDirectories = new[] {
                AppDomain.CurrentDomain.BaseDirectory,
                Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Plugins")
            };

            foreach (string directory in searchDirectories)
            {
                string assemblyPath = Path.Combine(directory, $"{assemblyName}.dll");
                if (File.Exists(assemblyPath))
                {
                    try
                    {
                        assembly = Assembly.LoadFrom(assemblyPath);
                        return assembly.GetType(fullTypeName);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"加载程序集失败: {assemblyPath}, 错误: {ex.Message}");
                    }
                }
            }

            return null;
        }


        public static string FormatType(Type type)
        {
            if (type == null)
            {
                throw new ArgumentNullException(nameof(type));
            }

            string assemblyName = type.Assembly.GetName().Name ?? string.Empty;
            string fullNamespace = type.Namespace ?? string.Empty;
            string namespaceWithoutAssembly = RemoveAssemblyPrefix(fullNamespace, assemblyName);

            string typeName = string.IsNullOrEmpty(namespaceWithoutAssembly)
                ? type.Name
                : $"{namespaceWithoutAssembly}.{type.Name}";

            return $"{assemblyName}/{typeName}";
        }

        private static string RemoveAssemblyPrefix(string fullNamespace, string assemblyName)
        {
            string prefix = $"{assemblyName}.";
            if (fullNamespace.StartsWith(prefix))
            {
                return fullNamespace.Substring(prefix.Length);
            }
            return fullNamespace;
        }



    }
}
