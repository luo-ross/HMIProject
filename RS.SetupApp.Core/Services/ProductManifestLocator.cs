namespace RS.SetupApp.Core;

public static class ProductManifestLocator
{
    public static ProductManifestLocationResult Resolve(
        RuntimeOptions options,
        ISystemPaths paths,
        IFileSystem fileSystem)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(paths);
        ArgumentNullException.ThrowIfNull(fileSystem);

        if (!string.IsNullOrWhiteSpace(options.ProductManifestPath))
        {
            string explicitPath = Path.GetFullPath(options.ProductManifestPath);
            return new ProductManifestLocationResult(explicitPath, [explicitPath], fileSystem.FileExists(explicitPath));
        }

        List<string> candidates =
        [
            Path.Combine(paths.GetPayloadDirectory(), SetupRuntimeDefaults.ProductManifestFileName),
            Path.Combine(paths.AppBaseDirectory, SetupRuntimeDefaults.ProductManifestFileName),
            Path.Combine(Directory.GetCurrentDirectory(), SetupRuntimeDefaults.ProductManifestFileName)
        ];

        string developmentTemplatePath = TryResolveDevelopmentTemplateManifestPath(paths.AppBaseDirectory, fileSystem);
        if (!string.IsNullOrWhiteSpace(developmentTemplatePath))
        {
            candidates.Add(developmentTemplatePath);
        }

        foreach (string candidate in candidates
            .Where(static candidate => !string.IsNullOrWhiteSpace(candidate))
            .Select(Path.GetFullPath)
            .Distinct(StringComparer.OrdinalIgnoreCase))
        {
            if (fileSystem.FileExists(candidate))
            {
                return new ProductManifestLocationResult(candidate, candidates, true);
            }
        }

        return new ProductManifestLocationResult(candidates.FirstOrDefault() ?? SetupRuntimeDefaults.ProductManifestFileName, candidates, false);
    }

    private static string TryResolveDevelopmentTemplateManifestPath(string appBaseDirectory, IFileSystem fileSystem)
    {
        string[] relativeSegments =
        [
            Path.Combine("..", "..", "..", "Templates", "RS.SetupApp.Template", SetupRuntimeDefaults.ProductManifestFileName),
            Path.Combine("..", "..", "..", "..", "Templates", "RS.SetupApp.Template", SetupRuntimeDefaults.ProductManifestFileName),
            Path.Combine("Templates", "RS.SetupApp.Template", SetupRuntimeDefaults.ProductManifestFileName)
        ];

        foreach (string relativeSegment in relativeSegments)
        {
            string candidate = Path.GetFullPath(Path.Combine(appBaseDirectory, relativeSegment));
            if (fileSystem.FileExists(candidate))
            {
                return candidate;
            }
        }

        return string.Empty;
    }
}
