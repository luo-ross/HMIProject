namespace RS.SetupApp.Core;

public sealed class RemoveSystemIntegrationsStep : ISetupStep
{
    public string Name => "Remove system integrations";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstalledStateManifest state = context.ExistingState ?? throw new InvalidOperationException("Installed state has not been loaded.");

        context.Services.Shortcuts.RemoveShortcuts(state.Shortcuts);
        context.Services.Registry.RemoveInstallerEntries(product, state, removeFileAssociations: true, removeAutorun: true);
        return Task.CompletedTask;
    }
}
