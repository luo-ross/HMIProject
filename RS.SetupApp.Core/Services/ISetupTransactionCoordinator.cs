namespace RS.SetupApp.Core;

public interface ISetupTransactionCoordinator
{
    Task<Guid> RegisterBeforeMutationAsync(SetupCompensationRecord record, CancellationToken token);

    Task MarkAppliedAsync(Guid recordId, CancellationToken token);

    Task<IReadOnlyList<string>> RollbackAsync(SetupTransactionJournal journal, CancellationToken recoveryToken);
}
