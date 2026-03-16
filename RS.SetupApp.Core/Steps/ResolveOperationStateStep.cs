namespace RS.SetupApp.Core;

public sealed class ResolveOperationStateStep : ISetupStep
{
    public string Name => "Resolve operation state";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");

        if (context.ExistingState != null && context.Options.Scope.HasValue && context.Options.Scope.Value != context.ExistingState.InstallScope)
        {
            throw new InvalidOperationException("Cross-scope upgrades are not allowed.");
        }

        context.EffectiveScope = context.ExistingState?.InstallScope ?? context.Options.Scope ?? product.InstallDefaults.DefaultScope;
        context.ActualMode = context.Options.Mode switch
        {
            SetupMode.Repair => SetupMode.Repair,
            SetupMode.Update => SetupMode.Update,
            _ when context.ExistingState == null => SetupMode.Install,
            _ when SetupPathUtility.CompareVersions(package.Version, context.ExistingState.Version) > 0 => SetupMode.Update,
            _ => SetupMode.Repair
        };

        context.InstallDirectory = context.ExistingState?.InstallDirectory
            ?? (!string.IsNullOrWhiteSpace(context.Options.InstallDirectory)
                ? Path.GetFullPath(context.Options.InstallDirectory)
                : context.Services.Paths.GetDefaultInstallDirectory(product, context.EffectiveScope));

        context.ResultState = SetupPipelineHelper.CreateInstalledState(context);
        return Task.CompletedTask;
    }
}
