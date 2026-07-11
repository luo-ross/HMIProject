namespace RS.SetupApp.Core;

public sealed class ResolvePackageStep : ISetupStep
{
    public string Name => "Resolve package";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");

        if (!string.IsNullOrWhiteSpace(context.PackageManifestPath))
        {
            context.Package = context.Services.Serializer.Load<PackageManifest>(context.PackageManifestPath);
        }
        else
        {
            UpdateFeedManifest updateFeed = context.UpdateFeed ?? throw new InvalidOperationException("Update manifest has not been resolved.");
            if (!string.Equals(updateFeed.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException("Update feed does not match the requested product.");
            }

            if (context.ExistingState != null && SetupPathUtility.CompareVersions(updateFeed.Version, context.ExistingState.Version) <= 0)
            {
                throw new InvalidOperationException("No newer update is available.");
            }

            string manifestSource = SetupPipelineHelper.ResolveSourceRelativeToManifest(
                context.Options.UpdateManifestPath ?? product.Update.ManifestUrl ?? context.UpdateManifestPath ?? string.Empty,
                updateFeed.PackageManifestUrl);
            string packageSource = SetupPipelineHelper.ResolveSourceRelativeToManifest(
                context.Options.UpdateManifestPath ?? product.Update.ManifestUrl ?? context.UpdateManifestPath ?? string.Empty,
                updateFeed.PackageUrl);

            context.PackageManifestPath = Path.Combine(context.WorkingDirectory ?? throw new InvalidOperationException("Working directory is required."), SetupRuntimeDefaults.PackageManifestFileName);
            string packageManifestSignaturePath = Path.Combine(context.WorkingDirectory, SetupRuntimeDefaults.PackageManifestSignatureFileName);
            string archiveName = Path.GetFileName(packageSource);
            context.PackagePath = Path.Combine(context.WorkingDirectory, string.IsNullOrWhiteSpace(archiveName) ? "package.zip" : archiveName);

            await SetupPipelineHelper.DownloadOrCopyAsync(context, manifestSource, context.PackageManifestPath, cancellationToken).ConfigureAwait(false);
            await SetupPipelineHelper.DownloadOrCopyAsync(
                context,
                SetupPipelineHelper.GetAdjacentSignatureSource(manifestSource),
                packageManifestSignaturePath,
                cancellationToken).ConfigureAwait(false);
            SetupPipelineHelper.VerifyOnlineSignature(context, context.PackageManifestPath, packageManifestSignaturePath);
            await SetupPipelineHelper.DownloadOrCopyAsync(context, packageSource, context.PackagePath, cancellationToken).ConfigureAwait(false);
            context.Package = context.Services.Serializer.Load<PackageManifest>(context.PackageManifestPath);
        }

        if (context.Package == null)
        {
            throw new InvalidOperationException("No package manifest was resolved.");
        }

        if (!string.Equals(context.Package.ProductId, product.ProductId, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Package productId does not match the product manifest.");
        }
    }
}
