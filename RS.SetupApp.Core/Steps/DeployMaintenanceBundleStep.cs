namespace RS.SetupApp.Core;

public sealed class DeployMaintenanceBundleStep : ISetupStep, IRollbackStep
{
    public string Name => "Deploy maintenance runtime";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        if (context.TransactionCoordinator != null)
        {
            InstalledStateManifest state = context.ResultState ?? throw new InvalidOperationException("Installed state has not been prepared.");
            Guid recordId = await context.TransactionCoordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
            {
                Id = Guid.NewGuid(),
                Kind = SetupCompensationKind.DeleteDirectory,
                Target = state.MaintenanceDirectory
            }, cancellationToken).ConfigureAwait(false);
            SetupPipelineHelper.DeployMaintenanceBundle(context);
            await context.TransactionCoordinator.MarkAppliedAsync(recordId, cancellationToken).ConfigureAwait(false);
            return;
        }

        SetupPipelineHelper.DeployMaintenanceBundle(context);
    }

    public Task RollbackAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (context.TransactionCoordinator != null)
        {
            return Task.CompletedTask;
        }

        if (context.ResultState != null && context.Services.FileSystem.DirectoryExists(context.ResultState.MaintenanceDirectory))
        {
            context.Services.FileSystem.DeleteDirectory(context.ResultState.MaintenanceDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }
}
