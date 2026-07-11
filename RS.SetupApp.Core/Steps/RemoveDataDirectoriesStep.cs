namespace RS.SetupApp.Core;

public sealed class RemoveDataDirectoriesStep : ISetupStep
{
    public string Name => "Remove product data";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        UninstallPlan plan = context.UninstallPlan ?? throw new InvalidOperationException("A validated uninstall plan is required.");
        int index = 0;
        foreach (UninstallTarget target in plan.FileSystemTargets
                     .Where(target => target.Purpose == SetupPathPurpose.DataRoot))
        {
            await SetupPipelineHelper.QuarantineDirectoryAsync(
                context,
                target,
                $"data-{index++}",
                cancellationToken).ConfigureAwait(false);
        }
    }
}
