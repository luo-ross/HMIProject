using RS.SetupApp.Core;
using RS.SetupApp.Tests.Fakes;
using RS.SetupApp.Tests.Helpers;

namespace RS.SetupApp.Tests.Engine;

[TestClass]
public sealed class SetupRecoveryCoordinatorTests
{
    [DataTestMethod]
    [DataRow(SetupTransactionPhase.SnapshotCreated)]
    [DataRow(SetupTransactionPhase.Applying)]
    [DataRow(SetupTransactionPhase.Committing)]
    public async Task RecoverAsync_ShouldRestoreIncompleteJournal_AndRemoveDurableRecoveryData(SetupTransactionPhase phase)
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, phase);
        string target = Path.Combine(temp.DirectoryPath, $"mutated-{phase}.txt");
        File.WriteAllText(target, "mutated");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        SetupRecoveryResult result = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RolledBack, result.Journal.Phase);
        Assert.IsFalse(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [TestMethod]
    public async Task FindIncompleteAsync_ShouldFindBothRequestedScopes()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal userJournal = CreateJournal(paths, SetupTransactionPhase.Prepared, InstallScope.CurrentUser);
        SetupTransactionJournal machineJournal = CreateJournal(paths, SetupTransactionPhase.Applying, InstallScope.AllUsers);
        await store.SaveAsync(userJournal, CancellationToken.None);
        await store.SaveAsync(machineJournal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        IReadOnlyList<SetupTransactionJournal> found = await coordinator.FindIncompleteAsync(
            "demo-app",
            [InstallScope.CurrentUser, InstallScope.AllUsers],
            CancellationToken.None);

        Assert.AreEqual(2, found.Count);
        CollectionAssert.AreEquivalent(
            new[] { userJournal.OperationId, machineJournal.OperationId },
            found.Select(item => item.OperationId).ToArray());
    }

    [TestMethod]
    public async Task RecoverAsync_ShouldRetainEvidenceOnCompensationFailure_ThenRetryAfterFaultClears()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        string target = Path.Combine(temp.DirectoryPath, "cannot-delete.txt");
        FaultInjectingFileSystem fileSystem = new(physical)
        {
            FailureFactory = (operation, path) => operation == nameof(IFileSystem.DeleteFile) &&
                string.Equals(path, target, StringComparison.OrdinalIgnoreCase)
                    ? new IOException("Injected recovery failure.")
                    : null
        };
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, SetupTransactionPhase.Applying);
        File.WriteAllText(target, "evidence");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        SetupRecoveryResult failed = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsFalse(failed.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, failed.Journal.Phase);
        Assert.IsTrue(failed.Errors.Count > 0);
        Assert.IsTrue(File.Exists(target));
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));

        fileSystem.FailureFactory = null;
        SetupRecoveryResult retried = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(retried.Succeeded);
        Assert.IsFalse(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [TestMethod]
    public async Task RecoverAsync_ShouldKeepRecoveryFailedEvidence_WhenDurableRollbackSaveFails()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        FaultInjectingFileSystem fileSystem = new(physical);
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, SetupTransactionPhase.Applying);
        string target = Path.Combine(temp.DirectoryPath, "durable-save-target.txt");
        File.WriteAllText(target, "mutated");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);
        int rollbackSaveCount = 0;
        fileSystem.FailureFactory = (operation, _) => operation == nameof(IFileSystem.WriteAllTextAtomic) &&
            Interlocked.Increment(ref rollbackSaveCount) == 3
                ? new IOException("Journal persistence failed.")
                : null;

        SetupRecoveryResult result = await CreateCoordinator(store, fileSystem)
            .RecoverAsync(journal, CancellationToken.None);

        Assert.IsFalse(result.Succeeded);
        Assert.AreEqual(SetupTransactionPhase.RecoveryFailed, result.Journal.Phase);
        Assert.IsTrue(result.Errors.Count > 0);
        Assert.IsFalse(File.Exists(target));
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));
    }

    [TestMethod]
    public async Task RecoverAsync_ShouldTreatTerminalCleanupFailureAsWarning_AndRetryOnlyCleanupLater()
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem physical = new();
        Guid operationId = Guid.NewGuid();
        string recoveryDirectory = paths.GetRecoveryDirectory("demo-app", operationId, InstallScope.CurrentUser);
        FaultInjectingFileSystem fileSystem = new(physical)
        {
            FailureFactory = (operation, path) => operation == nameof(IFileSystem.DeleteDirectory) &&
                string.Equals(path, recoveryDirectory, StringComparison.OrdinalIgnoreCase)
                    ? new IOException("Terminal cleanup is temporarily unavailable.")
                    : null
        };
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, SetupTransactionPhase.Applying, operationId: operationId);
        string target = Path.Combine(temp.DirectoryPath, "cleanup-warning-target.txt");
        File.WriteAllText(target, "mutated");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryCoordinator coordinator = CreateCoordinator(store, fileSystem);
        SetupRecoveryResult recovered = await coordinator.RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(recovered.Succeeded);
        Assert.AreEqual(0, recovered.Errors.Count);
        Assert.AreEqual(1, recovered.CleanupWarnings.Count);
        Assert.AreEqual(SetupTransactionPhase.RolledBack, journal.Phase);
        Assert.IsFalse(File.Exists(target));
        Assert.IsTrue(File.Exists(JsonSetupTransactionStore.GetJournalPath(journal)));

        File.WriteAllText(target, "must not replay");
        IReadOnlyList<SetupTransactionJournal> terminals = await coordinator.FindTerminalAsync(
            "demo-app",
            [InstallScope.CurrentUser],
            CancellationToken.None);
        Assert.AreEqual(1, terminals.Count);
        SetupRecoveryResult retryWarning = await coordinator.RecoverAsync(terminals[0], CancellationToken.None);
        Assert.IsTrue(retryWarning.Succeeded);
        Assert.AreEqual(0, retryWarning.Errors.Count);
        Assert.IsTrue(File.Exists(target));

        fileSystem.FailureFactory = null;
        terminals = await coordinator.FindTerminalAsync(
            "demo-app",
            [InstallScope.CurrentUser],
            CancellationToken.None);
        SetupRecoveryResult cleaned = await coordinator.RecoverAsync(terminals[0], CancellationToken.None);

        Assert.IsTrue(cleaned.Succeeded);
        Assert.AreEqual(0, cleaned.CleanupWarnings.Count);
        Assert.IsTrue(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [DataTestMethod]
    [DataRow(SetupTransactionPhase.Committed)]
    [DataRow(SetupTransactionPhase.RolledBack)]
    public async Task RecoverAsync_ShouldCleanupTerminalJournalsWithoutReplayingCompensation(SetupTransactionPhase phase)
    {
        using TempDirectoryScope temp = new();
        TestSystemPaths paths = new(temp.DirectoryPath);
        PhysicalFileSystem fileSystem = new();
        JsonSetupTransactionStore store = new(fileSystem, new JsonManifestSerializer(), paths);
        SetupTransactionJournal journal = CreateJournal(paths, phase);
        string target = Path.Combine(temp.DirectoryPath, "must-not-be-replayed.txt");
        File.WriteAllText(target, "preserve");
        journal.Compensations.Add(new SetupCompensationRecord
        {
            Id = Guid.NewGuid(),
            Kind = SetupCompensationKind.DeleteFile,
            Target = target,
            Applied = true
        });
        await store.SaveAsync(journal, CancellationToken.None);

        SetupRecoveryResult result = await CreateCoordinator(store, fileSystem)
            .RecoverAsync(journal, CancellationToken.None);

        Assert.IsTrue(result.Succeeded);
        Assert.IsTrue(File.Exists(target));
        Assert.IsFalse(Directory.Exists(journal.RecoveryDirectory));
    }

    [TestMethod]
    public void SetupOperationResult_ShouldKeepSucceededCompatibilityAndRecoveryDetails()
    {
        InvalidOperationException primary = new("primary failure");
        SetupOperationResult failed = new()
        {
            Status = SetupOperationStatus.RecoveryFailed,
            FailureCode = SetupFailureCodes.RecoveryFailed,
            PrimaryError = primary,
            RecoveryErrors = ["first recovery failure", "second recovery failure"],
            OperationId = Guid.NewGuid(),
            RecoveryDirectory = "C:\\recovery"
        };

        Assert.IsFalse(failed.Succeeded);
        Assert.AreSame(primary, failed.PrimaryError);
        Assert.AreEqual(2, failed.RecoveryErrors.Count);
        Assert.AreNotEqual(Guid.Empty, failed.OperationId);
        Assert.AreEqual("C:\\recovery", failed.RecoveryDirectory);
        Assert.IsTrue(new SetupOperationResult { Status = SetupOperationStatus.Succeeded }.Succeeded);
    }

    private static SetupRecoveryCoordinator CreateCoordinator(ISetupTransactionStore store, IFileSystem fileSystem)
    {
        return new SetupRecoveryCoordinator(store, fileSystem, new FakeRegistryService(), new FakeShortcutService());
    }

    private static SetupTransactionJournal CreateJournal(
        TestSystemPaths paths,
        SetupTransactionPhase phase,
        InstallScope scope = InstallScope.CurrentUser,
        Guid? operationId = null)
    {
        Guid resolvedOperationId = operationId ?? Guid.NewGuid();
        return new SetupTransactionJournal
        {
            OperationId = resolvedOperationId,
            ProductId = "demo-app",
            Scope = scope,
            Mode = SetupMode.Install,
            InstallDirectory = Path.Combine(paths.RootPath, "install", scope.ToString(), "demo-app"),
            RecoveryDirectory = paths.GetRecoveryDirectory("demo-app", resolvedOperationId, scope),
            Phase = phase,
            StartedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };
    }
}
