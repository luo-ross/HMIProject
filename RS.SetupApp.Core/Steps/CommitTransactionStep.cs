namespace RS.SetupApp.Core;

public sealed class CommitTransactionStep : ISetupStep
{
    public string Name => "Commit transaction";

    public async Task ExecuteAsync(SetupExecutionContext context, CancellationToken cancellationToken)
    {
        if (context.Journal == null)
        {
            return;
        }

        cancellationToken.ThrowIfCancellationRequested();
        context.Journal.Phase = SetupTransactionPhase.Committing;
        await context.Services.TransactionStore.SaveAsync(context.Journal, cancellationToken).ConfigureAwait(false);
        context.Journal.Phase = SetupTransactionPhase.Committed;
        await context.Services.TransactionStore.SaveAsync(context.Journal, cancellationToken).ConfigureAwait(false);
    }
}
