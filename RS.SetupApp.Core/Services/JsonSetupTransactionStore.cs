using System.Text.Json;
using System.Text.Json.Serialization;

namespace RS.SetupApp.Core;

public sealed class JsonSetupTransactionStore : ISetupTransactionStore
{
    private const string JournalFileName = "transaction.json";
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
        Converters = { new JsonStringEnumConverter() }
    };

    private readonly IFileSystem _fileSystem;
    private readonly IManifestSerializer _serializer;
    private readonly ISystemPaths _paths;

    public JsonSetupTransactionStore(
        IFileSystem fileSystem,
        IManifestSerializer serializer,
        ISystemPaths paths)
    {
        _fileSystem = fileSystem;
        _serializer = serializer;
        _paths = paths;
    }

    public Task SaveAsync(SetupTransactionJournal journal, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(journal);
        token.ThrowIfCancellationRequested();

        journal.UpdatedAtUtc = DateTimeOffset.UtcNow;
        _fileSystem.WriteAllTextAtomic(GetJournalPath(journal), _serializer.Serialize(journal));
        return Task.CompletedTask;
    }

    public Task<IReadOnlyList<SetupTransactionJournal>> LoadIncompleteAsync(
        string productId,
        InstallScope scope,
        CancellationToken token)
    {
        return LoadAsync(productId, scope, terminal: false, token);
    }

    public Task<IReadOnlyList<SetupTransactionJournal>> LoadTerminalAsync(
        string productId,
        InstallScope scope,
        CancellationToken token)
    {
        return LoadAsync(productId, scope, terminal: true, token);
    }

    private Task<IReadOnlyList<SetupTransactionJournal>> LoadAsync(
        string productId,
        InstallScope scope,
        bool terminal,
        CancellationToken token)
    {
        token.ThrowIfCancellationRequested();
        string root = _paths.GetRecoveryRoot(productId, scope);
        if (!_fileSystem.DirectoryExists(root))
        {
            return Task.FromResult<IReadOnlyList<SetupTransactionJournal>>(Array.Empty<SetupTransactionJournal>());
        }

        List<SetupTransactionJournal> journals = [];
        foreach (string directory in _fileSystem.EnumerateDirectories(root, "*", SearchOption.TopDirectoryOnly))
        {
            token.ThrowIfCancellationRequested();
            string journalPath = Path.Combine(directory, JournalFileName);
            if (!_fileSystem.FileExists(journalPath))
            {
                continue;
            }

            SetupTransactionJournal? journal = JsonSerializer.Deserialize<SetupTransactionJournal>(
                _fileSystem.ReadAllText(journalPath),
                JsonOptions);
            if (journal == null ||
                !string.Equals(journal.ProductId, productId, StringComparison.OrdinalIgnoreCase) ||
                journal.Scope != scope ||
                IsTerminal(journal.Phase) != terminal)
            {
                continue;
            }

            journals.Add(journal);
        }

        return Task.FromResult<IReadOnlyList<SetupTransactionJournal>>(journals);
    }

    public Task DeleteAsync(SetupTransactionJournal journal, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(journal);
        token.ThrowIfCancellationRequested();
        if (!IsTerminal(journal.Phase))
        {
            throw new InvalidOperationException("A nonterminal transaction journal cannot be deleted.");
        }

        if (_fileSystem.DirectoryExists(journal.RecoveryDirectory))
        {
            _fileSystem.DeleteDirectory(journal.RecoveryDirectory, recursive: true);
        }

        return Task.CompletedTask;
    }

    public static string GetJournalPath(SetupTransactionJournal journal)
    {
        return Path.Combine(journal.RecoveryDirectory, JournalFileName);
    }

    private static bool IsTerminal(SetupTransactionPhase phase)
    {
        return phase is SetupTransactionPhase.Committed or SetupTransactionPhase.RolledBack;
    }
}
