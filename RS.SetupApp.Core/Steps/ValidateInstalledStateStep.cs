namespace RS.SetupApp.Core;

public sealed class ValidateInstalledStateStep : ISetupStep
{
    public string Name => "Validate installed state";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest? state = context.ExistingState;
        if (state == null)
        {
            return;
        }

        ProductManifest product = context.Product
            ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        string declaredStatePath = context.Services.Paths.GetStateManifestPath(
            product.ProductId,
            state.InstallScope);
        if (context.LoadedStateScope != state.InstallScope ||
            !PathsEqual(context.LoadedStateManifestPath, declaredStatePath))
        {
            throw new InvalidOperationException(
                "Installed-state validation failed (state-source-mismatch): the declared state scope does not match the scope and path from which it was loaded.");
        }

        if (context.Options.ClaimLegacyInstallation)
        {
            LegacyInstallationClaimResult claim = await context.Services.LegacyInstallationClaimService
                .ClaimAsync(product, state, cancellationToken)
                .ConfigureAwait(false);
            if (!claim.Succeeded)
            {
                throw new InvalidOperationException(
                    $"Legacy installation claim failed ({claim.FailureCode}): {claim.Message}");
            }
        }

        InstalledStateValidationResult validation = context.Services.InstalledStateValidator.Validate(
            product,
            state,
            context.Options);
        context.InstalledStateValidation = validation;
        if (!validation.IsValid || validation.Plan == null)
        {
            throw new InvalidOperationException(
                $"Installed-state validation failed ({validation.FailureCode}): {validation.Message}");
        }

        if (context.LoadedStateScope != validation.Plan.InstallScope ||
            !PathsEqual(context.LoadedStateManifestPath, validation.Plan.StateManifestPath))
        {
            throw new InvalidOperationException(
                "Installed-state validation failed (state-source-mismatch): the state contents do not match the scope and path from which they were loaded.");
        }

        context.UninstallPlan = validation.Plan;
    }

    private static bool PathsEqual(string? left, string? right)
    {
        if (string.IsNullOrWhiteSpace(left) || string.IsNullOrWhiteSpace(right))
        {
            return false;
        }

        try
        {
            return string.Equals(
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(left)),
                Path.TrimEndingDirectorySeparator(Path.GetFullPath(right)),
                StringComparison.OrdinalIgnoreCase);
        }
        catch (Exception exception) when (exception is ArgumentException or IOException or NotSupportedException or System.Security.SecurityException)
        {
            return false;
        }
    }
}
