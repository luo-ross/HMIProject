using System.Reflection;

namespace RS.SetupApp.Core;

public static class ExtensionLoader
{
    public static IReadOnlyList<ISetupExtension> Load(ProductManifest product, string productManifestPath, ISetupLogger logger)
    {
        List<ISetupExtension> extensions = new();
        foreach (ExtensionRegistrationManifest extension in product.Extensions)
        {
            try
            {
                string assemblyPath = SetupPathUtility.ResolveManifestRelativePath(productManifestPath, extension.AssemblyPath);
                Assembly assembly = Assembly.LoadFrom(assemblyPath);
                Type? type = assembly.GetType(extension.TypeName, throwOnError: true, ignoreCase: false);
                if (type == null || !typeof(ISetupExtension).IsAssignableFrom(type))
                {
                    throw new InvalidOperationException($"Type '{extension.TypeName}' does not implement ISetupExtension.");
                }

                extensions.Add((ISetupExtension)Activator.CreateInstance(type)!);
            }
            catch (Exception ex)
            {
                if (extension.Optional)
                {
                    logger.Warn("Optional setup extension failed to load.", new { extension.AssemblyPath, extension.TypeName, error = ex.Message });
                    continue;
                }

                throw new InvalidOperationException($"Failed to load required setup extension '{extension.TypeName}'.", ex);
            }
        }

        return extensions;
    }
}
