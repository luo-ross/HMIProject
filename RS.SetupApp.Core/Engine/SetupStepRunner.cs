namespace RS.SetupApp.Core;

public sealed class SetupStepRunner
{
    public async Task<SetupStepRunResult> RunAsync(
        SetupExecutionContext context,
        IReadOnlyList<ISetupStep> steps,
        IProgress<SetupProgress>? progress,
        CancellationToken operationToken,
        CancellationToken recoveryToken = default)
    {
        Stack<IRollbackStep> rollbackSteps = new();

        try
        {
            if (context.Journal != null)
            {
                context.Journal.Phase = SetupTransactionPhase.Applying;
                await context.Services.TransactionStore.SaveAsync(context.Journal, operationToken).ConfigureAwait(false);
            }

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
                if (step is IRollbackStep rollbackStep)
                {
                    rollbackSteps.Push(rollbackStep);
                }

                await step.ExecuteAsync(context, operationToken).ConfigureAwait(false);
                if (context.Journal != null)
                {
                    context.Journal.CompletedSteps.Add(step.Name);
                    await context.Services.TransactionStore.SaveAsync(context.Journal, operationToken).ConfigureAwait(false);
                }
            }

            if (context.Journal != null)
            {
                context.Journal.Phase = SetupTransactionPhase.Verifying;
                await context.Services.TransactionStore.SaveAsync(context.Journal, operationToken).ConfigureAwait(false);
            }

            return new SetupStepRunResult { Completed = true };
        }
        catch (Exception primaryError)
        {
            using CancellationTokenSource recoveryCancellation = new(TimeSpan.FromMinutes(5));
            using CancellationTokenSource recoveryLinked = CancellationTokenSource.CreateLinkedTokenSource(
                recoveryToken,
                recoveryCancellation.Token);
            List<string> recoveryErrors = [];
            while (rollbackSteps.Count > 0)
            {
                IRollbackStep rollbackStep = rollbackSteps.Pop();
                try
                {
                    context.Logger?.Warn($"Rollback: {rollbackStep.Name}");
                    await rollbackStep.RollbackAsync(context, recoveryLinked.Token).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    context.Logger?.Error($"Rollback failed: {rollbackStep.Name}", ex);
                    recoveryErrors.Add($"{rollbackStep.Name}: {ex.Message}");
                }
            }

            if (context.Journal != null && context.TransactionCoordinator != null)
            {
                context.Journal.PrimaryError = primaryError.ToString();
                IReadOnlyList<string> compensationErrors = await context.TransactionCoordinator
                    .RollbackAsync(context.Journal, recoveryLinked.Token)
                    .ConfigureAwait(false);
                recoveryErrors.AddRange(compensationErrors);
            }

            if (context.Journal != null && recoveryErrors.Count > 0)
            {
                context.Journal.RecoveryErrors.Clear();
                context.Journal.RecoveryErrors.AddRange(recoveryErrors);
                context.Journal.Phase = SetupTransactionPhase.RecoveryFailed;
                try
                {
                    await context.Services.TransactionStore
                        .SaveAsync(context.Journal, recoveryLinked.Token)
                        .ConfigureAwait(false);
                }
                catch (Exception exception)
                {
                    recoveryErrors.Add($"Journal save: {exception.Message}");
                    context.Journal.RecoveryErrors.Clear();
                    context.Journal.RecoveryErrors.AddRange(recoveryErrors);
                }
            }

            context.RecoveryErrors.AddRange(recoveryErrors);

            return new SetupStepRunResult
            {
                Completed = false,
                PrimaryError = primaryError,
                RecoveryErrors = recoveryErrors
            };
        }
    }
}
