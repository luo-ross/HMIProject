namespace RS.SetupApp.Core;

public sealed class SetupRecoveryCoordinator
{
    private readonly ISetupTransactionStore _store;
    private readonly IFileSystem _fileSystem;
    private readonly IRegistryService _registry;
    private readonly IShortcutService _shortcuts;

    public SetupRecoveryCoordinator(
        ISetupTransactionStore store,
        IFileSystem fileSystem,
        IRegistryService registry,
        IShortcutService shortcuts)
    {
        _store = store;
        _fileSystem = fileSystem;
        _registry = registry;
        _shortcuts = shortcuts;
    }

    public async Task<IReadOnlyList<SetupTransactionJournal>> FindIncompleteAsync(
        string productId,
        IReadOnlyCollection<InstallScope> scopes,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentNullException.ThrowIfNull(scopes);

        List<SetupTransactionJournal> journals = [];
        foreach (InstallScope scope in scopes.Distinct())
        {
            token.ThrowIfCancellationRequested();
            IReadOnlyList<SetupTransactionJournal> found = await _store
                .LoadIncompleteAsync(productId, scope, token)
                .ConfigureAwait(false);
            journals.AddRange(found);
        }

        return journals;
    }

    public async Task<IReadOnlyList<SetupTransactionJournal>> FindTerminalAsync(
        string productId,
        IReadOnlyCollection<InstallScope> scopes,
        CancellationToken token)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(productId);
        ArgumentNullException.ThrowIfNull(scopes);

        List<SetupTransactionJournal> journals = [];
        foreach (InstallScope scope in scopes.Distinct())
        {
            token.ThrowIfCancellationRequested();
            IReadOnlyList<SetupTransactionJournal> found = await _store
                .LoadTerminalAsync(productId, scope, token)
                .ConfigureAwait(false);
            journals.AddRange(found);
        }

        return journals;
    }

    public async Task<SetupRecoveryResult> RecoverAsync(
        SetupTransactionJournal journal,
        CancellationToken recoveryToken)
    {
        ArgumentNullException.ThrowIfNull(journal);

        List<string> errors = [];
        List<string> cleanupWarnings = [];
        if (journal.Phase is SetupTransactionPhase.Committed or SetupTransactionPhase.RolledBack)
        {
            await TryCleanupAsync(journal, recoveryToken, cleanupWarnings).ConfigureAwait(false);
            return CreateSucceededResult(journal, cleanupWarnings);
        }

        try
        {
            SetupTransactionCoordinator coordinator = new(
                journal,
                _store,
                _fileSystem,
                _registry,
                _shortcuts);
            IReadOnlyList<string> rollbackErrors = await coordinator
                .RollbackAsync(journal, recoveryToken)
                .ConfigureAwait(false);
            errors.AddRange(rollbackErrors);
            if (errors.Count == 0 && journal.Phase == SetupTransactionPhase.RolledBack)
            {
                await TryCleanupAsync(journal, recoveryToken, cleanupWarnings).ConfigureAwait(false);
                return CreateSucceededResult(journal, cleanupWarnings);
            }
        }
        catch (Exception exception)
        {
            errors.Add(exception.Message);
        }

        return new SetupRecoveryResult(false, journal, errors);
    }

    private async Task TryCleanupAsync(
        SetupTransactionJournal journal,
        CancellationToken recoveryToken,
        ICollection<string> cleanupWarnings)
    {
        try
        {
            await _store.DeleteAsync(journal, recoveryToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            cleanupWarnings.Add(exception.Message);
        }
    }

    private static SetupRecoveryResult CreateSucceededResult(
        SetupTransactionJournal journal,
        IReadOnlyList<string> cleanupWarnings)
    {
        return new SetupRecoveryResult(true, journal, Array.Empty<string>())
        {
            CleanupWarnings = cleanupWarnings.ToArray()
        };
    }
}
