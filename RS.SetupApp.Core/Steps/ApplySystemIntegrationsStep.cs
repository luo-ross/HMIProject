namespace RS.SetupApp.Core;

public sealed class ApplySystemIntegrationsStep : ISetupStep, IRollbackStep
{
    public string Name => "Apply system integrations";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SetupPipelineHelper.ApplySystemIntegrations(context);
        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
        context.Services.Shortcuts.RemoveShortcuts(state.Shortcuts);
        context.Services.Registry.RemoveInstallerEntries(product, state, removeFileAssociations: true, removeAutorun: true);
        return Task.CompletedTask;
    }
}
