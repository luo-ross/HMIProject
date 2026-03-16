namespace RS.SetupApp.Core;

public sealed class DeployApplicationFilesStep : ISetupStep, IRollbackStep
{
    public string Name => "Deploy application files";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string extractionDirectory = context.ExtractionDirectory ?? throw new InvalidOperationException("Extraction directory has not been prepared.");
        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");

        if (context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(installDirectory, recursive: true);
        }

        context.Services.FileSystem.CopyDirectory(extractionDirectory, installDirectory, overwrite: true);
        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");
        if (context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(installDirectory, recursive: true);
        }

        if (!string.IsNullOrWhiteSpace(context.BackupDirectory) && context.Services.FileSystem.DirectoryExists(context.BackupDirectory))
        {
            context.Services.FileSystem.MoveDirectory(context.BackupDirectory, installDirectory);
        }

        return Task.CompletedTask;
    }
}
