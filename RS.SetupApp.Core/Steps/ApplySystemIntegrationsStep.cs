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
        UninstallPlan rollbackPlan = new(
            state.InstallDirectory,
            state.StateManifestPath,
            Array.Empty<UninstallTarget>(),
            Array.AsReadOnly(state.Shortcuts.Select(CloneShortcut).ToArray()))
        {
            ProductId = state.ProductId,
            InstallationId = state.InstallationId,
            InstallScope = state.InstallScope,
            MainExecutablePath = state.MainExecutablePath,
            MaintenanceDirectory = state.MaintenanceDirectory,
            AutorunEntryName = state.AutorunEntryName,
            FileAssociations = Array.AsReadOnly(state.FileAssociations.Select(CloneAssociation).ToArray())
        };
        context.Services.Shortcuts.RemoveShortcuts(rollbackPlan.Shortcuts);
        context.Services.Registry.RemoveInstallerEntries(product, rollbackPlan, removeFileAssociations: true, removeAutorun: true);
        return Task.CompletedTask;
    }

    private static RegisteredShortcutState CloneShortcut(RegisteredShortcutState shortcut)
    {
        return new RegisteredShortcutState
        {
            Name = shortcut.Name,
            Path = shortcut.Path,
            Location = shortcut.Location
        };
    }

    private static RegisteredFileAssociationState CloneAssociation(RegisteredFileAssociationState association)
    {
        return new RegisteredFileAssociationState
        {
            Extension = association.Extension,
            ProgId = association.ProgId,
            Command = association.Command,
            CommandRegistryPath = association.CommandRegistryPath
        };
    }
}
