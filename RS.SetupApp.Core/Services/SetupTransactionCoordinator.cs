namespace RS.SetupApp.Core;

public sealed class SetupTransactionCoordinator : ISetupTransactionCoordinator
{
    internal const string RetainEvidenceUntilAppliedKey = "retainEvidenceUntilApplied";

    private readonly SetupTransactionJournal _journal;
    private readonly ISetupTransactionStore _store;
    private readonly IFileSystem _fileSystem;
    private readonly IRegistryService? _registry;
    private readonly IShortcutService? _shortcuts;

    public SetupTransactionCoordinator(
        SetupTransactionJournal journal,
        ISetupTransactionStore store,
        IFileSystem fileSystem,
        IRegistryService? registry = null,
        IShortcutService? shortcuts = null)
    {
        _journal = journal;
        _store = store;
        _fileSystem = fileSystem;
        _registry = registry;
        _shortcuts = shortcuts;
    }

    public async Task<Guid> RegisterBeforeMutationAsync(SetupCompensationRecord record, CancellationToken token)
    {
        ArgumentNullException.ThrowIfNull(record);
        if (record.Id == Guid.Empty)
        {
            throw new ArgumentException("A compensation record requires an identifier.", nameof(record));
        }

        if (_journal.Compensations.Any(item => item.Id == record.Id))
        {
            throw new InvalidOperationException("The compensation record has already been registered.");
        }

        _journal.Compensations.Add(record);
        await _store.SaveAsync(_journal, token).ConfigureAwait(false);
        return record.Id;
    }

    public async Task MarkAppliedAsync(Guid recordId, CancellationToken token)
    {
        SetupCompensationRecord record = _journal.Compensations.SingleOrDefault(item => item.Id == recordId)
            ?? throw new InvalidOperationException("The compensation record was not registered.");
        record.Applied = true;
        await _store.SaveAsync(_journal, token).ConfigureAwait(false);
    }

    public async Task<IReadOnlyList<string>> RollbackAsync(
        SetupTransactionJournal journal,
        CancellationToken recoveryToken)
    {
        ArgumentNullException.ThrowIfNull(journal);
        List<string> errors = [];
        journal.Phase = SetupTransactionPhase.RollingBack;
        await TrySaveAsync(journal, recoveryToken, errors).ConfigureAwait(false);

        foreach (SetupCompensationRecord record in journal.Compensations.AsEnumerable().Reverse())
        {
            if (record.Reverted)
            {
                continue;
            }

            if (ShouldRetainEvidenceUntilApplied(record))
            {
                errors.Add($"{record.Kind} '{record.Target}': unproven cross-volume move evidence was retained.");
                continue;
            }

            try
            {
                recoveryToken.ThrowIfCancellationRequested();
                Compensate(record);
                record.Reverted = true;
                await TrySaveAsync(journal, recoveryToken, errors).ConfigureAwait(false);
            }
            catch (Exception exception)
            {
                errors.Add($"{record.Kind} '{record.Target}': {exception.Message}");
            }
        }

        if (errors.Count == 0)
        {
            journal.Phase = SetupTransactionPhase.RolledBack;
            try
            {
                await _store.SaveAsync(journal, recoveryToken).ConfigureAwait(false);
                return errors;
            }
            catch (Exception exception)
            {
                errors.Add($"Journal save: {exception.Message}");
            }
        }

        journal.RecoveryErrors.Clear();
        journal.RecoveryErrors.AddRange(errors);
        journal.Phase = SetupTransactionPhase.RecoveryFailed;
        await TrySaveAsync(journal, recoveryToken, errors).ConfigureAwait(false);
        return errors;
    }

    private static bool ShouldRetainEvidenceUntilApplied(SetupCompensationRecord record)
    {
        return !record.Applied &&
               record.Kind == SetupCompensationKind.RestoreDirectory &&
               record.Metadata.TryGetValue(RetainEvidenceUntilAppliedKey, out string? value) &&
               string.Equals(value, "true", StringComparison.OrdinalIgnoreCase);
    }

    private void Compensate(SetupCompensationRecord record)
    {
        switch (record.Kind)
        {
            case SetupCompensationKind.RestoreDirectory:
                RestoreDirectory(record);
                break;
            case SetupCompensationKind.DeleteDirectory:
                if (_fileSystem.DirectoryExists(record.Target))
                {
                    _fileSystem.DeleteDirectory(record.Target, recursive: true);
                }

                break;
            case SetupCompensationKind.RestoreFile:
                RestoreFile(record);
                break;
            case SetupCompensationKind.DeleteFile:
                if (_fileSystem.FileExists(record.Target))
                {
                    _fileSystem.DeleteFile(record.Target);
                }

                break;
            case SetupCompensationKind.RestoreRegistryValue:
                (_registry ?? throw new InvalidOperationException("The registry recovery service is not configured."))
                    .RestoreInstallerEntriesSnapshot(record.Backup
                        ?? throw new InvalidOperationException("The registry snapshot is missing."));
                break;
            case SetupCompensationKind.RestoreShortcut:
                (_shortcuts ?? throw new InvalidOperationException("The shortcut recovery service is not configured."))
                    .RestoreSnapshot(record.Backup
                        ?? throw new InvalidOperationException("The shortcut snapshot is missing."));
                break;
            case SetupCompensationKind.DeleteRegistryValue:
                DeleteRegistryValue(record);
                break;
            case SetupCompensationKind.DeleteShortcut:
                (_shortcuts ?? throw new InvalidOperationException("The shortcut recovery service is not configured."))
                    .DeleteShortcut(record.Target);
                break;
            default:
                throw new NotSupportedException($"Compensation kind '{record.Kind}' is not configured.");
        }
    }

    private void RestoreDirectory(SetupCompensationRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Backup) || !_fileSystem.DirectoryExists(record.Backup))
        {
            return;
        }

        if (_fileSystem.DirectoryExists(record.Target))
        {
            _fileSystem.DeleteDirectory(record.Target, recursive: true);
        }

        _fileSystem.MoveDirectory(record.Backup, record.Target);
    }

    private void RestoreFile(SetupCompensationRecord record)
    {
        if (string.IsNullOrWhiteSpace(record.Backup) || !_fileSystem.FileExists(record.Backup))
        {
            return;
        }

        if (_fileSystem.FileExists(record.Target))
        {
            _fileSystem.DeleteFile(record.Target);
        }

        _fileSystem.MoveFile(record.Backup, record.Target, overwrite: false);
    }

    private void DeleteRegistryValue(SetupCompensationRecord record)
    {
        if (!record.Metadata.TryGetValue("keyPath", out string? keyPath) ||
            !record.Metadata.TryGetValue("valueName", out string? valueName))
        {
            throw new InvalidOperationException("The registry value compensation metadata is incomplete.");
        }

        InstallScope scope = _journal.Scope;
        if (record.Metadata.TryGetValue("scope", out string? scopeValue) &&
            !Enum.TryParse(scopeValue, ignoreCase: true, out scope))
        {
            throw new InvalidOperationException("The registry value compensation scope is invalid.");
        }

        (_registry ?? throw new InvalidOperationException("The registry recovery service is not configured."))
            .DeleteValue(scope, keyPath, valueName);
    }

    private async Task TrySaveAsync(
        SetupTransactionJournal journal,
        CancellationToken recoveryToken,
        ICollection<string> errors)
    {
        try
        {
            await _store.SaveAsync(journal, recoveryToken).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            errors.Add($"Journal save: {exception.Message}");
        }
    }
}
