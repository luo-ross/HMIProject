namespace RS.SetupApp.Core;

public sealed class DeployMaintenanceBundleStep : ISetupStep, IRollbackStep
{
    public string Name => "Deploy maintenance runtime";

    public Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        SetupPipelineHelper.DeployMaintenanceBundle(context);
        return Task.CompletedTask;
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.ResultState != null && context.Services.FileSystem.DirectoryExists(context.ResultState.MaintenanceDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.ResultState.MaintenanceDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
