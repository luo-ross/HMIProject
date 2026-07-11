namespace RS.SetupApp.Core;

public sealed class DownloadUpdateManifestStep : ISetupStep
{
    public string Name => "Download update manifest";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");

        string? packagePath = context.Options.PackagePath;
        string? packageManifestPath = context.Options.PackageManifestPath;
        if (string.IsNullOrWhiteSpace(packagePath) && context.Services.FileSystem.DirectoryExists(context.PayloadDirectory))
        {
            packagePath = context.Services.FileSystem.EnumerateFiles(context.PayloadDirectory, "*.zip", SearchOption.TopDirectoryOnly)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .FirstOrDefault();
        }

        if (string.IsNullOrWhiteSpace(packageManifestPath))
        {
            string defaultManifestPath = Path.Combine(context.PayloadDirectory, SetupRuntimeDefaults.PackageManifestFileName);
            if (context.Services.FileSystem.FileExists(defaultManifestPath))
            {
                packageManifestPath = defaultManifestPath;
            }
        }

        if (!string.IsNullOrWhiteSpace(packagePath))
        {
            context.PackagePath = Path.GetFullPath(packagePath);
        }

        if (!string.IsNullOrWhiteSpace(packageManifestPath))
        {
            context.PackageManifestPath = Path.GetFullPath(packageManifestPath);
        }

        if (!string.IsNullOrWhiteSpace(context.PackagePath) && !string.IsNullOrWhiteSpace(context.PackageManifestPath))
        {
            context.RequiresOnlinePackage = false;
            return;
        }

        if (!product.Update.AllowOnlineUpdate)
        {
            throw new InvalidOperationException(
                $"No offline package was found in '{context.PayloadDirectory}'. " +
                "This runtime build only contains the installer shell. " +
                "Use RS.SetupApp.Builder to generate a bundled installer, or place package.manifest.json and the zip package into the payload directory.");
        }

        if (IsPlaceholderUpdateManifestUrl(product.Update.ManifestUrl))
        {
            throw new InvalidOperationException(
                "The current product.json still uses the template online update address. " +
                "Replace update.manifestUrl with your real update feed, or generate a bundled installer that includes an offline payload.");
        }

        context.RequiresOnlinePackage = true;
        context.UpdateManifestPath = Path.Combine(context.WorkingDirectory ?? throw new InvalidOperationException("Working directory is required."), SetupRuntimeDefaults.UpdateManifestFileName);

        string manifestSource = context.Options.UpdateManifestPath
            ?? product.Update.ManifestUrl
            ?? Path.Combine(context.PayloadDirectory, SetupRuntimeDefaults.UpdateManifestFileName);

        string signaturePath = Path.Combine(context.WorkingDirectory, SetupRuntimeDefaults.UpdateManifestSignatureFileName);
        await SetupPipelineHelper.DownloadOrCopyAsync(context, manifestSource, context.UpdateManifestPath, cancellationToken).ConfigureAwait(false);
        await SetupPipelineHelper.DownloadOrCopyAsync(
            context,
            SetupPipelineHelper.GetAdjacentSignatureSource(manifestSource),
            signaturePath,
            cancellationToken).ConfigureAwait(false);
        SetupPipelineHelper.VerifyOnlineSignature(context, context.UpdateManifestPath, signaturePath);
        context.UpdateFeed = context.Services.Serializer.Load<UpdateFeedManifest>(context.UpdateManifestPath);
    }

    private static bool IsPlaceholderUpdateManifestUrl(string? manifestUrl)
    {
        if (string.IsNullOrWhiteSpace(manifestUrl))
        {
            return false;
        }

        return Uri.TryCreate(manifestUrl, UriKind.Absolute, out Uri? uri)
            && string.Equals(uri.Host, "example.com", StringComparison.OrdinalIgnoreCase);
    }
}
