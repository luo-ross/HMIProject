namespace RS.SetupApp.Core;

public sealed class RemoveInstalledStateStep : ISetupStep
{
    public string Name => "Remove installed state";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        context.Services.FileSystem.DeleteFile(plan.StateManifestPath);

        string? stateDirectory = Path.GetDirectoryName(plan.StateManifestPath);
        if (!string.IsNullOrWhiteSpace(stateDirectory) && context.Services.FileSystem.DirectoryExists(stateDirectory))
        {
            bool isEmpty = !context.Services.FileSystem.EnumerateFiles(
                    stateDirectory,
                    "*",
                    SearchOption.TopDirectoryOnly).Any() &&
                !context.Services.FileSystem.EnumerateDirectories(
                    stateDirectory,
                    "*",
                    SearchOption.TopDirectoryOnly).Any();
            if (isEmpty)
            {
                context.Services.FileSystem.DeleteDirectory(stateDirectory, recursive: false);
            }
        }

        foreach (string backupDirectory in plan.FileSystemTargets
                     .Where(target => target.Purpose == SetupPathPurpose.BackupRoot)
                     .Select(target => target.Path)
                     .Where(context.Services.FileSystem.DirectoryExists))
        {
            context.Services.FileSystem.DeleteDirectory(backupDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
