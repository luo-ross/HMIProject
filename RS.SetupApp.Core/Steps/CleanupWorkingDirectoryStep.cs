namespace RS.SetupApp.Core;

public sealed class CleanupWorkingDirectoryStep : ISetupStep
{
    public string Name => "Cleanup working directory";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!string.IsNullOrWhiteSpace(context.WorkingDirectory) && context.Services.FileSystem.DirectoryExists(context.WorkingDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.WorkingDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
