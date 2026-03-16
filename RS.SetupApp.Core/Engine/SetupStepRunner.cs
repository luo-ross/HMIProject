namespace RS.SetupApp.Core;

public sealed class SetupStepRunner
{
    public async Task RunAsync(
        SetupExecutionContext context,
        IReadOnlyList<ISetupStep> steps,
        IProgress<SetupProgress>? progress,
        CancellationToken cancellationToken)
    {
        Stack<IRollbackStep> rollbackSteps = new();

        try
        {
            for (int index = 0; index < steps.Count; index++)
            {
                ISetupStep step = steps[index];
                progress?.Report(new SetupProgress
                {
                    CurrentStep = index + 1,
                    TotalSteps = steps.Count,
                    Message = step.Name
                });

                context.Logger?.Info(step.Name);
                await step.ExecuteAsync(context, cancellationToken).ConfigureAwait(false);
                if (step is IRollbackStep rollbackStep)
                {
                    rollbackSteps.Push(rollbackStep);
                }
            }
        }
        catch
        {
            while (rollbackSteps.Count > 0)
            {
                IRollbackStep rollbackStep = rollbackSteps.Pop();
                try
                {
                    context.Logger?.Warn($"Rollback: {rollbackStep.Name}");
                    await rollbackStep.RollbackAsync(context, cancellationToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    context.Logger?.Error($"Rollback failed: {rollbackStep.Name}", ex);
                }
            }

            throw;
        }
    }
}
