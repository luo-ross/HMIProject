namespace RS.SetupApp.Core;

public sealed class BeginTransactionStep : ISetupStep
{
    public string Name => "Begin transaction";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.Journal != null)
        {
            return;
        }

        ProductManifest product = context.Product ?? throw new InvalidOperationException("Product manifest has not been loaded.");
        InstallScope scope = context.UninstallPlan?.InstallScope
            ?? context.ExistingState?.InstallScope
            ?? context.Options.Scope
            ?? product.InstallDefaults.DefaultScope;
        string installDirectory = context.UninstallPlan?.InstallDirectory
            ?? context.ExistingState?.InstallDirectory
            ?? (!string.IsNullOrWhiteSpace(context.Options.InstallDirectory)
                ? Path.GetFullPath(context.Options.InstallDirectory)
                : context.Services.Paths.GetDefaultInstallDirectory(product, scope));

        if (context.Options.Mode == SetupMode.Uninstall && context.ExistingState == null)
        {
            return;
        }

        Guid operationId = Guid.NewGuid();
        string recoveryDirectory = context.Services.Paths.GetRecoveryDirectory(product.ProductId, operationId, scope);
        SetupTransactionJournal journal = new()
        {
            OperationId = operationId,
            ProductId = product.ProductId,
            Scope = scope,
            Mode = context.Options.Mode,
            InstallDirectory = installDirectory,
            RecoveryDirectory = recoveryDirectory,
            Phase = SetupTransactionPhase.Prepared,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        context.OperationId = operationId;
        context.Journal = journal;
        context.RecoveryDirectory = recoveryDirectory;
        context.CanonicalDeletionTargets.Clear();
        if (context.UninstallPlan != null)
        {
            context.CanonicalDeletionTargets.AddRange(context.UninstallPlan.FileSystemTargets);
        }

        context.TransactionCoordinator = new SetupTransactionCoordinator(
            journal,
            context.Services.TransactionStore,
            context.Services.FileSystem,
            context.Services.Registry,
            context.Services.Shortcuts);
        await context.Services.TransactionStore.SaveAsync(journal, cancellationToken).ConfigureAwait(false);
    }
}
