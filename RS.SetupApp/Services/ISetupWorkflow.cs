using RS.SetupApp.Core;

namespace RS.SetupApp.Services;

public interface ISetupWorkflow
{
    Task<SetupWorkspace> LoadAsync(CancellationToken cancellationToken);

    Task<SetupOperationResult> ExecuteAsync(
        RuntimeOptions options,
        IProgress<SetupProgress>? progress,
        CancellationToken cancellationToken);

    Task<SetupOperationResult> RecoverAsync(CancellationToken cancellationToken);

    Task<UpdateFeedManifest?> CheckForUpdatesAsync(string productManifestPath, CancellationToken cancellationToken);

    Task<LegacyInstallationClaimResult> ClaimLegacyInstallationAsync(
        ProductManifest product,
        InstalledStateManifest state,
        RuntimeOptions options,
        CancellationToken cancellationToken);
}

public sealed record SetupWorkspace(
    string ProductManifestPath,
    ProductManifest Product,
    InstalledStateManifest? InstalledState,
    bool HasValidUnclaimedLegacyInstallation);
