namespace RS.SetupApp.Core;

public sealed class DeployApplicationFilesStep : ISetupStep, IRollbackStep
{
    public string Name => "Deploy application files";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        string extractionDirectory = context.ExtractionDirectory ?? throw new InvalidOperationException("Extraction directory has not been prepared.");
        string installDirectory = context.InstallDirectory ?? throw new InvalidOperationException("Install directory has not been resolved.");

        if (context.TransactionCoordinator != null)
        {
            Guid recordId = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
            {
                Id = Guid.NewGuid(),
                Kind = SetupCompensationKind.DeleteDirectory,
                Target = installDirectory
            }, cancellationToken).ConfigureAwait(false);

            if (context.Services.FileSystem.DirectoryExists(installDirectory))
            {
                context.Services.FileSystem.DeleteDirectory(installDirectory, recursive: true);
            }

            context.Services.FileSystem.CopyDirectory(extractionDirectory, installDirectory, overwrite: true);
            await context.TransactionCoordinator.MarkAppliedAsync(recordId, cancellationToken).ConfigureAwait(false);
            return;
        }

        if (context.Services.FileSystem.DirectoryExists(installDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(installDirectory, recursive: true);
        }
        context.Services.FileSystem.CopyDirectory(extractionDirectory, installDirectory, overwrite: true);
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TransactionCoordinator != null)
        {
            return Task.CompletedTask;
        }

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
