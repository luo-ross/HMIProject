namespace RS.SetupApp.Core;

public static class SetupPipelineHelper
{
    public static async Task DownloadOrCopyAsync(
        SetupExecutionContext context,
        string source,
        string destinationPath,
        CancellationToken cancellationToken)
    {
        if (Uri.TryCreate(source, UriKind.Absolute, out Uri? uri) && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps))
        {
            await context.Services.Downloads.DownloadAsync(uri, destinationPath, cancellationToken).ConfigureAwait(false);
            return;
        }

        string localPath = source;
        if (Uri.TryCreate(source, UriKind.Absolute, out uri) && uri.IsFile)
        {
            localPath = uri.LocalPath;
        }

        context.Services.FileSystem.CopyFile(localPath, destinationPath, overwrite: true);
    }

    public static string ResolveSourceRelativeToManifest(string manifestSource, string assetSource)
    {
        if (Uri.TryCreate(assetSource, UriKind.Absolute, out Uri? assetUri))
        {
            return assetUri.IsFile ? assetUri.LocalPath : assetUri.ToString();
        }

        if (Uri.TryCreate(manifestSource, UriKind.Absolute, out Uri? manifestUri))
        {
            if (manifestUri.IsFile)
            {
                return Path.Combine(Path.GetDirectoryName(manifestUri.LocalPath) ?? AppContext.BaseDirectory, assetSource);
            }

            return new Uri(manifestUri, assetSource).ToString();
        }

        return Path.Combine(Path.GetDirectoryName(manifestSource) ?? AppContext.BaseDirectory, assetSource);
    }

    public static InstalledStateManifest CreateInstalledState(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        InstalledStateManifest? existingState = context.ExistingState;
        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");
        string maintenanceDirectory = context.Services.Paths.GetMaintenanceDirectory(installDirectory);

        InstalledStateManifest state = new()
        {
            ProductId = product.ProductId,
            DisplayName = product.DisplayName,
            Publisher = product.Publisher,
            Version = package.Version,
            InstallScope = context.EffectiveScope,
            InstallDirectory = installDirectory,
            MainExecutablePath = Path.Combine(installDirectory, package.MainExecutable),
            StateManifestPath = context.Services.Paths.GetStateManifestPath(product.ProductId, context.EffectiveScope),
            MaintenanceDirectory = maintenanceDirectory,
            MaintenanceExecutablePath = Path.Combine(maintenanceDirectory, Path.GetFileName(Environment.ProcessPath ?? "Setup.exe")),
            MaintenanceProductManifestPath = Path.Combine(maintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.ProductManifestFileName),
            MaintenancePackageManifestPath = Path.Combine(maintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, SetupRuntimeDefaults.PackageManifestFileName),
            MaintenancePackagePath = Path.Combine(maintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName, package.ArchiveFileName),
            AutorunEntryName = existingState?.AutorunEntryName ?? SetupPathUtility.SanitizePathSegment(product.ProductId),
            AutorunEnabled = context.Options.NoAutostart ? false : (existingState?.AutorunEnabled ?? product.InstallDefaults.EnableAutoStartByDefault),
            InstalledAtUtc = existingState?.InstalledAtUtc ?? DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow,
            LastSuccessfulInstallAtUtc = existingState?.LastSuccessfulInstallAtUtc ?? DateTimeOffset.UtcNow
        };

        foreach (DataDirectoryManifest directory in product.DataDirectories)
        {
            state.DataDirectories[directory.Key] = existingState != null && existingState.DataDirectories.TryGetValue(directory.Key, out string? existingPath)
                ? existingPath
                : context.Services.Paths.GetDataDirectory(product, context.EffectiveScope, directory);
        }

        return state;
    }

    public static void DeployMaintenanceBundle(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");

        if (context.Services.FileSystem.DirectoryExists(state.MaintenanceDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(state.MaintenanceDirectory, recursive: true);
        }

        context.Services.FileSystem.CopyDirectory(context.Services.Paths.AppBaseDirectory, state.MaintenanceDirectory, overwrite: true);

        string payloadDirectory = Path.Combine(state.MaintenanceDirectory, SetupRuntimeDefaults.DefaultPayloadFolderName);
        context.Services.FileSystem.CreateDirectory(payloadDirectory);
        context.Services.Serializer.Save(Path.Combine(payloadDirectory, SetupRuntimeDefaults.ProductManifestFileName), product);

        if (context.Package != null)
        {
            context.Services.Serializer.Save(Path.Combine(payloadDirectory, SetupRuntimeDefaults.PackageManifestFileName), context.Package);
        }

        if (!string.IsNullOrWhiteSpace(context.PackagePath) && context.Services.FileSystem.FileExists(context.PackagePath))
        {
            context.Services.FileSystem.CopyFile(context.PackagePath, Path.Combine(payloadDirectory, Path.GetFileName(context.PackagePath)), overwrite: true);
        }

        if (!string.IsNullOrWhiteSpace(context.UpdateManifestPath) && context.Services.FileSystem.FileExists(context.UpdateManifestPath))
        {
            context.Services.FileSystem.CopyFile(context.UpdateManifestPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.UpdateManifestFileName), overwrite: true);
        }

        if (!string.IsNullOrWhiteSpace(context.SchemaPath) && context.Services.FileSystem.FileExists(context.SchemaPath))
        {
            context.Services.FileSystem.CopyFile(context.SchemaPath, Path.Combine(payloadDirectory, SetupRuntimeDefaults.ProductSchemaFileName), overwrite: true);
        }

        foreach (string assetPath in GetManifestAssets(product, context.ProductManifestPath))
        {
            if (!context.Services.FileSystem.FileExists(assetPath))
            {
                continue;
            }

            string relative = Path.GetRelativePath(Path.GetDirectoryName(context.ProductManifestPath) ?? context.PayloadDirectory, assetPath);
            context.Services.FileSystem.CopyFile(assetPath, Path.Combine(payloadDirectory, relative), overwrite: true);
        }
    }

    public static void ApplySystemIntegrations(SetupExecutionContext context)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");

        foreach (KeyValuePair<string, string> directory in state.DataDirectories)
        {
            context.Services.FileSystem.CreateDirectory(directory.Value);
        }

        state.Shortcuts = context.Services.Shortcuts.CreateShortcuts(product, state, enabled: !context.Options.NoShortcuts).ToList();
        context.Services.Registry.RegisterInstallerEntries(product, package, state);
    }

    private static IEnumerable<string> GetManifestAssets(ProductManifest product, string productManifestPath)
    {
        List<string?> candidates =
        [
            product.Branding.IconPath,
            product.Branding.LicensePath
        ];
        candidates.AddRange(product.Shortcuts.Select(item => item.IconPath));
        candidates.AddRange(product.FileAssociations.Select(item => item.IconPath));

        return candidates
            .Where(path => !string.IsNullOrWhiteSpace(path))
            .Select(path => SetupPathUtility.ResolveManifestRelativePath(productManifestPath, path))
            .Distinct(StringComparer.OrdinalIgnoreCase);
    }
}
