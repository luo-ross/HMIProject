using RS.SetupApp.Core;
using System.IO;

namespace RS.SetupApp.Services;

/// <summary>UI-facing adapter over the rollback-safe core engine.</summary>
public sealed class SetupWorkflow : ISetupWorkflow
{
    private readonly SetupServices _services;
    private readonly SetupEngine _engine;
    private RuntimeOptions? _lastOptions;

    public SetupWorkflow(SetupServices services, SetupEngine engine)
    {
        _services = services;
        _engine = engine;
    }

    public async Task<SetupWorkspace> LoadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        string productManifestPath = ResolveProductManifestPath();
        ProductManifestLoadResult result = ProductManifestLoader.Load(productManifestPath, _services.Serializer);
        if (result.Errors.Count > 0)
        {
            throw new InvalidOperationException(string.Join(Environment.NewLine, result.Errors));
        }

        ProductManifest product = result.Manifest ?? throw new InvalidOperationException("Product manifest could not be loaded.");
        InstalledStateManifest? installedState = InstalledStateLocator.TryLoad(
            product,
            requestedScope: null,
            _services.Paths,
            _services.Serializer,
            _services.FileSystem);

        bool isUnclaimedLegacyInstallation = false;
        if (installedState != null && _services.OwnershipService.Load(installedState.InstallDirectory) == null)
        {
            LegacyInstallationClaimResult validation = await _services.LegacyInstallationClaimService
                .ValidateAsync(product, installedState, new RuntimeOptions(), cancellationToken)
                .ConfigureAwait(false);
            isUnclaimedLegacyInstallation = validation.Succeeded;
        }

        return new SetupWorkspace(productManifestPath, product, installedState, isUnclaimedLegacyInstallation);
    }

    public Task<SetupOperationResult> ExecuteAsync(
        RuntimeOptions options,
        IProgress<SetupProgress>? progress,
        CancellationToken cancellationToken)
    {
        _lastOptions = options;
        return _engine.ExecuteAsync(options, progress, cancellationToken);
    }

    public Task<SetupOperationResult> RecoverAsync(CancellationToken cancellationToken)
    {
        if (_lastOptions == null)
        {
            return Task.FromResult(new SetupOperationResult
            {
                Status = SetupOperationStatus.Failed,
                FailureCode = SetupFailureCodes.RecoveryFailed,
                Message = "No operation is available to recover."
            });
        }

        // The core engine owns journal discovery, rollback and recovery; the UI never reimplements it.
        return _engine.ExecuteAsync(_lastOptions, progress: null, cancellationToken);
    }

    public Task<UpdateFeedManifest?> CheckForUpdatesAsync(string productManifestPath, CancellationToken cancellationToken)
    {
        return _engine.CheckForUpdatesAsync(productManifestPath, cancellationToken);
    }

    public Task<LegacyInstallationClaimResult> ClaimLegacyInstallationAsync(
        ProductManifest product,
        InstalledStateManifest state,
        RuntimeOptions options,
        CancellationToken cancellationToken)
    {
        return _services.LegacyInstallationClaimService.ClaimAsync(product, state, options, cancellationToken);
    }

    private string ResolveProductManifestPath()
    {
        string payloadManifest = Path.Combine(_services.Paths.GetPayloadDirectory(), SetupRuntimeDefaults.ProductManifestFileName);
        if (_services.FileSystem.FileExists(payloadManifest))
        {
            return payloadManifest;
        }

        string directManifest = Path.Combine(_services.Paths.AppBaseDirectory, SetupRuntimeDefaults.ProductManifestFileName);
        if (_services.FileSystem.FileExists(directManifest))
        {
            return directManifest;
        }

        throw new FileNotFoundException("Unable to locate product.json in the installer payload.");
    }
}
