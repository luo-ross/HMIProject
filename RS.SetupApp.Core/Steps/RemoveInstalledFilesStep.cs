namespace RS.SetupApp.Core;

public sealed class RemoveInstalledFilesStep : ISetupStep
{
    public string Name => "Remove installed files";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        if (context.Services.FileSystem.DirectoryExists(plan.InstallDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(plan.InstallDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
