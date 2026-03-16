namespace RS.SetupApp.Core;

public sealed class ValidatePackageArchiveStep : ISetupStep
{
    public string Name => "Validate package archive";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        if (string.IsNullOrWhiteSpace(context.PackagePath))
        {
            throw new InvalidOperationException("Package path has not been resolved.");
        }

        string archiveHash = context.Services.Hasher.ComputeSha256(context.PackagePath);
        if (!string.Equals(archiveHash, package.ArchiveSha256, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Package archive hash mismatch.");
        }

        if (context.UpdateFeed != null &&
            !string.Equals(context.UpdateFeed.PackageSha256, archiveHash, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Update feed hash does not match the downloaded archive.");
        }

        return Task.CompletedTask;
    }
}
