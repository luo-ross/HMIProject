namespace RS.SetupApp.Core;

public sealed class RemoveInstalledFilesStep : ISetupStep
{
    public string Name => "Remove installed files";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        InstalledStateManifest state = context.ExistingState ?? throw new InvalidOperationException("Installed state has not been loaded.");
        if (context.Services.FileSystem.DirectoryExists(state.InstallDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(state.InstallDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
