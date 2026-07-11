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
        _ = recoveryToken;
        Stack<IRollbackStep> rollbackSteps = new();

        try
        {
            for (int index = 0; index < steps.Count; index++)
            {
                ISetupStep step = steps[index];
                await PersistPhaseBeforeStepAsync(context, step).ConfigureAwait(false);
                progress?.Report(new SetupProgress
                {
                    OperationId = context.OperationId,
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
                if (context.Journal != null && !IsTerminal(context.Journal.Phase))
                {
                    context.Journal.CompletedSteps.Add(step.Name);
                    await context.Services.TransactionStore.SaveAsync(context.Journal, CancellationToken.None).ConfigureAwait(false);
                }
            }

            if (context.Journal != null && !IsTerminal(context.Journal.Phase))
            {
                context.Journal.Phase = SetupTransactionPhase.Verifying;
                await context.Services.TransactionStore.SaveAsync(context.Journal, CancellationToken.None).ConfigureAwait(false);
            }

            return new SetupStepRunResult { Completed = true };
        }
        catch (Exception primaryError)
        {
            using CancellationTokenSource recoveryCancellation = new(TimeSpan.FromMinutes(5));
            CancellationToken independentRecoveryToken = recoveryCancellation.Token;
            List<string> recoveryErrors = [];
            bool transactionIsTerminal = context.Journal != null && IsTerminal(context.Journal.Phase);
            while (!transactionIsTerminal && rollbackSteps.Count > 0)
            {
                IRollbackStep rollbackStep = rollbackSteps.Pop();
                try
                {
                    context.Logger?.Warn($"Rollback: {rollbackStep.Name}");
                    await rollbackStep.RollbackAsync(context, independentRecoveryToken).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    context.Logger?.Error($"Rollback failed: {rollbackStep.Name}", ex);
                    recoveryErrors.Add($"{rollbackStep.Name}: {ex.Message}");
                }
            }

            if (!transactionIsTerminal && context.Journal != null && context.TransactionCoordinator != null)
            {
                context.Journal.PrimaryError = primaryError.ToString();
                IReadOnlyList<string> compensationErrors = await context.TransactionCoordinator
                    .RollbackAsync(context.Journal, independentRecoveryToken)
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
                        .SaveAsync(context.Journal, independentRecoveryToken)
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

    private static async Task PersistPhaseBeforeStepAsync(SetupExecutionContext context, ISetupStep step)
    {
        if (context.Journal == null || IsTerminal(context.Journal.Phase))
        {
            return;
        }

        SetupTransactionPhase? nextPhase = step switch
        {
            CommitTransactionStep => SetupTransactionPhase.Verifying,
            _ when context.Journal.Phase == SetupTransactionPhase.Prepared && step is not BeginTransactionStep
                => SetupTransactionPhase.Applying,
            _ => null
        };
        if (!nextPhase.HasValue)
        {
            return;
        }

        context.Journal.Phase = nextPhase.Value;
        await context.Services.TransactionStore.SaveAsync(context.Journal, CancellationToken.None).ConfigureAwait(false);
    }

    private static bool IsTerminal(SetupTransactionPhase phase)
    {
        return phase is SetupTransactionPhase.Committed or SetupTransactionPhase.RolledBack;
    }
}
