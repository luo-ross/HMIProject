namespace RS.SetupApp.Core;

public interface ISetupTransactionStore
{
    Task SaveAsync(SetupTransactionJournal journal, CancellationToken token);

    Task<IReadOnlyList<SetupTransactionJournal>> LoadIncompleteAsync(
        string productId,
        InstallScope scope,
        CancellationToken token);

    Task DeleteAsync(SetupTransactionJournal journal, CancellationToken token);
}
