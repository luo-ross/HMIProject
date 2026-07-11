namespace RS.SetupApp.Core;

public sealed class RemoveDataDirectoriesStep : ISetupStep
{
    public string Name => "Remove product data";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        foreach (string directoryPath in plan.FileSystemTargets
                     .Where(target => target.Purpose == SetupPathPurpose.DataRoot)
                     .Select(target => target.Path)
                     .Where(context.Services.FileSystem.DirectoryExists))
        {
            context.Services.FileSystem.DeleteDirectory(directoryPath, recursive: true);
        }

        return Task.CompletedTask;
    }
}
