namespace RS.SetupApp.Core;

public sealed class RemoveInstalledStateStep : ISetupStep
{
    public string Name => "Remove installed state";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest state = context.ExistingState ?? throw new InvalidOperationException("Installed state has not been loaded.");
        context.Services.FileSystem.DeleteFile(state.StateManifestPath);

        string? stateDirectory = Path.GetDirectoryName(state.StateManifestPath);
        if (!string.IsNullOrWhiteSpace(stateDirectory) && context.Services.FileSystem.DirectoryExists(stateDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(stateDirectory, recursive: true);
        }

        if (!string.IsNullOrWhiteSpace(state.PendingBackupDirectory) && context.Services.FileSystem.DirectoryExists(state.PendingBackupDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(state.PendingBackupDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
