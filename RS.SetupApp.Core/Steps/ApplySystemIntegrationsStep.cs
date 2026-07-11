namespace RS.SetupApp.Core;

public sealed class ApplySystemIntegrationsStep : ISetupStep, IRollbackStep
{
    public string Name => "Apply system integrations";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.TransactionCoordinator == null)
        {
            SetupPipelineHelper.ApplySystemIntegrations(context);
            return;
        }

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        PackageManifest package = context.Package ?? throw new InvalidOperationException("Package manifest has not been loaded.");
        InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");

        foreach (string directory in state.DataDirectories.Values)
        {
            if (context.Services.FileSystem.DirectoryExists(directory))
            {
                continue;
            }

            Guid directoryRecord = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
            {
                Id = Guid.NewGuid(),
                Kind = SetupCompensationKind.DeleteDirectory,
                Target = directory
            }, cancellationToken).ConfigureAwait(false);
            context.Services.FileSystem.CreateDirectory(directory);
            await context.TransactionCoordinator.MarkAppliedAsync(directoryRecord, cancellationToken).ConfigureAwait(false);
        }

        IReadOnlyList<RegisteredShortcutState> shortcutTargets = GetShortcutTargets(context, product, state);
        string shortcutSnapshot = context.Services.Shortcuts.CaptureSnapshot(shortcutTargets);
        Guid shortcutRecord = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreShortcut,
            Target = product.ProductId,
            Backup = shortcutSnapshot
        }, cancellationToken).ConfigureAwait(false);
        state.Shortcuts = context.Services.Shortcuts
            .CreateShortcuts(product, state, enabled: !context.Options.NoShortcuts)
            .ToList();
        await context.TransactionCoordinator.MarkAppliedAsync(shortcutRecord, cancellationToken).ConfigureAwait(false);

        string registrySnapshot = context.Services.Registry.CaptureInstallerEntriesSnapshot(product, state);
        Guid registryRecord = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreRegistryValue,
            Target = product.ProductId,
            Backup = registrySnapshot
        }, cancellationToken).ConfigureAwait(false);
        context.Services.Registry.RegisterInstallerEntries(product, package, state);
        await context.TransactionCoordinator.MarkAppliedAsync(registryRecord, cancellationToken).ConfigureAwait(false);
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.TransactionCoordinator != null)
        {
            return Task.CompletedTask;
        }

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

    private static IReadOnlyList<RegisteredShortcutState> GetShortcutTargets(
        SetupExecutionContext context,
        ProductManifest product,
        InstalledStateManifest state)
    {
        return product.Shortcuts
            .Where(item => item.EnabledByDefault)
            .Select(item => new RegisteredShortcutState
            {
                Name = string.IsNullOrWhiteSpace(item.Name) ? product.DisplayName : item.Name,
                Path = context.Services.Paths.GetShortcutPath(product, item, state.InstallScope),
                Location = item.Location
            })
            .ToArray();
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
