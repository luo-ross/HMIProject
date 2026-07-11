using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Services;

[TestClass]
public sealed class JsonSetupTransactionStoreTests
{
    [TestMethod]
    public async Task Coordinator_ShouldPersistBeforeMutation_AndReplayAppliedRecordsInReverseOrder()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths);
        SetupTransactionCoordinator coordinator = new(journal, store, fileSystem);
        string first = Path.Combine(temp.DirectoryPath, "first.txt");
        string second = Path.Combine(temp.DirectoryPath, "second.txt");

        Guid firstId = await coordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = first
        }, CancellationToken.None);
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));
        File.WriteAllText(first, "first");
        await coordinator.MarkAppliedAsync(firstId, CancellationToken.None);

        Guid secondId = await coordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = second
        }, CancellationToken.None);
        File.WriteAllText(second, "second");
        await coordinator.MarkAppliedAsync(secondId, CancellationToken.None);

        IReadOnlyList<string> errors = await coordinator.RollbackAsync(journal, CancellationToken.None);

        Assert.AreEqual(0, errors.Count);
        Assert.IsFalse(File.Exists(first));
        Assert.IsFalse(File.Exists(second));
        Assert.IsTrue(journal.Compensations.All(record => record.Reverted));
        Assert.AreEqual(SetupTransactionPhase.RolledBack, journal.Phase);
    }

    [TestMethod]
    public async Task Coordinator_ShouldRetainJournalAndEvidence_WhenRecoveryFails()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        FaultInjectingFileSystem fileSystem = new(physical)
        {
            FailureFactory = (operation, path) => operation == nameof(IFileSystem.DeleteFile) &&
                path.EndsWith("cannot-delete.txt", StringComparison.OrdinalIgnoreCase)
                    ? new IOException("Recovery delete failed.")
                    : null
        };
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths);
        SetupTransactionCoordinator coordinator = new(journal, store, fileSystem);
        string target = Path.Combine(temp.DirectoryPath, "cannot-delete.txt");
        File.WriteAllText(target, "evidence");
        Guid id = await coordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target
        }, CancellationToken.None);
        await coordinator.MarkAppliedAsync(id, CancellationToken.None);

        IReadOnlyList<string> errors = await coordinator.RollbackAsync(journal, CancellationToken.None);

        Assert.AreEqual(1, errors.Count);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, journal.Phase);
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));
        Assert.IsTrue(File.Exists(target));
    }

    [TestMethod]
    public async Task Recovery_ShouldRetainPersistedUnappliedCrossVolumeMoveEvidence_WithoutDeletingEitherDirectory()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths);
        SetupTransactionCoordinator coordinator = new(journal, store, fileSystem);
        string source = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "install", "demo-app")).FullName;
        string sourceSentinel = Path.Combine(source, "source-sentinel.txt");
        File.WriteAllText(sourceSentinel, "source evidence");
        string backup = Directory.CreateDirectory(Path.Combine(temp.DirectoryPath, "recovery-evidence")).FullName;
        string backupSentinel = Path.Combine(backup, "external-sentinel.txt");
        File.WriteAllText(backupSentinel, "external evidence");

        SetupCompensationRecord record = new()
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.RestoreDirectory,
            Target = source,
            Backup = backup
        };
        record.Metadata[SetupTransactionCoordinator.RetainEvidenceUntilAppliedKey] = "true";
        await coordinator.RegisterBeforeMutationAsync(record, CancellationToken.None);

        // Reload the journal as a new process would after crashing between promotion and MarkApplied.
        SetupTransactionJournal recoveredJournal = (await store.LoadIncompleteAsync(
            journal.ProductId,
            journal.Scope,
            CancellationToken.None)).Single();
        SetupRecoveryResult recovery = await new SetupRecoveryCoordinator(
            store,
            fileSystem,
            new FakeRegistryService(),
            new FakeShortcutService()).RecoverAsync(recoveredJournal, CancellationToken.None);

        Assert.IsFalse(recovery.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, recoveredJournal.Phase);
        Assert.AreEqual("source evidence", File.ReadAllText(sourceSentinel));
        Assert.AreEqual("external evidence", File.ReadAllText(backupSentinel));
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(recoveredJournal)));
    }

    [TestMethod]
    public async Task Coordinator_ShouldTreatMissingShortcutDeletionAsAnIdempotentSuccess()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths);
        SetupTransactionCoordinator coordinator = new(
            journal,
            store,
            fileSystem,
            new FakeRegistryService(),
            new FakeShortcutService());
        Guid id = await coordinator.RegisterBeforeMutationAsync(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteShortcut,
            Target = Path.Combine(temp.DirectoryPath, "missing.lnk")
        }, CancellationToken.None);
        await coordinator.MarkAppliedAsync(id, CancellationToken.None);

        IReadOnlyList<string> errors = await coordinator.RollbackAsync(journal, CancellationToken.None);

        Assert.AreEqual(0, errors.Count);
        Assert.AreEqual(SetupTransactionPhase.RolledBack, journal.Phase);
    }

    [TestMethod]
    public async Task SaveAsync_ShouldUseAtomicWrite_AndRetainPreviousReadableJournal_WhenSaveFails()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        FaultInjectingFileSystem fileSystem = new(physical);
        JsonManifestSerializer serializer = new();
        JsonSetupTransactionStore store = new(fileSystem, serializer, paths);
        SetupTransactionJournal journal = CreateJournal(paths);

        await store.SaveAsync(journal, CancellationToken.None);
        journal.Phase = SetupTransactionPhase.Applying;
        fileSystem.FailureFactory = (operation, _) => operation == nameof(IFileSystem.WriteAllTextAtomic)
            ? new IOException("Interrupted atomic journal save.")
            : null;

        await Assert.ThrowsExceptionAsync<IOException>(() => store.SaveAsync(journal, CancellationToken.None));

        fileSystem.FailureFactory = null;
        IReadOnlyList<SetupTransactionJournal> incomplete = await store.LoadIncompleteAsync(
            journal.ProductId,
            journal.Scope,
            CancellationToken.None);
        Assert.AreEqual(1, incomplete.Count);
        Assert.AreEqual(SetupTransactionPhase.Prepared, incomplete[0].Phase);
        Assert.IsTrue(fileSystem.Mutations.Any(item => item.Operation == nameof(IFileSystem.WriteAllTextAtomic)));
    }

    private static SetupTransactionJournal CreateJournal(TestSystemPaths paths)
    {
        Guid operationId = Guid.Parse("e6d13046-bb16-4b06-a60c-dd4b8d89397f");
        return new SetupTransactionJournal
        {
            OperationId = operationId,
            ProductId = "demo-app",
            Scope = InstallScope.CurrentUser,
            Mode = SetupMode.Install,
            InstallDirectory = Path.Combine(paths.RootPath, "install", "demo-app"),
            RecoveryDirectory = paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser),
            Phase = SetupTransactionPhase.Prepared,
            StartedAtUtc = new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero),
            UpdatedAtUtc = new DateTimeOffset(2026, 7, 11, 0, 0, 0, TimeSpan.Zero)
        };
    }
}
