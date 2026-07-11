namespace RS.SetupApp.Core;

public sealed class RemoveSystemIntegrationsStep : ISetupStep
{
    public string Name => "Remove system integrations";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");

        context.Services.Shortcuts.RemoveShortcuts(plan.Shortcuts);
        context.Services.Registry.RemoveInstallerEntries(product, plan, removeFileAssociations: true, removeAutorun: true);
        return Task.CompletedTask;
    }
}
