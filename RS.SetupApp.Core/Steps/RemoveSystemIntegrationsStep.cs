namespace RS.SetupApp.Core;

public sealed class RemoveSystemIntegrationsStep : ISetupStep
{
    public string Name => "Remove system integrations";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        if (context.TransactionCoordinator == null)
        {
            context.Services.Shortcuts.RemoveShortcuts(plan.Shortcuts);
            context.Services.Registry.RemoveInstallerEntries(product, plan, removeFileAssociations: true, removeAutorun: true);
            return;
        }

        string shortcutSnapshot = context.Services.Shortcuts.CaptureSnapshot(plan.Shortcuts);
        Guid shortcutRecord = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreShortcut,
            Target = product.ProductId,
            Backup = shortcutSnapshot
        }, cancellationToken).ConfigureAwait(false);
        context.Services.Shortcuts.RemoveShortcuts(plan.Shortcuts);
        await context.TransactionCoordinator.MarkAppliedAsync(shortcutRecord, cancellationToken).ConfigureAwait(false);

        string registrySnapshot = context.Services.Registry.CaptureInstallerEntriesSnapshot(product, plan);
        Guid registryRecord = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreRegistryValue,
            Target = product.ProductId,
            Backup = registrySnapshot
        }, cancellationToken).ConfigureAwait(false);
        context.Services.Registry.RemoveInstallerEntries(product, plan, removeFileAssociations: true, removeAutorun: true);
        await context.TransactionCoordinator.MarkAppliedAsync(registryRecord, cancellationToken).ConfigureAwait(false);
    }
}
