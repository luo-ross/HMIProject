namespace RS.SetupApp.Core;

public sealed class RemoveInstalledStateStep : ISetupStep
{
    public string Name => "Remove installed state";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        UninstallTarget target = plan.FileSystemTargets.Single(item => item.Purpose == SetupPathPurpose.StateManifest);
        await SetupPipelineHelper.QuarantineFileAsync(context, target, "state/installed-state.json", cancellationToken)
            .ConfigureAwait(false);
    }
}
