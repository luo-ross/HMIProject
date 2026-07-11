namespace RS.SetupApp.Core;

public sealed class BackupCurrentInstallationStep : ISetupStep, IRollbackStep
{
    private bool _backupMoveCompleted;

    public string Name => "Backup current installation";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string installDirectory = context.ExistingState == null
            ? context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.")
            : context.UninstallPlan?.InstallDirectory
              ?? throw new InvalidOperationException("A validated uninstall plan is required for an existing installation.");
        if (!context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            return;
        }

        string recoveryDirectory = context.RecoveryDirectory
            ?? throw new InvalidOperationException("The persistent recovery directory has not been initialized.");
        context.BackupDirectory = Path.Combine(recoveryDirectory, "backup", "installation");
        context.Services.FileSystem.CreateDirectory(Path.GetDirectoryName(context.BackupDirectory)
            ?? throw new InvalidOperationException("The backup directory is invalid."));

        if (context.TransactionCoordinator != null)
        {
            SetupCompensationRecord record = new()
            {
                Id = Guid.NewGuid(),
                Kind = SetupCompensationKind.RestoreDirectory,
                Target = installDirectory,
                Backup = context.BackupDirectory
            };
            context.Journal!.Phase = SetupTransactionPhase.SnapshotCreated;
            Guid recordId = await context.TransactionCoordinator
                .RegisterBeforeMutationAsync(record, cancellationToken)
                .ConfigureAwait(false);
            try
            {
                context.Services.FileSystem.MoveDirectory(installDirectory, context.BackupDirectory);
                _backupMoveCompleted = true;
            }
            catch
            {
                record.Metadata[SetupTransactionCoordinator.RetainUnprovenMoveEvidenceKey] = "true";
                await context.Services.TransactionStore.SaveAsync(context.Journal, CancellationToken.None).ConfigureAwait(false);
                throw;
            }

            await context.TransactionCoordinator.MarkAppliedAsync(recordId, cancellationToken).ConfigureAwait(false);
        }
        else
        {
            context.Services.FileSystem.MoveDirectory(installDirectory, context.BackupDirectory);
            _backupMoveCompleted = true;
        }

        if (context.ResultState != null)
        {
            context.ResultState.PendingBackupDirectory = context.BackupDirectory;
            context.ResultState.LastBackupDirectory = context.BackupDirectory;
        }
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TransactionCoordinator != null || !_backupMoveCompleted)
        {
            return Task.CompletedTask;
        }

        if (string.IsNullOrWhiteSpace(context.BackupDirectory) || !context.Services.FileSystem.DirectoryExists(context.BackupDirectory))
        {
            return Task.CompletedTask;
        }

        string installDirectory = context.UninstallPlan?.InstallDirectory
            ?? context.InstallDirectory
            ?? throw new InvalidOperationException("Install directory has not been resolved.");
        if (context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(installDirectory, recursive: true);
        }

        context.Services.FileSystem.MoveDirectory(context.BackupDirectory, installDirectory);
        return Task.CompletedTask;
    }
}
