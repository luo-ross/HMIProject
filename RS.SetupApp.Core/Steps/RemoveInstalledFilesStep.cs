namespace RS.SetupApp.Core;

public sealed class RemoveInstalledFilesStep : ISetupStep
{
    public string Name => "Remove installed files";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        UninstallTarget target = plan.FileSystemTargets.Single(item => item.Purpose == SetupPathPurpose.InstallRoot);
        await SetupPipelineHelper.QuarantineDirectoryAsync(context, target, "install", cancellationToken).ConfigureAwait(false);
    }
}
