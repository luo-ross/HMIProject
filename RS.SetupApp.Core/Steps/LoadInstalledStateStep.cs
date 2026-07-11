namespace RS.SetupApp.Core;

public sealed class LoadInstalledStateStep : ISetupStep
{
    public string Name => "Load installed state";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        context.ExistingState = InstalledStateLocator.TryLoad(
            product,
            context.Options.Scope,
            context.Services.Paths,
            context.Services.Serializer,
            context.Services.FileSystem,
            out string? loadedManifestPath,
            out InstallScope? loadedScope);
        context.LoadedStateManifestPath = loadedManifestPath;
        context.LoadedStateScope = loadedScope;

        return Task.CompletedTask;
    }
}
