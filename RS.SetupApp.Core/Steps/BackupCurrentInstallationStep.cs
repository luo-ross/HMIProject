namespace RS.SetupApp.Core;

public sealed class BackupCurrentInstallationStep : ISetupStep, IRollbackStep
{
    public string Name => "Backup current installation";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string installDirectory = context.ExistingState == null
            ? context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.")
            : context.UninstallPlan?.InstallDirectory
              ?? throw new InvalidOperationException("A validated uninstall plan is required for an existing installation.");
        if (!context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            return Task.CompletedTask;
        }

        foreach (string legacyBackup in context.UninstallPlan?.FileSystemTargets
                     .Where(target => target.Purpose == SetupPathPurpose.BackupRoot)
                     .Select(target => target.Path)
                 ?? Enumerable.Empty<string>())
        {
            if (context.Services.FileSystem.DirectoryExists(legacyBackup))
            {
                context.Services.FileSystem.DeleteDirectory(legacyBackup, recursive: true);
            }
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
