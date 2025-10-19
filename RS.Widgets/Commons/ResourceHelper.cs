using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RS.Widgets.Commons
{
    public static class ResourceHelper
    {

        public static string ReadAsString(Assembly assembly, string resourcePath)
        {
            using (var stream = GetResourceStream(assembly, resourcePath))
            {
                using (var reader = new StreamReader(stream))
                {
                    return reader.ReadToEnd();
                }
            }
        }


        public static string ReadAsString(string resourcePath)
        {
            return ReadAsString(Assembly.GetExecutingAssembly(), resourcePath);
        }


        public static byte[] ReadAsBytes(Assembly assembly, string resourcePath)
        {
            using (var stream = GetResourceStream(assembly, resourcePath))
            {
                byte[] buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return buffer;
            }
        }


        public static byte[] ReadAsBytes(string resourcePath)
        {
            return ReadAsBytes(Assembly.GetExecutingAssembly(), resourcePath);
        }


        public static Stream GetResourceStream(Assembly assembly, string resourcePath)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException(
                    nameof(assembly),
                    string.Format("Assembly cannot be null. Parameter name: {0}", nameof(assembly))
                );
            }

            if (string.IsNullOrWhiteSpace(resourcePath))
            {
                throw new ArgumentException(
                    string.Format("Resource path cannot be null or whitespace. Path provided: '{0}'", resourcePath),
                    nameof(resourcePath)
                );
            }

            string assemblyName = assembly.GetName().Name;
            string fullResourceName = $"{assemblyName}.{resourcePath.Replace("/", ".").Replace("\\", ".")}";
            Stream stream = assembly.GetManifestResourceStream(fullResourceName);

            if (stream == null)
            {
                foreach (var resourceName in assembly.GetManifestResourceNames())
                {
                    if (resourceName.EndsWith(resourcePath, StringComparison.OrdinalIgnoreCase))
                    {
                        fullResourceName = resourceName;
                        stream = assembly.GetManifestResourceStream(resourceName);
                        break;
                    }
                }
            }

            if (stream == null)
            {
                throw new FileNotFoundException(
                    string.Format(
                        "Embedded resource not found. Assembly: '{0}', Resource path: '{1}', Full name searched: '{2}'",
                        assemblyName,
                        resourcePath,
                        fullResourceName
                    )
                );
            }

            return stream;
        }


        public static string[] GetAllResourceNames(Assembly assembly)
        {
            if (assembly == null)
            {
                return Array.Empty<string>();
            }
            else
            {
                return assembly.GetManifestResourceNames();
            }
        }
    }
}
