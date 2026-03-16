namespace RS.SetupApp.Core;

public sealed class BackupCurrentInstallationStep : ISetupStep, IRollbackStep
{
    public string Name => "Backup current installation";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");
        if (!context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            return Task.CompletedTask;
        }

        if (!string.IsNullOrWhiteSpace(context.ExistingState?.PendingBackupDirectory) &&
            context.Services.FileSystem.DirectoryExists(context.ExistingState.PendingBackupDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.ExistingState.PendingBackupDirectory, recursive: true);
        }

        context.BackupDirectory = Path.Combine(context.WorkingDirectory ?? throw new InvalidOperationException("Working directory is required."), "backup");
        if (context.Services.FileSystem.DirectoryExists(context.BackupDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.BackupDirectory, recursive: true);
        }

        context.Services.FileSystem.MoveDirectory(installDirectory, context.BackupDirectory);

        if (context.ResultState != null)
        {
            context.ResultState.PendingBackupDirectory = context.BackupDirectory;
            context.ResultState.LastBackupDirectory = context.BackupDirectory;
        }

        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(context.BackupDirectory) || !context.Services.FileSystem.DirectoryExists(context.BackupDirectory))
        {
            return Task.CompletedTask;
        }

        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");
        if (context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(installDirectory, recursive: true);
        }

        context.Services.FileSystem.MoveDirectory(context.BackupDirectory, installDirectory);
        return Task.CompletedTask;
    }
}
